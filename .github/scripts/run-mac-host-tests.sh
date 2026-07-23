#!/usr/bin/env bash
set -euo pipefail

# Builds the device test app's macOS head against a packed FFmpegKit.Net package and runs its
# smoke tests directly on this Mac - macOS is the target platform, so no simulator or device is
# involved. The app prints its verdict to stdout; this script turns that into an exit code.
#
# Usage: run-mac-host-tests.sh [VARIANT] VERSION [TARGET_FRAMEWORK]

VARIANT="${1:-Video}"
VERSION="${2:?a package version is required}"
TARGET_FRAMEWORK="${3:-net9.0-macos15.0}"

LOG_FILE="mac-host-tests.log"
# CI runners are Apple silicon. Override for an Intel machine.
HOST_RID="${FFMPEGKIT_HOST_RID:-osx-arm64}"

REPO_ROOT="$(cd "$(dirname "$0")/../.." && pwd)"
PROJECT="${REPO_ROOT}/tests/FFmpegKit.Net.DeviceTests/FFmpegKit.Net.DeviceTests.csproj"

# The .NET 9 band builds net8/net9 and the .NET 10 band builds net9/net10, so pick the SDK that
# owns the requested target framework. As on iOS (and unlike Android), there is no net8 ->
# .NET 8 band mapping: the .NET 9 band's macOS workload builds net8.0-macos outright.
case "${TARGET_FRAMEWORK}" in
    net10.0-*) sdk_major=10 ;;
    *)         sdk_major=9 ;;
esac

sdk_version="$(dotnet --list-sdks | grep "^${sdk_major}\." | tail -1 | cut -d' ' -f1)"
if [ -z "${sdk_version}" ]; then
    echo "::error::no .NET ${sdk_major} SDK installed, cannot build ${TARGET_FRAMEWORK}"
    exit 1
fi

SDK_DIR="$(mktemp -d)"
trap 'rm -rf "${SDK_DIR}"' EXIT
printf '{ "sdk": { "version": "%s", "rollForward": "latestFeature" } }\n' "${sdk_version}" \
    > "${SDK_DIR}/global.json"

# NuGet caches by package id + version, so rebuilding a version that was already restored once
# silently reuses the stale copy. CI versions are unique, but locally you will re-pack the same
# version repeatedly and test yesterday's bits without this.
package_id="ffmpegkit.net.$(printf '%s' "${VARIANT}" | tr '[:upper:]' '[:lower:]')"
rm -rf "${HOME}/.nuget/packages/${package_id}/${VERSION}"

echo "==> building host tests (variant=${VARIANT}, version=${VERSION}, tfm=${TARGET_FRAMEWORK}, sdk=${sdk_version})"
( cd "${SDK_DIR}" && dotnet build "${PROJECT}" \
    --configuration Release \
    -p:FFmpegKitVariant="${VARIANT}" \
    -p:FFmpegKitPackageVersion="${VERSION}" \
    -p:FFmpegKitDeviceTargetFramework="${TARGET_FRAMEWORK}" \
    -p:RuntimeIdentifier="${HOST_RID}" )

APP_PATH="$(find "${REPO_ROOT}/tests/FFmpegKit.Net.DeviceTests/bin/Release/${TARGET_FRAMEWORK}/${HOST_RID}" \
    -maxdepth 1 -name '*.app' -print -quit)"
if [ -z "${APP_PATH}" ]; then
    echo "::error::no .app bundle was produced"
    exit 1
fi

EXECUTABLE="$(find "${APP_PATH}/Contents/MacOS" -maxdepth 1 -type f -perm +111 -print -quit)"
if [ -z "${EXECUTABLE}" ]; then
    echo "::error::no executable inside ${APP_PATH}"
    exit 1
fi

echo "==> running ${EXECUTABLE}"
# The app prints FFMPEGKIT_E2E_DONE and exits itself; the timeout is a backstop against a hang.
set +e
if command -v timeout >/dev/null 2>&1; then
    timeout 600 "${EXECUTABLE}" 2>&1 | tee "${LOG_FILE}"
else
    "${EXECUTABLE}" 2>&1 | tee "${LOG_FILE}"
fi
set -e

if ! grep -q "FFMPEGKIT_E2E_DONE PASS" "${LOG_FILE}"; then
    echo "::error::FFmpegKit.Net macOS host smoke tests failed or timed out"
    exit 1
fi

echo "==> macOS host smoke tests passed"
