#!/bin/sh

set -e

# Downloads the native FFmpegKit xcframeworks the binding is built against, into
# src/FFmpegKit.iOS/libs/<Variant>/.
#
# NOTE: unlike Android, there is no Maven-style feed for the iOS binaries. The original
# arthenica/ffmpeg-kit repo is archived and every one of its releases now carries zero assets;
# its successor ffmpeg-kit-next is source-only; and ffmpegkit-maintained/ffmpeg - the fork the
# Android bindings use - publishes .aar files only, nothing for Apple platforms. The xcframeworks
# below come from the ffmpeg_kit_flutter fork, which is the only source still publishing all
# eight variants for iOS. See the README for the full comparison.
#
# Usage:
#   ./FetchXcFrameworks.sh              # every variant, version from Directory.Build.props
#   ./FetchXcFrameworks.sh 8.1.2        # override the version
#   ./FetchXcFrameworks.sh 8.1.2 Video  # ...and fetch a single variant
#
# Each release is tagged <version>-<variant> and carries the eight xcframeworks as separate
# zips plus a checksums.json. Every download is verified against it: these are ~30 MB of native
# code that gets linked into consumers' apps, so a truncated or substituted archive must fail
# here rather than at link time or, worse, silently.

cd "$(dirname "$0")"

ROOT="$(cd .. && pwd)"
PROPS="$ROOT/Directory.Build.props"
# The frameworks belong to the binding project, which lives elsewhere in the tree.
LIBS="$ROOT/src/FFmpegKit.Net.iOS/libs"
RELEASE_BASE="https://github.com/sk3llo/ffmpeg_kit_flutter/releases/download"

# Read the version from Directory.Build.props rather than repeating it here: the .csproj resolves
# the xcframeworks through the same property, so a second copy that drifts out of sync fails the
# build with a confusing "file not found" on frameworks nobody downloaded. (The Android binding
# has its own FFmpegKitAndroidNativeVersion in the same file - the two do not track each other,
# see the root Directory.Build.props.)
FFMPEG_KIT_VERSION="$1"
if [ -z "$FFMPEG_KIT_VERSION" ]; then
    FFMPEG_KIT_VERSION=$(sed -n 's:.*<FFmpegKitIosNativeVersion>\(.*\)</FFmpegKitIosNativeVersion>.*:\1:p' "$PROPS" | head -1)
fi

if [ -z "$FFMPEG_KIT_VERSION" ]; then
    echo "error: could not read FFmpegKitIosNativeVersion from $PROPS" >&2
    exit 1
fi

# The version and variant are interpolated into URLs and paths, so reject anything exotic up front.
case "$FFMPEG_KIT_VERSION" in
    *[!A-Za-z0-9._-]*)
        echo "error: invalid version '$FFMPEG_KIT_VERSION'" >&2
        exit 1
        ;;
esac

VARIANTS="Audio Full FullGpl Https HttpsGpl Min MinGpl Video"
if [ -n "$2" ]; then
    case "$2" in
        *[!A-Za-z]*)
            echo "error: invalid variant '$2'" >&2
            exit 1
            ;;
    esac
    VARIANTS="$2"
fi

# The eight frameworks every variant ships. ffmpegkit is the Objective-C API the binding is
# generated from; the other seven are the FFmpeg libraries it links against.
FRAMEWORKS="ffmpegkit libavcodec libavdevice libavfilter libavformat libavutil libswresample libswscale"

# FullGpl -> full-gpl, HttpsGpl -> https-gpl, Video -> video, ... - the same mapping the .csproj
# applies, so the download location and the path the build expects cannot disagree.
release_tag_for() {
    printf '%s' "$1" \
        | tr '[:upper:]' '[:lower:]' \
        | sed 's/gpl$/-gpl/'
}

sha256_of() {
    if command -v shasum >/dev/null 2>&1; then
        shasum -a 256 "$1" | cut -d' ' -f1
    else
        sha256sum "$1" | cut -d' ' -f1
    fi
}

