#!/usr/bin/env bash
# Builds the self-contained Windows x64 single-file executable from any host OS.
# The flag set is locked by ADR-005 — do not edit without revising the ADR.
# PublishTrimmed=false is mandatory because DocumentFormat.OpenXml uses reflection.
set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
SOLUTION_DIR="$(cd "${SCRIPT_DIR}/.." && pwd)"

cd "${SOLUTION_DIR}"

dotnet publish DocFormatter.Cli \
    -c Release \
    -r win-x64 \
    --self-contained true \
    -p:PublishSingleFile=true \
    -p:IncludeNativeLibrariesForSelfExtract=true \
    -p:PublishReadyToRun=true \
    -p:PublishTrimmed=false

ARTIFACT="${SOLUTION_DIR}/DocFormatter.Cli/bin/Release/net10.0/win-x64/publish/docformatter.exe"

if [[ ! -f "${ARTIFACT}" ]]; then
    echo "ERROR: expected artifact not found at ${ARTIFACT}" >&2
    exit 1
fi

ARTIFACT_BYTES=$(stat -f%z "${ARTIFACT}" 2>/dev/null || stat -c%s "${ARTIFACT}")
ARTIFACT_MB=$((ARTIFACT_BYTES / 1024 / 1024))

echo "Artifact: ${ARTIFACT}"
echo "Size:     ${ARTIFACT_MB} MB (${ARTIFACT_BYTES} bytes)"
