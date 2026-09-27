#!/usr/bin/env bash
# Packs and signs the Unity package of a kit package for OpenUPM.
#
#   upm-pack.sh <package-dir> <destination>        e.g. upm-pack.sh srcs/csharp/Sagittaras.Dices artifacts
#
# Takes the committed pre-image (run upm-gate.sh first — this script does not judge whether it is distributable),
# fills in the package.json the skeleton leaves open — the version computed by nbgv and the kit dependencies from the
# csproj ProjectReferences at their current versions; dependencies written by hand (Unity packages) are kept — and
# runs `upm pack`, which signs the tarball with the organization's credentials. OpenUPM publishes that tarball
# unchanged from the release asset, so the signature survives. Prints the path of the signed tarball, named
# com.sagittaras.gamedevkit.<slug>-<version>.tgz.
#
# Requires git, nbgv, pwsh, tar and upm (see install-upm-cli.sh) on PATH, and the signing credentials in
# UPM_ORG_ID, UPM_SERVICE_ACCOUNT_KEY_ID and UPM_SERVICE_ACCOUNT_KEY_SECRET.

set -euo pipefail

root=$(git rev-parse --show-toplevel)
source "$root/.github/scripts/upm-lib.sh"

if [[ $# -ne 2 ]]; then
    echo "Usage: $0 <package-dir> <destination>" >&2
    exit 64
fi

if [[ -z ${UPM_ORG_ID:-} ]]; then
    echo "UPM_ORG_ID is not set — the package cannot be signed." >&2
    exit 1
fi

dir=${1%/}
destination=$2
name=${dir##*/}
version=$(package_version "$dir")
skeleton=$(package_skeleton "$name")

work=$(mktemp -d)
trap 'rm -rf "$work"' EXIT
mkdir -p "$work/package" "$work/signed"
cp -R "$skeleton/." "$work/package/"

dependencies=""
while IFS= read -r dep_dir; do
    dependencies+="$UPM_PACKAGE_PREFIX$(package_slug "$dep_dir")=$(package_version "$dep_dir")"$'\n'
done < <(kit_dependencies "$dir")

SOURCE="$skeleton/package.json" TARGET="$work/package/package.json" VERSION="$version" DEPENDENCIES="$dependencies" \
    pwsh -NoProfile -Command '
        $manifest = Get-Content -Raw $env:SOURCE | ConvertFrom-Json
        $manifest.version = $env:VERSION
        $dependencies = [ordered]@{}
        if ($manifest.PSObject.Properties["dependencies"]) {
            $manifest.dependencies.PSObject.Properties | ForEach-Object { $dependencies[$_.Name] = $_.Value }
        }
        foreach ($line in $env:DEPENDENCIES -split "\n" | Where-Object { $_ }) {
            $package, $packageVersion = $line -split "=", 2
            $dependencies[$package] = $packageVersion
        }
        if ($dependencies.Count -gt 0) {
            $manifest | Add-Member -Force -NotePropertyName dependencies -NotePropertyValue $dependencies
        }
        $manifest | ConvertTo-Json -Depth 10 | Set-Content $env:TARGET'

# upm reports its progress on stdout; keep stdout for the tarball path.
upm pack "$work/package" --organization-id "$UPM_ORG_ID" --destination "$work/signed" >&2

shopt -s nullglob
archives=("$work"/signed/*.tgz "$work"/signed/*.tar.gz)
if (( ${#archives[@]} != 1 )); then
    echo "upm pack produced ${#archives[@]} archives instead of one." >&2
    exit 1
fi

contents=$(tar -tzf "${archives[0]}")
for required in package/package.json package/.attestation.p7m; do
    if ! grep -qxF "$required" <<< "$contents"; then
        echo "The signed archive of $name is missing $required." >&2
        exit 1
    fi
done

packed=$(tar -xOzf "${archives[0]}" package/package.json | pwsh -NoProfile -Command '($input | Out-String | ConvertFrom-Json).version')
if [[ $packed != "$version" ]]; then
    echo "The signed archive of $name holds version $packed instead of $version." >&2
    exit 1
fi

mkdir -p "$destination"
tarball="$destination/$(tarball_name "$name" "$version")"
mv "${archives[0]}" "$tarball"
echo "$tarball"