# Each xcframework ships an ios device slice, an ios simulator slice and a macos one. The macos
# slice is a third of the payload and is unreachable from a net*-ios binding, but it would still
# be embedded in the package: the whole xcframework is zipped into the assembly's .resources.zip,
# once per target framework. Three target frameworks x eight variants makes that the difference
# between packages of ~120 MB and ~190 MB, against a 250 MB limit on nuget.org.
#
# The slice directory alone cannot just be deleted - AvailableLibraries in Info.plist would still
# advertise it, and the iOS SDK rejects an xcframework whose manifest points at a missing slice -
# so the manifest is rewritten to match. plistlib is stdlib, and python3 is already required by
# the package merge step.
strip_non_ios_slices() {
    python3 - "$1" <<'PYTHON'
import plistlib
import shutil
import sys
from pathlib import Path

root = Path(sys.argv[1])

for manifest in sorted(root.glob("*.xcframework/Info.plist")):
    with manifest.open("rb") as handle:
        plist = plistlib.load(handle)

    libraries = plist.get("AvailableLibraries", [])
    keep = [lib for lib in libraries if lib.get("SupportedPlatform") == "ios"]
    dropped = [lib for lib in libraries if lib.get("SupportedPlatform") != "ios"]

    if not keep:
        raise SystemExit(f"error: {manifest.parent.name} has no ios slice")
    if not dropped:
        continue

    for lib in dropped:
        shutil.rmtree(manifest.parent / lib["LibraryIdentifier"], ignore_errors=True)

    plist["AvailableLibraries"] = keep
    with manifest.open("wb") as handle:
        plistlib.dump(plist, handle)
PYTHON
}

WORK_DIR="$(mktemp -d)"
trap 'rm -rf "$WORK_DIR"' EXIT

for variant in $VARIANTS; do
    tag="$(release_tag_for "$variant")"
    base="$RELEASE_BASE/$FFMPEG_KIT_VERSION-$tag"
    target="$LIBS/$variant"

    echo "==> fetching FFmpegKit $FFMPEG_KIT_VERSION $variant"

    # Fetch the manifest first: a variant that does not exist for this version fails here, before
    # 200 MB has been downloaded.
    checksums="$WORK_DIR/$variant-checksums.json"
    if ! curl -fsSL -o "$checksums" "$base/checksums.json"; then
        echo "error: no checksums.json for $FFMPEG_KIT_VERSION-$tag - does that release exist?" >&2
        exit 1
    fi

    rm -rf "$target"
    mkdir -p "$target"

    for framework in $FRAMEWORKS; do
        archive="$WORK_DIR/$variant-$framework.zip"
        curl -fsSL -o "$archive" "$base/$framework.xcframework.zip"

        expected=$(sed -n "s/.*\"$framework\"[[:space:]]*:[[:space:]]*\"\([0-9a-f]*\)\".*/\1/p" "$checksums" | head -1)
        if [ -z "$expected" ]; then
            echo "error: checksums.json for $variant has no entry for $framework" >&2
            exit 1
        fi

        actual=$(sha256_of "$archive")
        if [ "$actual" != "$expected" ]; then
            echo "error: checksum mismatch for $variant/$framework" >&2
            echo "  expected $expected" >&2
            echo "  actual   $actual" >&2
            exit 1
        fi

        # -q because eight variants x eight frameworks is thousands of lines of file listing.
        unzip -q "$archive" -d "$target"
        rm -f "$archive"
    done

    # macOS archives carry AppleDouble (._*) companions and __MACOSX/ directories. Left in place
    # they end up inside the packed .resources.zip, and the iOS SDK treats the stray ._Info.plist
    # beside a real one as a malformed xcframework.
    find "$target" -name '._*' -delete
    rm -rf "$target/__MACOSX"

    strip_non_ios_slices "$target"
done

echo "==> done"
