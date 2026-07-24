#!/usr/bin/env bash
set -euo pipefail

# Packs FFmpegKit.Net's own two packages - the cross-platform client and the MAUI package - for
# all eight variants, for net8, net9 and net10.
#
# This repository does not build the Android or iOS bindings: both projects depend on
# FFmpegKit.Net.<Variant>.Android / .iOS, already published to nuget.org from
# sbokatuk/FFmpegKit.Android and sbokatuk/FFmpegKit.iOS, pinned to an exact version in
# Directory.Build.props (FFmpegKitAndroidPackageVersion / FFmpegKitIosPackageVersion).
#
# Usage:
#   ./build/BuildNugets.sh                              # version from Directory.Build.props
#   ./build/BuildNugets.sh 8.1.2-beta.4                  # explicit package version
#   ./build/BuildNugets.sh 8.1.2-beta.4 8.1.2.4 8.1.2.1  # override the Android/iOS package pins
#
# Requires macOS: both packages multi-target Android and iOS together, so restoring either needs
# the iOS workload regardless of which platform's code is being exercised.
#
# Packages are written to ./artifacts.
#
# Each .NET SDK's workloads support only two target frameworks per platform (the .NET 9 band
# builds net8/net9, the .NET 10 band builds net10), so every variant is packed twice per project
# and the results merged with build/merge-packages.py. global.json pins the .NET 9 SDK, and the
# SDK is resolved from the working directory, so the second pass runs from a scratch directory
# carrying its own global.json.

SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"
ROOT="$(cd "${SCRIPT_DIR}/.." && pwd)"
OUTPUT="${ROOT}/artifacts"

VARIANTS="Audio Full FullGpl Https HttpsGpl Min MinGpl Video"

PASS1_BAND="net9"
PASS2_BAND="net10"
PASS2_SDK="10.0.100"

if [ "$(uname -s)" != "Darwin" ]; then
    echo "::error::this repository's packages multi-target Android and iOS together and require macOS with Xcode" >&2
    exit 1
fi

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

NATIVE_ARGS=""
if [ -n "${2:-}" ]; then
    NATIVE_ARGS="${NATIVE_ARGS} -p:FFmpegKitAndroidPackageVersion=${2}"
fi
if [ -n "${3:-}" ]; then
    NATIVE_ARGS="${NATIVE_ARGS} -p:FFmpegKitIosPackageVersion=${3}"
fi
if [ -n "${4:-}" ]; then
    NATIVE_ARGS="${NATIVE_ARGS} -p:FFmpegKitMacPackageVersion=${4}"
fi

# NuGet prefers the global cache over any feed for an exact version, so re-packing the same
# version - routine before a release, since the version only moves when FFmpeg or the binding
# revision does - would let the Maui pass restore a PREVIOUS local pack of the client instead
# of the one built moments earlier, and compile against a stale API. Evict this repository's
# own ids at the version being packed. The platform bindings stay cached; those are immutable
# nuget.org publishes.
EFFECTIVE_VERSION="${VERSION}"
if [ -z "${EFFECTIVE_VERSION}" ]; then
    ffmpeg_version="$(sed -n 's:.*<FFmpegVersion>\(.*\)</FFmpegVersion>.*:\1:p' "${ROOT}/Directory.Build.props" | head -1)"
    binding_revision="$(sed -n 's:.*<FFmpegKitBindingRevision>\(.*\)</FFmpegKitBindingRevision>.*:\1:p' "${ROOT}/Directory.Build.props" | head -1)"
    EFFECTIVE_VERSION="${ffmpeg_version}.${binding_revision}"
fi
for variant in ${VARIANTS}; do
    id_lower="$(printf 'ffmpegkit.net.%s' "${variant}" | tr '[:upper:]' '[:lower:]')"
    rm -rf "${HOME}/.nuget/packages/${id_lower}/${EFFECTIVE_VERSION}" \
           "${HOME}/.nuget/packages/${id_lower}.maui/${EFFECTIVE_VERSION}"
done

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
    # FFmpegKit.Net.Maui depends on FFmpegKit.Net (this same repository, by PackageReference), so
    # it must be packed after - restore resolves it from ./artifacts via the local-artifacts
    # source in NuGet.config.
    pack_and_merge "${ROOT}/src/FFmpegKit.Net/FFmpegKit.Net.csproj" "${variant}"
    pack_and_merge "${ROOT}/src/FFmpegKit.Net.Maui/FFmpegKit.Net.Maui.csproj" "${variant}"
done

echo "==> packages in ${OUTPUT}:"
ls -1 "${OUTPUT}"/*.nupkg
