#!/usr/bin/env bash
# Unity package gate — decides whether a kit package may be distributed through OpenUPM at the current commit.
#
#   upm-gate.sh <package-dir>        e.g. upm-gate.sh srcs/csharp/Sagittaras.Dices
#
# The Unity package is the committed pre-image in srcs/unity/Packages/com.sagittaras.gamedevkit.<slug>/, verified
# in Unity by a human. It is distributable only when:
#   1. every file of the skeleton has its .meta — Unity has imported it,
#   2. the DLL in Plugins/ carries the version nbgv computes for the package — nothing touching the package changed
#      since the pre-image was built (Plugins/ is excluded from pathFilters, so committing it keeps the version),
#   3. every kit dependency at its current version is on OpenUPM — its <slug>/<version> release carries the signed
#      Unity package — or, when that version is not released yet, passes this gate itself, so it gets signed in the
#      same release run. Otherwise the package.json would depend on a version that never reaches OpenUPM.
#
# Exit code: 0 distributable (prints the version), 1 not distributable (prints the reason),
#            2 the package has no Unity skeleton, so it is not a Unity package at all.
# Requires git, nbgv, gh and pwsh on PATH (all present on GitHub-hosted runners once nbgv is installed).

set -uo pipefail

root=$(git rev-parse --show-toplevel)
source "$root/.github/scripts/upm-lib.sh"

assembly_version() {
    local version
    version=$(pwsh -NoProfile -Command "[System.Diagnostics.FileVersionInfo]::GetVersionInfo('$1').ProductVersion") || return 1
    echo "${version%%+*}"
}

informational_version() {
    local version
    version=$(nbgv get-version --project "$1" --variable AssemblyInformationalVersion) || return 1
    echo "${version%%+*}"
}

check() {
    local dir=${1%/}
    local name=${dir##*/}
    local skeleton version dll built file dep_dir dep_name dep_version status reason
    skeleton=$(package_skeleton "$name")

    if [[ ! -f $skeleton/package.json ]]; then
        echo "$name has no Unity package skeleton."
        return 2
    fi

    while IFS= read -r file; do
        if [[ ! -f $file.meta ]]; then
            echo "$name pre-image is missing ${file#"$root/"}.meta — open it in Unity before committing."
            return 1
        fi
    done < <(find "$skeleton" -mindepth 1 ! -name '*.meta')

    version=$(informational_version "$dir") || { echo "$name version could not be computed."; return 1; }
    dll="$skeleton/Plugins/$name.dll"
    if [[ ! -f $dll ]]; then
        echo "$name pre-image has no Plugins/$name.dll."
        return 1
    fi

    built=$(assembly_version "$dll") || { echo "$name pre-image DLL version could not be read."; return 1; }
    if [[ $built != "$version" ]]; then
        echo "$name pre-image holds $built, but the package is at $version — rebuild it with -p:UpdateUnityPackage=true."
        return 1
    fi

    while IFS= read -r dep_dir; do
        dep_name=${dep_dir##*/}
        dep_version=$(package_version "$dep_dir") || { echo "$dep_name version could not be computed."; return 1; }

        status=0
        has_signed_tarball "$(package_slug "$dep_name")" "$dep_version" || status=$?
        case $status in
            0)
                continue
                ;;
            1)
                echo "$name depends on $dep_name $dep_version, which was released without its signed Unity package — attach it with the OpenUPM workflow first."
                return 1
                ;;
            2)
                if ! reason=$(check "$dep_dir"); then
                    echo "$name depends on $dep_name $dep_version, which is not distributable: $reason"
                    return 1
                fi
                ;;
            *)
                echo "$name depends on $dep_name $dep_version, whose release could not be looked up."
                return 1
                ;;
        esac
    done < <(kit_dependencies "$dir")

    echo "$version"
}

if [[ $# -ne 1 ]]; then
    echo "Usage: $0 <package-dir>" >&2
    exit 64
fi

check "$1"
