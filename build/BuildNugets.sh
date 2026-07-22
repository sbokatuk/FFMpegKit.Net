#!/usr/bin/env bash
set -euo pipefail

# Packs every FFmpegKit.Net package - both bindings, the cross-platform client and the MAUI
# package - for all eight variants, for net8, net9 and net10.
#
# Usage:
#   ./build/BuildNugets.sh                                  # version from Directory.Build.props
#   ./build/BuildNugets.sh 8.1.2-beta.4                      # explicit package version
#   ./build/BuildNugets.sh 8.1.2-beta.4 android              # only the Android binding
#   ./build/BuildNugets.sh 8.1.2-beta.4 apple                # only the iOS binding + cross-platform + Maui
#   ./build/BuildNugets.sh 8.1.2-beta.4 all 8.1.7 8.1.2      # override the Android/iOS native versions
#
# The scope argument exists for CI, which packs Android on a Linux runner and the Apple-only
# projects on a macOS one - the cross-platform client and the MAUI package multi-target Android
# *and* iOS together, so restoring either needs the iOS workload regardless of which platform's
# code is being exercised. It defaults to 'all', minus the Apple projects when not running on
# macOS.
#
# Run the native fetch scripts first - src/FFmpegKit.Net.Android/Jars/FetchJars.sh and, on macOS,
# build/FetchXcFrameworks.sh - or the bindings will pack without their native payload.
#
# Packages are written to ./artifacts.
#
# Each .NET SDK's workloads support only two target frameworks per platform (the .NET 9 band
# builds net8/net9, the .NET 10 band builds net10), so every variant is packed twice per project
# and the results merged with build/merge-packages.py. global.json pins the .NET 9 SDK, and the
# SDK is resolved from the working directory, so the second pass runs from a scratch directory
# carrying its own global.json.
#
# With scope 'apple', the Android package for the variant being built must already be in
# ./artifacts (CI downloads it from the pack-android job) - FFmpegKit.Net.<Variant> depends on
# both platform bindings by PackageReference, so it can only restore once both are present.

SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"
ROOT="$(cd "${SCRIPT_DIR}/.." && pwd)"
OUTPUT="${ROOT}/artifacts"

VARIANTS="Audio Full FullGpl Https HttpsGpl Min MinGpl Video"

PASS1_BAND="net9"
PASS2_BAND="net10"
PASS2_SDK="10.0.100"

VERSION="${1:-}"
VERSION_ARG=""
if [ -n "${VERSION}" ]; then
    case "${VERSION}" in
        *[!A-Za-z0-9.+_-]*)
            echo "error: invalid version '${VERSION}'" >&2
            exit 1
            ;;
    esac
    VERSION_ARG="-p:Version=${VERSION}"
fi

SCOPE="${2:-all}"
case "${SCOPE}" in
    all|android|apple) ;;
    *)
        echo "error: scope must be all, android or apple (got '${SCOPE}')" >&2
        exit 1
        ;;
esac

NATIVE_ARGS=""
if [ -n "${3:-}" ]; then
    NATIVE_ARGS="${NATIVE_ARGS} -p:FFmpegKitAndroidNativeVersion=${3}"
fi
if [ -n "${4:-}" ]; then
    NATIVE_ARGS="${NATIVE_ARGS} -p:FFmpegKitIosNativeVersion=${4}"
fi

IS_MACOS=false
[ "$(uname -s)" = "Darwin" ] && IS_MACOS=true

PACK_ANDROID=false
PACK_APPLE=false

if [ "${SCOPE}" = "all" ] || [ "${SCOPE}" = "android" ]; then
    PACK_ANDROID=true
fi

if [ "${SCOPE}" = "all" ] || [ "${SCOPE}" = "apple" ]; then
    if [ "${IS_MACOS}" = true ]; then
        PACK_APPLE=true
    elif [ "${SCOPE}" = "apple" ]; then
        echo "::error::scope 'apple' requires macOS with Xcode" >&2
        exit 1
    else
        echo "==> not macOS: skipping the iOS, cross-platform and Maui builds"
    fi
fi

mkdir -p "${OUTPUT}"

WORK="$(mktemp -d)"
trap 'rm -rf "${WORK}"' EXIT

PASS1_DIR="${WORK}/net9-pass"
PASS2_DIR="${WORK}/net10-pass"

SDK10_DIR="${WORK}/sdk10"
mkdir -p "${SDK10_DIR}"
cat > "${SDK10_DIR}/global.json" <<EOF
{ "sdk": { "version": "${PASS2_SDK}", "rollForward": "latestFeature" } }
EOF

# Packs one project in both SDK bands, then merges the two packages into artifacts/.
pack_and_merge() {
    local project="$1" variant="$2" name
    name="$(basename "${project}" .csproj)"

    echo "==> packing ${name} (${variant}, ${PASS1_BAND} band)"
    dotnet pack "${project}" \
        -c Release \
        -p:FFmpegKitBuildType="${variant}" \
        -p:FFmpegKitSdkBand="${PASS1_BAND}" \
        ${VERSION_ARG} ${NATIVE_ARGS} \
        -o "${PASS1_DIR}"

    echo "==> packing ${name} (${variant}, ${PASS2_BAND} band)"
    ( cd "${SDK10_DIR}" && dotnet pack "${project}" \
        -c Release \
        -p:FFmpegKitBuildType="${variant}" \
        -p:FFmpegKitSdkBand="${PASS2_BAND}" \
        ${VERSION_ARG} ${NATIVE_ARGS} \
        -o "${PASS2_DIR}" )

    echo "==> merging ${name} (${variant})"
    python3 "${SCRIPT_DIR}/merge-packages.py" "${PASS1_DIR}" "${PASS2_DIR}" "${OUTPUT}"

    rm -rf "${PASS1_DIR}" "${PASS2_DIR}"
}

for variant in ${VARIANTS}; do
    if [ "${PACK_ANDROID}" = true ]; then
        pack_and_merge "${ROOT}/src/FFmpegKit.Net.Android/FFmpegKit.Net.Android.csproj" "${variant}"
    fi

    if [ "${PACK_APPLE}" = true ]; then
        pack_and_merge "${ROOT}/src/FFmpegKit.Net.iOS/FFmpegKit.Net.iOS.csproj" "${variant}"

        # Both PackageReferences (Android and iOS) must already resolve from ./artifacts - see
        # the usage note above about scope 'apple' needing the Android package placed there first.
        pack_and_merge "${ROOT}/src/FFmpegKit.Net/FFmpegKit.Net.csproj" "${variant}"
        pack_and_merge "${ROOT}/src/FFmpegKit.Net.Maui/FFmpegKit.Net.Maui.csproj" "${variant}"
    fi
done

echo "==> packages in ${OUTPUT}:"
ls -1 "${OUTPUT}"/*.nupkg
