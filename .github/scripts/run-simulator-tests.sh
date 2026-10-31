#!/usr/bin/env bash
set -euo pipefail

# Builds the device test app against a packed FFmpegKit.Net package, installs it on an iOS
# simulator and runs its smoke tests. The app prints its verdict to stdout; this script turns that
# into an exit code.
#
# Usage: run-simulator-tests.sh [VARIANT] VERSION [TARGET_FRAMEWORK]

VARIANT="${1:-Video}"
VERSION="${2:?a package version is required}"
TARGET_FRAMEWORK="${3:-net10.0-ios26.0}"

BUNDLE_ID="com.sbokatuk.ffmpegkit.net.devicetests"
# The name the simulator's unified log files the app's lines under.
APP_PROCESS="FFmpegKit.Net.DeviceTests"
LOG_FILE="simulator-tests.log"
# CI runners are Apple silicon. Override for an Intel runner, whose simulator is x64.
SIMULATOR_RID="${FFMPEGKIT_SIMULATOR_RID:-iossimulator-arm64}"
SIMULATOR_DEVICE="${FFMPEGKIT_SIMULATOR_DEVICE:-iPhone 17}"

REPO_ROOT="$(cd "$(dirname "$0")/../.." && pwd)"
PROJECT="${REPO_ROOT}/tests/FFmpegKit.Net.DeviceTests/FFmpegKit.Net.DeviceTests.csproj"

# The .NET 9 band builds net8/net9 and the .NET 10 band builds net9/net10, so pick the SDK that
# owns the requested target framework. Unlike run-device-tests.sh there is no net8 -> .NET 8 band
# mapping and that is not an oversight: on Android the API level in the target framework decides
# which workload owns the runtime packs, and API 34 belongs to .NET 8. Here the iOS SDK version
# does not move with the target framework the same way - the .NET 9 band's iOS workload builds
# net8.0-ios18.0 outright, runtime packs included, and building it on the .NET 8 band would
# additionally pin the runner to Xcode 16 while everything else needs Xcode 26. The SDK is
# resolved from the working directory, and the repository's global.json pins .NET 9, hence the
# scratch directory.
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

# The app's own intermediate output has to go too, not just the NuGet cache: the platform
# binding's native payload is extracted into obj/ and copied into the .app, and neither step
# re-runs when the package version string is unchanged.
rm -rf "${REPO_ROOT}/tests/FFmpegKit.Net.DeviceTests/obj" \
       "${REPO_ROOT}/tests/FFmpegKit.Net.DeviceTests/bin"

echo "==> building device tests (variant=${VARIANT}, version=${VERSION}, tfm=${TARGET_FRAMEWORK}, sdk=${sdk_version})"
# Debug, not Release. A Release simulator build AOT-compiles every assembly in the app - several
# hundred BCL assemblies plus ~100 MB of statically linked FFmpeg through the trimmer - which
# runs 20+ minutes on a CI runner before the first check executes. Nothing is lost: this suite
# verifies that the packed package resolves the right binding and that the cross-platform API
# drives real FFmpeg - none of which AOT affects. The sample build covers a consumer-shaped
# compile against the same packages.
( cd "${SDK_DIR}" && dotnet build "${PROJECT}" \
    --configuration Debug \
    -p:FFmpegKitVariant="${VARIANT}" \
    -p:FFmpegKitPackageVersion="${VERSION}" \
    -p:FFmpegKitDeviceTargetFramework="${TARGET_FRAMEWORK}" \
    -p:RuntimeIdentifier="${SIMULATOR_RID}" )

APP_PATH="$(find "${REPO_ROOT}/tests/FFmpegKit.Net.DeviceTests/bin/Debug/${TARGET_FRAMEWORK}/${SIMULATOR_RID}" \
    -maxdepth 1 -name '*.app' -print -quit)"
if [ -z "${APP_PATH}" ]; then
    echo "::error::no .app bundle was produced"
    exit 1
fi

echo "==> selecting simulator"
# Prefer the requested device, but fall back to any available iPhone rather than failing: which
# device names exist depends on the installed Xcode, and pinning one couples this script to a
# runner image that changes without notice. Newest runtime first, so the picked device is the
# most current one available.
# The device name goes to python through the environment rather than being interpolated into the
# script text: macOS still ships bash 3.2, so the ${VAR@Q} quoting operator is unavailable, and
# an unquoted name containing a space would corrupt the program.
selection="$(xcrun simctl list devices available --json \
    | FFMPEGKIT_PREFERRED_DEVICE="${SIMULATOR_DEVICE}" python3 -c "
