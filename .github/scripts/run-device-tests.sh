#!/usr/bin/env bash
set -euo pipefail

# Installs the device test app against a packed FFmpegKit.Net package and runs its smoke tests on
# the emulator that the calling workflow step has already booted. Results are reported to logcat
# under the FFmpegKitNetE2E tag; this script turns them into an exit code.
#
# Usage: run-device-tests.sh [VARIANT] VERSION [TARGET_FRAMEWORK]

VARIANT="${1:-Video}"
VERSION="${2:?a package version is required}"
TARGET_FRAMEWORK="${3:-net10.0-android36.0}"

PACKAGE_NAME="com.sbokatuk.ffmpegkit.net.devicetests"
LOG_FILE="device-tests-logcat.txt"
# CI emulators are x86_64; override when running this against a local arm64 emulator or device.
DEVICE_RID="${FFMPEGKIT_DEVICE_RID:-android-x64}"
POLL_ATTEMPTS=60
POLL_INTERVAL=5

REPO_ROOT="$(cd "$(dirname "$0")/../.." && pwd)"
PROJECT="${REPO_ROOT}/tests/FFmpegKit.Net.DeviceTests/FFmpegKit.Net.DeviceTests.csproj"

# Pick the SDK band that owns the requested target framework's Android API level: API 34 is the
# .NET 8 workload, 35 the .NET 9 one, 36 the .NET 10 one. The API level matters because that is
# what owns the runtime packs - a net8.0-android34.0 app compiles fine on the .NET 9 band and then
# fails at packaging with:
#
#     error NETSDK1112: The runtime pack for Microsoft.Android.Runtime.34.android-x64 was not
#     downloaded. Try running a NuGet restore with the RuntimeIdentifier 'android-x64'.
#
# The restore that error suggests does not help; the packs come from the workload. The SDK is
# resolved from the working directory, and the repository's global.json pins .NET 9, hence the
# scratch directory. Note that run-simulator-tests.sh does *not* do this - see the comment there.
case "${TARGET_FRAMEWORK}" in
    net10.0-*) sdk_major=10 ;;
    net8.0-*)  sdk_major=8 ;;
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

echo "==> installing device tests (variant=${VARIANT}, version=${VERSION}, tfm=${TARGET_FRAMEWORK}, sdk=${sdk_version})"
( cd "${SDK_DIR}" && dotnet build "${PROJECT}" \
    --configuration Release \
    -p:FFmpegKitVariant="${VARIANT}" \
    -p:FFmpegKitPackageVersion="${VERSION}" \
    -p:FFmpegKitDeviceTargetFramework="${TARGET_FRAMEWORK}" \
    -p:RuntimeIdentifier="${DEVICE_RID}" \
    -t:Install )

echo "==> launching"
adb logcat -c
adb shell am start -n "${PACKAGE_NAME}/.MainActivity"

echo "==> waiting for results"
for _ in $(seq 1 "${POLL_ATTEMPTS}"); do
    if adb logcat -d -s "FFmpegKitNetE2E:*" | grep -q "FFMPEGKIT_E2E_DONE"; then
        break
    fi
    sleep "${POLL_INTERVAL}"
done

adb logcat -d -s "FFmpegKitNetE2E:*" | tee "${LOG_FILE}"

if ! grep -q "FFMPEGKIT_E2E_DONE PASS" "${LOG_FILE}"; then
    # No verdict usually means the app died before reporting, so keep the crash trace.
    echo "==> no passing verdict; capturing crash output"
    adb logcat -d -s AndroidRuntime:E DEBUG:F "${PACKAGE_NAME}:*" | tee -a "${LOG_FILE}"
    echo "::error::FFmpegKit.Net device smoke tests failed or timed out"
    exit 1
fi

echo "==> device smoke tests passed"
