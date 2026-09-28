#!/usr/bin/env bash
# Packs a kit package for NuGet.
#
#   nuget-pack.sh <package-dir> <destination>        e.g. nuget-pack.sh srcs/csharp/Sagittaras.Dices artifacts
#
# Packs the Release build as it is (--no-build) — the same assembly the zip archive carries — into the package and its
# symbol package. The kit dependencies become NuGet dependencies at the versions nbgv computes for them, the shared
# metadata comes from srcs/csharp/Directory.Build.props. The NuGet workflow pushes both files to nuget.org unchanged
# from the release assets once the draft is published. Prints the paths of the package and the symbol package, named
# <PackageId>.<version>.nupkg and <PackageId>.<version>.snupkg.
#
# The package must carry exactly the release version: outside a public release ref (see publicReleaseRefSpec in the
# root version.json) nbgv appends the commit id, and such a package is refused.
#
# Expects the solution built in Release. Requires git, nbgv and dotnet on PATH.

set -euo pipefail

root=$(git rev-parse --show-toplevel)
source "$root/.github/scripts/upm-lib.sh"

if [[ $# -ne 2 ]]; then
    echo "Usage: $0 <package-dir> <destination>" >&2
    exit 64
fi

dir=${1%/}
destination=$2
name=${dir##*/}
version=$(package_version "$dir")

work=$(mktemp -d)
trap 'rm -rf "$work"' EXIT

# dotnet reports its progress on stdout; keep stdout for the package paths.
dotnet pack "$dir" --configuration Release --no-build --output "$work" >&2

shopt -s nullglob
packages=("$work"/*.nupkg)
symbols=("$work"/*.snupkg)
if (( ${#packages[@]} != 1 || ${#symbols[@]} != 1 )); then
    echo "dotnet pack produced ${#packages[@]} packages and ${#symbols[@]} symbol packages of $name instead of one each." >&2
    exit 1
fi

if [[ ${packages[0]##*/} != "$name.$version.nupkg" ]]; then
    echo "The NuGet package of $name is ${packages[0]##*/} instead of $name.$version.nupkg — is this a public release ref?" >&2
    exit 1
fi

mkdir -p "$destination"
mv "${packages[0]}" "${symbols[0]}" "$destination/"
echo "$destination/$name.$version.nupkg"
echo "$destination/$name.$version.snupkg"
