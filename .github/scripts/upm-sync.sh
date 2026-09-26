#!/usr/bin/env bash
# Publishes Unity package pre-images for OpenUPM, which builds packages from git tags.
#
#   upm-sync.sh
#
# Every package with a Unity skeleton owns an orphan branch upm/<slug> that holds just the package, one commit per
# version, tagged upm/<slug>/<version>. The script is a sync, so running it again is harmless: a package is published
# when its version computed at the current commit
#   - has a published GitHub release (tag <slug>/<version>) and no upm/<slug>/<version> tag yet,
#   - passes upm-gate.sh — the committed pre-image is verified and fresh,
#   - has every kit dependency on OpenUPM already; dependencies published in the same run count, as the sync
#     repeats its passes until nothing more can be published.
# The published package.json gets the version and the kit dependencies (from the csproj ProjectReferences, at their
# current versions); dependencies written in the skeleton by hand (Unity packages) are kept.
#
# Requires git with a configured identity and push access to origin, nbgv and pwsh on PATH.

set -euo pipefail

root=$(git rev-parse --show-toplevel)
cd "$root"
gate="$root/.github/scripts/upm-gate.sh"
source_commit=$(git rev-parse HEAD)

package_slug() {
    local name=${1##*/}
    echo "${name#Sagittaras.}" | sed -E 's/([a-z0-9])([A-Z])/\1-\2/g' | tr '[:upper:]' '[:lower:]'
}

package_version() {
    nbgv get-version --project "$1" --variable SimpleVersion
}

has_tag() {
    git rev-parse --quiet --verify "refs/tags/$1" > /dev/null
}

# Kit dependencies of a package as "<package-name>=<version>" lines.
kit_dependencies() {
    local dir=$1 ref dep_dir
    while IFS= read -r ref; do
        dep_dir="$dir/$(dirname "${ref//\\//}")"
        echo "com.sagittaras.gamedevkit.$(package_slug "$dep_dir")=$(package_version "$dep_dir")"
    done < <(sed -n 's/.*<ProjectReference Include="\([^"]*\)".*/\1/p' "$dir/${dir##*/}.csproj")
}

# Copies the skeleton's package.json with the release version and the kit dependencies filled in.
write_manifest() {
    SOURCE=$1 TARGET=$2 VERSION=$3 DEPENDENCIES=$4 pwsh -NoProfile -Command '
        $manifest = Get-Content -Raw $env:SOURCE | ConvertFrom-Json
        $manifest.version = $env:VERSION
        $dependencies = [ordered]@{}
        if ($manifest.PSObject.Properties["dependencies"]) {
            $manifest.dependencies.PSObject.Properties | ForEach-Object { $dependencies[$_.Name] = $_.Value }
        }
        foreach ($line in $env:DEPENDENCIES -split "\n" | Where-Object { $_ }) {
            $name, $version = $line -split "=", 2
            $dependencies[$name] = $version
        }
        if ($dependencies.Count -gt 0) {
            $manifest | Add-Member -Force -NotePropertyName dependencies -NotePropertyValue $dependencies
        }
        $manifest | ConvertTo-Json -Depth 10 | Set-Content $env:TARGET'
}

publish() {
    local dir=$1 dependencies=$2
    local name=${dir##*/} slug version skeleton branch tag work
    slug=$(package_slug "$name")
    version=$(package_version "$dir")
    skeleton="srcs/unity/Packages/com.sagittaras.gamedevkit.$slug"
    branch="upm/$slug"
    tag="upm/$slug/$version"
    work=$(mktemp -d)/package

    if git fetch --quiet origin "+refs/heads/$branch:refs/remotes/origin/$branch" 2> /dev/null; then
        git worktree add --quiet -B "$branch" "$work" "origin/$branch"
        git -C "$work" rm -rq --ignore-unmatch .
    else
        git worktree add --quiet --orphan -b "$branch" "$work"
    fi

    cp -R "$skeleton/." "$work/"
    write_manifest "$skeleton/package.json" "$work/package.json" "$version" "$dependencies"

    git -C "$work" add -A
    git -C "$work" commit --quiet -m "$name $version" -m "Built from $source_commit."
    git -C "$work" tag "$tag"
    # Atomic: the tag marks the version as published, so it must never land without its branch (or vice versa).
    git -C "$work" push --quiet --atomic origin "refs/heads/$branch" "refs/tags/$tag"
    git worktree remove --force "$work"

    echo "::notice::Published $name $version to OpenUPM as com.sagittaras.gamedevkit.$slug ($tag)."
}

pending=()
for manifest in srcs/csharp/*/version.json; do
    dir=${manifest%/version.json}
    name=${dir##*/}
    slug=$(package_slug "$name")
    [[ -f srcs/unity/Packages/com.sagittaras.gamedevkit.$slug/package.json ]] || continue

    version=$(package_version "$dir")
    if has_tag "upm/$slug/$version"; then
        echo "::notice::$name $version is already on OpenUPM."
    elif ! has_tag "$slug/$version"; then
        echo "::notice::$name $version has no published release — skipping."
    else
        pending+=("$dir")
    fi
done

while (( ${#pending[@]} > 0 )); do
    waiting=()
    for dir in "${pending[@]}"; do
        name=${dir##*/}
        dependencies=$(kit_dependencies "$dir")

        missing=""
        while IFS='=' read -r dep_name dep_version; do
            [[ -n $dep_name ]] || continue
            has_tag "upm/${dep_name#com.sagittaras.gamedevkit.}/$dep_version" || missing+=" $dep_name@$dep_version"
        done <<< "$dependencies"
        if [[ -n $missing ]]; then
            waiting+=("$dir")
            continue
        fi

        if ! reason=$(bash "$gate" "$dir"); then
            echo "::warning::$name is not published to OpenUPM: $reason"
            continue
        fi

        publish "$dir" "$dependencies"
    done

    if (( ${#waiting[@]} == ${#pending[@]} )); then
        for dir in "${waiting[@]}"; do
            echo "::warning::${dir##*/} is not published to OpenUPM: its kit dependencies are not on OpenUPM at their current versions."
        done
        break
    fi
    pending=("${waiting[@]}")
done
