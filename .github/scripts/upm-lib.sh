#!/usr/bin/env bash
# Shared helpers of the Unity package scripts (upm-gate.sh, upm-pack.sh, upm-sync.sh) — sourced, not run.
# Package directories are the srcs/csharp/Sagittaras.<Package> folders; helpers needing the repository root use $root.

readonly UPM_PACKAGE_PREFIX="com.sagittaras.gamedevkit."

# Slug of a package, e.g. Sagittaras.GuardClauses -> guard-clauses — the same one release tags use.
package_slug() {
    local name=${1##*/}
    echo "${name#Sagittaras.}" | sed -E 's/([a-z0-9])([A-Z])/\1-\2/g' | tr '[:upper:]' '[:lower:]'
}

# Unity package skeleton (pre-image) of a package.
package_skeleton() {
    echo "$root/srcs/unity/Packages/$UPM_PACKAGE_PREFIX$(package_slug "$1")"
}

# Release version of a package — the one its <slug>/<version> release tag carries.
package_version() {
    nbgv get-version --project "$1" --variable SimpleVersion
}

# Directories of the kit packages a package references, one per line.
kit_dependencies() {
    local dir=${1%/} ref
    while IFS= read -r ref; do
        echo "$dir/$(dirname "${ref//\\//}")"
    done < <(sed -n 's/.*<ProjectReference Include="\([^"]*\)".*/\1/p' "$dir/${dir##*/}.csproj")
}

# File name of the signed Unity package of a package version, as attached to its release.
tarball_name() {
    echo "$UPM_PACKAGE_PREFIX$(package_slug "$1")-$2.tgz"
}

# Asset names of a release (drafts included), one per line. Returns 2 when the release does not exist,
# 3 when the lookup itself fails.
release_assets() {
    local output
    if output=$(gh release view "$1" --json assets --jq '.assets[].name' 2>&1); then
        [[ -z $output ]] || echo "$output"
        return 0
    fi

    if [[ $output == *"release not found"* ]]; then
        return 2
    fi

    echo "$output" >&2
    return 3
}

# Whether the release of a package version (slug, version) carries its signed Unity package:
# 0 yes, 1 no, 2 there is no release, 3 the lookup failed.
has_signed_tarball() {
    local assets status=0
    assets=$(release_assets "$1/$2") || status=$?
    if (( status != 0 )); then
        return "$status"
    fi

    grep -qxF "$UPM_PACKAGE_PREFIX$1-$2.tgz" <<< "$assets"
}
