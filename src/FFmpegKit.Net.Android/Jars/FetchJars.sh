#!/bin/sh

set -e

# Downloads the native FFmpegKit binaries the binding is built against.
#
# NOTE: the original arthenica/ffmpeg-kit repo is archived and its v5.1.LTS release
# assets have been deleted from GitHub. Its successor, ffmpeg-kit-next, is source-only
# (no prebuilt binaries since v6.1.0). The .aar files below instead come from the
# community-maintained Maven Central mirror at dev.ffmpegkit-maintained.
#
# Usage:
#   ./FetchJars.sh          # version from Directory.Build.props
#   ./FetchJars.sh 8.2.0    # override, e.g. to try a newer upstream build before committing to it

cd "$(dirname "$0")"

ROOT="$(cd ../../.. && pwd)"
PROPS="$ROOT/Directory.Build.props"
MAVEN_BASE="https://repo1.maven.org/maven2/dev/ffmpegkit-maintained"
SMART_EXCEPTION_VERSION="0.2.1"

# Read the version from Directory.Build.props rather than repeating it here: the .aar file names
# are baked into the .csproj via FFmpegKitAndroidNativeVersion, so a second copy that drifts out
# of sync fails the build with a confusing "file not found" on an .aar nobody downloaded. (The
# iOS binding has its own FFmpegKitIosNativeVersion in the same file - the two do not track each
# other, see the root Directory.Build.props.)
FFMPEG_KIT_VERSION="$1"
if [ -z "$FFMPEG_KIT_VERSION" ]; then
    FFMPEG_KIT_VERSION=$(sed -n 's:.*<FFmpegKitAndroidNativeVersion>\(.*\)</FFmpegKitAndroidNativeVersion>.*:\1:p' "$PROPS" | head -1)
fi

if [ -z "$FFMPEG_KIT_VERSION" ]; then
    echo "error: could not read FFmpegKitAndroidNativeVersion from $PROPS" >&2
    exit 1
fi

# The version is interpolated into URLs and file names, so reject anything exotic up front.
case "$FFMPEG_KIT_VERSION" in
    *[!A-Za-z0-9._-]*)
        echo "error: invalid version '$FFMPEG_KIT_VERSION'" >&2
        exit 1
        ;;
esac

echo "==> fetching FFmpegKit $FFMPEG_KIT_VERSION"

# Downloaded into a staging directory and moved into place only once every file has arrived.
# Deleting first and downloading second leaves the working copy with no binaries at all when a
# version turns out not to exist, which costs a 200 MB re-download to recover from.
STAGING=$(mktemp -d)
trap 'rm -rf "$STAGING"' EXIT

# FFmpegKitConfig's static initialiser calls com.arthenica.smartexception.java.Exceptions, which
# is not bundled in the .aar and is not declared in its .pom. Without these two jars every
# FFmpeg call fails at runtime with NoClassDefFoundError.
for artifact in common java; do
    curl -fL --output-dir "$STAGING" -O \
        "https://github.com/tanersener/smart-exception/releases/download/v$SMART_EXCEPTION_VERSION/smart-exception-$artifact-$SMART_EXCEPTION_VERSION.jar"
done

for variant in audio full full-gpl https https-gpl min min-gpl video; do
    curl -fL --output-dir "$STAGING" -O \
        "$MAVEN_BASE/ffmpeg-kit-$variant/$FFMPEG_KIT_VERSION/ffmpeg-kit-$variant-$FFMPEG_KIT_VERSION.aar"
done

rm -f ./*.jar
rm -f ./*.aar
mv "$STAGING"/*.jar "$STAGING"/*.aar .

echo "==> fetched $(ls ./*.aar | wc -l | tr -d ' ') .aar and $(ls ./*.jar | wc -l | tr -d ' ') .jar files"
