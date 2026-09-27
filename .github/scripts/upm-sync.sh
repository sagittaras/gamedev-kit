#!/usr/bin/env bash
# Attaches the signed Unity package to releases that went out without it — the catch-up for a version released while
# its pre-image was stale (release.yml signs and attaches it itself whenever the pre-image passes the gate).
#
#   upm-sync.sh
#
# A package gets its signed tarball attached when its version computed at the current commit
#   - has a release <slug>/<version>, draft or published, without the tarball yet,
#   - passes upm-gate.sh — the committed pre-image is verified and fresh,
#   - has every kit dependency's release carrying its tarball already; dependencies attached in the same run count,
#     as the sync repeats its passes until nothing more can be attached.
# Attach to the draft before publishing it: OpenUPM finds the version by its tag, which appears on publishing, and
# waits for the asset of a published release only for 3 days. The sync is idempotent, so running it again is harmless.
#
# Requires the tools of upm-gate.sh and upm-pack.sh, and gh authenticated with write access to the releases.

set -euo pipefail

root=$(git rev-parse --show-toplevel)
cd "$root"
source "$root/.github/scripts/upm-lib.sh"
dist=$(mktemp -d)
trap 'rm -rf "$dist"' EXIT

pending=()
for manifest in srcs/csharp/*/version.json; do
    dir=${manifest%/version.json}
    name=${dir##*/}
    [[ -f $(package_skeleton "$name")/package.json ]] || continue

    version=$(package_version "$dir")
    status=0
    has_signed_tarball "$(package_slug "$name")" "$version" || status=$?
    case $status in
        0) echo "::notice::$name $version already carries its signed Unity package." ;;
        1) pending+=("$dir") ;;
        2) echo "::notice::$name $version has no release — skipping." ;;
        *) echo "::error::The release of $name $version could not be looked up."; exit 1 ;;
    esac
done

while (( ${#pending[@]} > 0 )); do
    waiting=()
    for dir in "${pending[@]}"; do
        name=${dir##*/}
        slug=$(package_slug "$name")
        version=$(package_version "$dir")

        ready=true
        while IFS= read -r dep_dir; do
            has_signed_tarball "$(package_slug "$dep_dir")" "$(package_version "$dep_dir")" || ready=false
        done < <(kit_dependencies "$dir")
        if [[ $ready != true ]]; then
            waiting+=("$dir")
            continue
        fi

        if ! reason=$(bash .github/scripts/upm-gate.sh "$dir"); then
            echo "::warning::$name $version gets no signed Unity package: $reason"
            continue
        fi

        if ! tarball=$(bash .github/scripts/upm-pack.sh "$dir" "$dist"); then
            echo "::warning::$name $version could not be signed."
            continue
        fi

        gh release upload "$slug/$version" "$tarball"
        echo "::notice::Attached the signed Unity package of $name $version to its release $slug/$version."
    done

    if (( ${#waiting[@]} == ${#pending[@]} )); then
        for dir in "${waiting[@]}"; do
            echo "::warning::${dir##*/} gets no signed Unity package: the releases of its kit dependencies don't carry theirs."
        done
        break
    fi
    pending=("${waiting[@]}")
done
