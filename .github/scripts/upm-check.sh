#!/usr/bin/env bash
# Checks that every Unity package pre-image matches its package — that the commit could be released as it is.
#
#   upm-check.sh
#
# Runs upm-gate.sh for every kit package with a Unity skeleton and fails when any of them is not distributable.
# Worth running before opening a pull request; the Unity Packages workflow runs it on every pull request to main.
#
# Requires the tools of upm-gate.sh.

set -uo pipefail

root=$(git rev-parse --show-toplevel)
cd "$root"

releasable=true
for manifest in srcs/csharp/*/version.json; do
    dir=${manifest%/version.json}
    name=${dir##*/}

    reason=$(bash .github/scripts/upm-gate.sh "$dir")
    case $? in
        0) echo "$name $reason — pre-image up to date." ;;
        2) ;;
        *) echo "::error::$reason"; releasable=false ;;
    esac
done

[[ $releasable == true ]]
