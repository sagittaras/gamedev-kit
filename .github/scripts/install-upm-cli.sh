#!/usr/bin/env bash
# Installs the Unity Package Manager CLI (upm) that packs and signs the Unity packages, and puts it on the PATH of the
# following GitHub Actions steps. The version is pinned so a new CLI release never changes signing unnoticed —
# bump it deliberately (the latest one is listed at https://cdn.packages.unity.com/upm-cli/latest.txt).

set -euo pipefail

readonly UPM_CLI_VERSION="v9.32.0"

curl -fsSL https://cdn.packages.unity.com/upm-cli/install.sh | bash -s -- "$UPM_CLI_VERSION"
echo "$HOME/.upm/bin" >> "${GITHUB_PATH:-/dev/null}"
"$HOME/.upm/bin/upm" --version
