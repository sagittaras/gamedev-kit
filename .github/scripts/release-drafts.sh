#!/usr/bin/env bash
# Drafts a release for every kit package whose computed version has none yet — for all of them, or for none.
#
#   release-drafts.sh <artifacts-dir>
#
# A package is released when its version (nbgv, see its version.json) has neither a <slug>/<version> tag nor a draft.
# Every release carries its NuGet package with the symbol package, which the NuGet workflow pushes to nuget.org once
# the draft is published, and the zip archive — the package's DLL, PDB and XML plus the DLLs of its kit dependencies,
# for Assets/Plugins/ or a .NET project without NuGet. A package with a Unity skeleton carries its signed Unity package
# as well, which OpenUPM publishes once the draft is published.
#
# The Unity packages are the precondition of a release: when any Unity package due for release is not distributable
# (upm-gate.sh) or cannot be signed, nothing is drafted at all. Refresh the pre-images, verify them in Unity, commit
# Plugins/ and run the release again — committing Plugins/ keeps the versions.
#
# Expects the solution built in Release. Requires the tools of upm-gate.sh, upm-pack.sh and nuget-pack.sh, zip, and gh
# with write access to the releases.

set -euo pipefail

root=$(git rev-parse --show-toplevel)
cd "$root"
source "$root/.github/scripts/upm-lib.sh"

if [[ $# -ne 1 ]]; then
    echo "Usage: $0 <artifacts-dir>" >&2
    exit 64
fi

mkdir -p "$1"
artifacts=$(cd "$1" && pwd)
notes=$(mktemp)
trap 'rm -f "$notes"' EXIT

has_unity_package() {
    [[ -f $(package_skeleton "$1")/package.json ]]
}

drafts=$(gh release list --limit 100 --json tagName,isDraft --jq '.[] | select(.isDraft) | .tagName')

packages=()
for manifest in srcs/csharp/*/version.json; do
    dir=${manifest%/version.json}
    name=${dir##*/}
    version=$(package_version "$dir")
    tag="$(package_slug "$name")/$version"

    if git rev-parse --quiet --verify "refs/tags/$tag" > /dev/null || grep -qxF "$tag" <<< "$drafts"; then
        echo "::notice::$name $version is already released — skipping."
        continue
    fi

    packages+=("$dir")
done

if (( ${#packages[@]} == 0 )); then
    echo "::notice::Every package is released — nothing to draft."
    exit 0
fi

# The precondition, checked for every package before anything is built: one stale pre-image stops the whole release
# instead of leaving some packages drafted and some not.
distributable=true
for dir in "${packages[@]}"; do
    name=${dir##*/}
    if ! has_unity_package "$name"; then
        echo "::notice::$name has no Unity package — its release carries the zip archive only."
        continue
    fi

    if ! reason=$(bash .github/scripts/upm-gate.sh "$dir"); then
        echo "::error::$reason"
        distributable=false
    fi
done

if [[ $distributable != true ]]; then
    echo "::error::Nothing was drafted. Refresh the Unity package pre-images (Tools → Sagittaras → Build Pre-image), verify them in Unity, commit Plugins/ and run the release again."
    exit 1
fi

# The assets of every release, before the first draft — a failed pack or signature stops the release as a whole, too.
declare -A assets
for dir in "${packages[@]}"; do
    name=${dir##*/}
    version=$(package_version "$dir")

    archive="$artifacts/$name-$version.zip"
    (cd "$dir/bin/Release/netstandard2.1" && zip -q "$archive" ./*.dll ./*.pdb ./*.xml)
    zip -qj "$archive" LICENSE
    assets[$dir]=$archive

    if ! nuget=$(bash .github/scripts/nuget-pack.sh "$dir" "$artifacts"); then
        echo "::error::$name $version could not be packed for NuGet — nothing was drafted."
        exit 1
    fi
    assets[$dir]+=$'\n'$nuget

    if has_unity_package "$name"; then
        if ! tarball=$(bash .github/scripts/upm-pack.sh "$dir" "$artifacts"); then
            echo "::error::$name $version could not be signed — nothing was drafted."
            exit 1
        fi
        assets[$dir]+=$'\n'$tarball
    fi
done

for dir in "${packages[@]}"; do
    name=${dir##*/}
    slug=$(package_slug "$name")
    version=$(package_version "$dir")

    previous=$(git tag --list "$slug/*" --sort=-v:refname | head -n 1)
    if [[ -n $previous ]]; then
        mapfile -t paths < <(pwsh -NoProfile -Command "(Get-Content -Raw '$dir/version.json' | ConvertFrom-Json).pathFilters" | tr -d '\r')
        {
            echo "## Changes since ${previous#"$slug/"}"
            echo
            git -C "$dir" log --no-merges --pretty='- %s (%h)' "$previous..HEAD" -- "${paths[@]}"
        } > "$notes"
    else
        echo "First independently versioned release of $name." > "$notes"
    fi

    mapfile -t files <<< "${assets[$dir]}"
    gh release create "$slug/$version" "${files[@]}" \
        --draft \
        --latest=false \
        --target "${GITHUB_SHA:-$(git rev-parse HEAD)}" \
        --title "$name $version" \
        --notes-file "$notes"
    echo "::notice::Drafted $name $version."
done