import json, os, sys

preferred = os.environ['FFMPEGKIT_PREFERRED_DEVICE']
runtimes = json.load(sys.stdin)['devices']

def candidates():
    for runtime in sorted(runtimes, reverse=True):
        for device in runtimes[runtime]:
            yield device

for device in candidates():
    if device['name'] == preferred:
        print(device['udid'], device['name'], sep='\t')
        raise SystemExit

for device in candidates():
    if device['name'].startswith('iPhone'):
        print(device['udid'], device['name'], sep='\t')
        raise SystemExit
")"

udid="${selection%%$'\t'*}"
device_name="${selection#*$'\t'}"

if [ -z "${udid}" ]; then
    echo "::error::no available iPhone simulator to run on"
    xcrun simctl list devices available
    exit 1
fi

if [ "${device_name}" != "${SIMULATOR_DEVICE}" ]; then
    echo "==> '${SIMULATOR_DEVICE}' is not available, using '${device_name}'"
fi

echo "==> booting ${device_name} (${udid})"

xcrun simctl boot "${udid}" 2>/dev/null || true
xcrun simctl bootstatus "${udid}" -b

echo "==> installing"
xcrun simctl install "${udid}" "${APP_PATH}"

echo "==> running"
# --console-pty streams the app's stdout straight back, so the verdict rarely needs log scraping.
# The app terminates itself once it has printed FFMPEGKIT_E2E_DONE.
#
# The unified log also holds earlier runs of the app, and a stale PASS from one of those must not
# stand in for this run, so every query below starts at this launch. A unix time, because `log show`
# reads its other formats in the simulator's time zone.
launch_started="$(date +%s)"
set +e
xcrun simctl launch --console-pty "${udid}" "${BUNDLE_ID}" 2>&1 | tee "${LOG_FILE}"
set -e

# Prints PASS or FAIL for the last verdict on stdin, or nothing when there is none. -a because the
# console stream is raw terminal output, and grep answers -o with "Binary file matches" if it
# decides that is not text.
read_verdict() {
    { grep -aEo "FFMPEGKIT_E2E_DONE (PASS|FAIL)" || true; } | tail -1 | cut -d' ' -f2
}

verdict="$(read_verdict < "${LOG_FILE}")"

# simctl can stop relaying when the app exits, with the last lines still undelivered. DatadogNet run
# 41110146674 printed a passing verdict, but the console stream ended half a second of output before
# it and was running two seconds behind the app, so a short wait in the app would not be enough.
# That is a lost report rather than a failed run, and the unified log still has it. The PASS and
# FAIL lines come too, so the uploaded log says which check failed. The retries only cost time on a
# run that is failing anyway, and a query that came too early would bring the false failure back.
if [ -z "${verdict}" ]; then
    echo "==> no verdict on the console stream; reading the device log"
    for _ in 1 2 3; do
        xcrun simctl spawn "${udid}" log show --start "@${launch_started}" --predicate \
            "process == '${APP_PROCESS}' AND (eventMessage BEGINSWITH 'PASS ' OR eventMessage BEGINSWITH 'FAIL ' OR eventMessage BEGINSWITH 'FFMPEGKIT_E2E_DONE ')" \
            2>/dev/null | tee -a "${LOG_FILE}" || true
        verdict="$(read_verdict < "${LOG_FILE}")"
        [ -z "${verdict}" ] || break
        sleep 5
    done

    if [ -n "${verdict}" ]; then
        echo "::warning::the console stream lost the verdict, so it was read from the device log"
    fi
fi

if [ "${verdict}" != "PASS" ]; then
    # No verdict usually means the app died before reporting, so keep the crash trace.
    echo "==> no passing verdict; capturing crash output"
    xcrun simctl spawn "${udid}" log show --start "@${launch_started}" --predicate "process == '${APP_PROCESS}'" \
        2>/dev/null | tail -100 | tee -a "${LOG_FILE}" || true
    echo "::error::FFmpegKit.Net simulator smoke tests failed or timed out"
    exit 1
fi

echo "==> simulator smoke tests passed"
