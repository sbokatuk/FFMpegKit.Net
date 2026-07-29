#!/usr/bin/env bash
set -euo pipefail

# Compares the native SDK pins in Directory.Build.props against what the vendors actually publish,
# and writes one markdown note file per reporting group for the upstream-drift workflow to turn
# into issues.
#
#   ./build/check-upstream.sh                    # print findings, exit 0
#   DRIFT_DIR=/tmp/drift ./build/check-upstream.sh   # ...and write <group>.md note files
#
# What to watch lives in build/upstream.tsv; this script only knows *how* to look. Adding a
# component means adding a row there, not editing this file.
#
# Every check is two-stage, and that is the whole point of the script:
#
#   1. discover - ask the vendor's index (CocoaPods trunk, maven-metadata.xml, the releases API)
#      what the newest version is;
#   2. confirm  - follow the vendor's own pointer to the artifact and check it actually downloads.
#
# Stage 2 is not belt-and-braces. Agora publishes RTC 4.6.3 to Maven Central for Android while
# download.agora.io still 404s the iOS zip, so a version-compare-only watcher files "bump iOS to
# 4.6.3" and build/fetch-video.sh then dies on a 404. A candidate that fails stage 2 is not an
# update - it is an announcement - and this script stays quiet about it.
#
# The reverse case *is* reported: if the download for the version currently pinned stops
# resolving, the next clean build breaks, and that should be an issue before it is a surprise.

ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
MANIFEST="${ROOT}/build/upstream.tsv"
PROPS="${ROOT}/Directory.Build.props"
DRIFT_DIR="${DRIFT_DIR:-}"

[ -f "${MANIFEST}" ] || { echo "error: no ${MANIFEST}" >&2; exit 1; }
[ -f "${PROPS}" ]    || { echo "error: no ${PROPS}" >&2; exit 1; }
[ -z "${DRIFT_DIR}" ] || mkdir -p "${DRIFT_DIR}"

# GitHub's API allows 60 unauthenticated requests an hour per IP, which a handful of components
# burns through on a shared runner. Authenticate when a token is around; work without one locally.
#
# Expanded as ${GH_AUTH[@]+"${GH_AUTH[@]}"} at the call sites: under `set -u`, bash 3.2 - which is
# what macOS still ships, so this matters for local runs - treats "${empty_array[@]}" as an unbound
# variable and aborts.
GH_AUTH=()
[ -n "${GH_TOKEN:-${GITHUB_TOKEN:-}}" ] && GH_AUTH=(-H "Authorization: Bearer ${GH_TOKEN:-${GITHUB_TOKEN}}")

fetch() { curl -fsSL --retry 3 --retry-delay 2 --max-time 60 "$@"; }

# The group column is the issue title, so it reads like prose ("RTC", "Mac Catalyst"); the note
# files are named after a slug of it.
slug() { printf '%s' "$1" | tr '[:upper:]' '[:lower:]' | tr -cs 'a-z0-9' '-' | sed 's/^-//; s/-$//'; }

note() { # note <group> <markdown>
    printf -- '- %s\n' "$2"
    [ -z "${DRIFT_DIR}" ] || printf -- '- %s\n' "$2" >> "${DRIFT_DIR}/$(slug "$1").md"
}

# Every group in the manifest, drift or not. The workflow walks this rather than the note files
# it happens to find: a group whose drift is gone has no note file, and that is precisely the
# case that has to close an issue rather than be skipped.
if [ -n "${DRIFT_DIR}" ]; then
    awk -F'\t' '!/^[[:space:]]*#/ && NF >= 6 && !seen[$1]++ { print $1 }' "${MANIFEST}" \
      | while IFS= read -r g; do printf '%s\t%s\n' "$(slug "${g}")" "${g}"; done > "${DRIFT_DIR}/groups.tsv"
fi

pin_of() { # pin_of <MSBuildPropertyName>
    sed -n "s:.*<$1>\([^<]*\)</$1>.*:\1:p" "${PROPS}" | head -1
}

# Releases only. Vendors push prereleases to the same indexes as releases - CocoaPods carries
# AgoraRtcEngine_iOS 4.6.0.TEST5 and 4.6.2-dev.3 - and `sort -V` ranks "4.6.2-dev.3" *above*
# "4.6.2", so filtering has to happen before sorting rather than after.
clean_versions() { grep -E '^[0-9]+(\.[0-9]+)*$' || true; }

newest() { sort -V | tail -1; }

# True when $2 sorts strictly above $1. Version-aware on purpose: a plain "differs from the pin"
# also fires for a patch cut from an older branch, and would point the re-pin backwards.
newer() {
    [ "$1" != "$2" ] && [ "$(printf '%s\n%s\n' "$1" "$2" | sort -V | tail -1)" = "$2" ]
}

# A real archive at the far end of a URL, as opposed to an error page served with a 200.
#
# This asks for the first four bytes rather than sending HEAD, for two reasons. Maven Central
# answers HEAD for a genuine .aar with "content-length: 630" - the size of something that is not
# the artifact - so a size threshold reads a real 40 MB download as a stub. And every artifact any
# of these manifests points at is ZIP-family (.zip, .aar, .jar, .xcframework.zip), so the first two
# bytes settle it outright: "PK" is an archive, anything else is a CDN error page. Checked against
# Maven Central, download.agora.io, artifact.tokbox.com and GitHub release assets, all of which
# honour the range request; the 404 case comes back "<?" from an XML error body.
downloadable() { # downloadable <url>
    local body magic rc=0
    body="$(mktemp)"
    curl -sL --retry 2 --max-time 60 -r 0-3 -o "${body}" "$1" 2>/dev/null || rc=1
    if [ "${rc}" -eq 0 ]; then
        magic="$(head -c 2 "${body}" 2>/dev/null || true)"
        [ "${magic}" = "PK" ] || rc=1
    fi
    rm -f "${body}"
    return "${rc}"
}

# --- discovery, one function per ecosystem -------------------------------------------------
#
# Each prints the newest published release version, or exits non-zero when the index could not be
# read. "Could not read" is never allowed to look like "nothing newer" - an unreachable index is
# reported as its own finding, because a watcher that goes quiet when the network breaks is worse
# than no watcher.

# stdin JSON -> one value per line, silent on malformed or empty input: a vendor 404 answers with
# an HTML body, and a traceback in the log obscures the finding that follows it. Empty output is
# already handled by the caller as "index unreadable", which is the right reading.
jq_lines() { python3 -c "$1" 2>/dev/null || true; }

discover_cocoapods() { # <PodName>
    fetch "https://trunk.cocoapods.org/api/v1/pods/$1" \
      | jq_lines 'import sys,json;[print(v["name"]) for v in json.load(sys.stdin)["versions"]]' \
      | clean_versions | newest
}

discover_maven() { # <base> <group-path> <artifact>
    fetch "$1/$2/$3/maven-metadata.xml" \
      | sed -n 's:.*<version>\([^<]*\)</version>.*:\1:p' | clean_versions | newest
}

# The version a *bound SDK's own POM* declares for one of its dependencies.
#
# For a library that is not independently upgradable this is the only question worth asking.
# com.vonage:webrtc is the case in point: its version is whatever the opentok-android-sdk release
# being bound was built against, and Central carrying a higher one is not an upgrade path - taking
# it would desync AndroidIgnoredJavaDependency from what the SDK actually pulls. So this reports a
# pin that disagrees with the bound SDK, and stays silent about upstream releases entirely.
discover_maven_pom_dep() { # <sdk group:artifact> <sdk-version> <dep group:artifact>
    local g="${1%%:*}" a="${1#*:}" dg="${3%%:*}" da="${3#*:}"
    fetch "https://repo1.maven.org/maven2/${g//.//}/${a}/$2/${a}-$2.pom" \
      | python3 -c "
import sys,re
t=sys.stdin.read()
for m in re.finditer(r'<dependency>(.*?)</dependency>', t, re.S):
    d = m.group(1)
    if re.search(r'<groupId>\s*${dg}\s*</groupId>', d) and re.search(r'<artifactId>\s*${da}\s*</artifactId>', d):
        v = re.search(r'<version>\s*([^<\s]+)\s*</version>', d)
        if v: print(v.group(1))
        break
" 2>/dev/null || true
}

# Highest *version* among published releases, not /releases/latest - which is the most recently
# published one, and answers with an old version when a maintenance branch gets a late patch.
discover_github_release() { # <owner/repo> [tag-strip-regex]
    fetch ${GH_AUTH[@]+"${GH_AUTH[@]}"} "https://api.github.com/repos/$1/releases?per_page=100" \
      | jq_lines 'import sys,json;[print(r["tag_name"]) for r in json.load(sys.stdin) if not r["draft"] and not r["prerelease"]]' \
      | sed -E "s/^v//; ${2:-s/^//}" | clean_versions | newest
}

discover_github_tag() { # <owner/repo> [tag-strip-expr]
    git ls-remote --tags --refs "https://github.com/$1.git" \
      | awk -F/ '{ print $NF }' | sed -E "s/^v//; ${2:-s/^//}" | clean_versions | newest
}

# nuget.org's registration index, filtered to *listed* versions.
#
# Deliberately not the flat container (v3-flatcontainer/<id>/index.json), which is the obvious
# choice and the wrong one: it enumerates every version ever pushed, including delisted ones, and
# a delisted package is still downloadable - so neither the version list nor the stage-2 download
# check can tell the difference. FFmpegKit.Net.Full.Android is the case in point: 8.1.7.1 through
# 8.1.7.3 are all delisted, the flat container still lists them, and a watcher built on it reports
# "re-pin 8.1.2.5 to 8.1.7.3" - pointing at packages the author deliberately withdrew.
#
# The registration index carries catalogEntry.listed per version. Delisted entries also carry the
# published sentinel 1900-01-01, which is checked too: listed is absent on some older entries.
#
# Paginated for packages with enough versions, and the pages are gzip whether or not they are
# asked for, hence --compressed on the outer fetch and the magic-byte check on the inner ones.
discover_nuget() { # <PackageId>
    local id
    id="$(printf '%s' "$1" | tr '[:upper:]' '[:lower:]')"
    fetch --compressed "https://api.nuget.org/v3/registration5-gz-semver2/${id}/index.json" \
      | jq_lines 'import sys,json,gzip,urllib.request
def load(u):
    r=urllib.request.Request(u,headers={"Accept-Encoding":"gzip"})
    b=urllib.request.urlopen(r,timeout=60).read()
    if b[:2]==b"\x1f\x8b": b=gzip.decompress(b)
    return json.loads(b)
for page in json.load(sys.stdin)["items"]:
    items=page.get("items")
    if items is None: items=load(page["@id"])["items"]
    for it in items:
        ce=it["catalogEntry"]
        if ce.get("listed",True) and not str(ce.get("published","")).startswith("1900"):
            print(ce["version"])' \
      | clean_versions | newest
}

# Some repositories tag one version many times, once per build variant - sk3llo/ffmpeg_kit_flutter
# ships 8.1.2-audio, 8.1.2-full, 8.1.2-min and five more. The manifest supplies a sed expression
# after the slug to reduce a tag to its bare version; without one, every such tag is discarded by
# clean_versions and the component silently never reports.
strip_expr() { case "$1" in */*:*) printf '%s' "${1#*:}" ;; *) printf 's/^//' ;; esac; }

# --- confirmation ---------------------------------------------------------------------------
#
# Resolves the URL a given version actually downloads from, following the vendor's own published
# pointer wherever there is one. For CocoaPods that is the podspec's `source` - which is how
# `pod install` finds the bytes, and is exactly what this repository's fetch-*.sh scripts already
# resolve - so a version this script confirms is one the existing tooling can fetch.

# CocoaPods' CDN shards a pod's spec directory by the first three hex digits of the *pod name's*
# MD5 (not the version's), so every version of a pod lives under the same shard.
podspec_url() { # <PodName> <version>
    local md5
    if command -v md5 >/dev/null 2>&1; then md5="$(printf '%s' "$1" | md5 -q)"
    else md5="$(printf '%s' "$1" | md5sum | cut -d' ' -f1)"; fi
    printf 'https://cdn.cocoapods.org/Specs/%s/%s/%s/%s/%s/%s.podspec.json' \
      "${md5:0:1}" "${md5:1:1}" "${md5:2:1}" "$1" "$2" "$1"
}

download_url() { # download_url <kind> <locator> <confirm-template> <version>
    local kind="$1" locator="$2" confirm="$3" version="$4"

    # An explicit template in the manifest wins - some vendors publish an SDK zip that their
    # podspec does not point at (Agora's RTM 2.x per-artifact zips, for one).
    if [ "${confirm}" != "-" ]; then
        printf '%s' "${confirm//\{version\}/${version}}"
        return 0
    fi

    case "${kind}" in
        cocoapods)
            # source.http is the artifact; source.git means the pod builds from source, and there
            # is no archive to HEAD - the tag existing is the whole of the confirmation.
            fetch "$(podspec_url "${locator}" "${version}")" \
              | jq_lines 'import sys,json;s=json.load(sys.stdin).get("source",{});print(s.get("http",""))'
            ;;
        maven)
            local group="${locator%%:*}" rest="${locator#*:}" artifact packaging base
            artifact="${rest%%:*}"; rest="${rest#*:}"
            packaging="${rest%%@*}"; [ "${packaging}" = "${artifact}" ] && packaging=aar
            base="${locator##*@}"; [ "${base}" = "${locator}" ] && base="https://repo1.maven.org/maven2"
            printf '%s/%s/%s/%s/%s-%s.%s' \
              "${base}" "${group//.//}" "${artifact}" "${version}" "${artifact}" "${version}" "${packaging}"
            ;;
        nuget)
            local nid
            nid="$(printf '%s' "${locator}" | tr '[:upper:]' '[:lower:]')"
            printf 'https://api.nuget.org/v3-flatcontainer/%s/%s/%s.%s.nupkg' \
              "${nid}" "${version}" "${nid}" "${version}"
            ;;
        *) printf '' ;;
    esac
}

# --- the pass ------------------------------------------------------------------------------

checked=0
while IFS=$'\t' read -r group label kind pin locator confirm; do
    case "${group}" in ''|'#'*) continue ;; esac
    checked=$((checked + 1))

    current="$(pin_of "${pin}")"
    if [ -z "${current}" ]; then
        note "${group}" "**${label}**: could not read \`${pin}\` from \`Directory.Build.props\` — the manifest and the props file disagree."
        continue
    fi

    # Stage 1: what does the vendor's index say is newest?
    latest=""
    case "${kind}" in
        cocoapods)      latest="$(discover_cocoapods "${locator}" || true)" ;;
        maven)
            g="${locator%%:*}"; r="${locator#*:}"; a="${r%%:*}"
            b="${locator##*@}"; [ "${b}" = "${locator}" ] && b="https://repo1.maven.org/maven2"
            latest="$(discover_maven "${b}" "${g//.//}" "${a}" || true)"
            ;;
        maven-pom-dep)
            # locator is three space-separated tokens: <sdk coords> <sdk version property> <dep coords>
            set -- ${locator}
            sdk_ver="$(pin_of "$2")"
            if [ -z "${sdk_ver}" ]; then
                note "${group}" "**${label}**: could not read \`$2\` from \`Directory.Build.props\` to locate the bound SDK's POM."
                continue
            fi
            latest="$(discover_maven_pom_dep "$1" "${sdk_ver}" "$3" || true)"
            ;;
        nuget)          latest="$(discover_nuget "${locator}" || true)" ;;
        github-release) latest="$(discover_github_release "${locator%%:*}" "$(strip_expr "${locator}")" || true)" ;;
        github-tag)     latest="$(discover_github_tag   "${locator%%:*}" "$(strip_expr "${locator}")" || true)" ;;
        *)
            note "${group}" "**${label}**: manifest asks for unknown check kind \`${kind}\`."
            continue
            ;;
    esac

    if [ -z "${latest}" ]; then
        note "${group}" "**${label}**: could not read the upstream index (\`${kind}\`, \`${locator}\`) — pinned \`${current}\`, so this component went unchecked."
        continue
    fi

    # The pinned version's own artifact still has to be there. When it is not, the next clean
    # build fails at fetch time; better to hear it here.
    pinned_url="$(download_url "${kind}" "${locator}" "${confirm}" "${current}" || true)"
    if [ -n "${pinned_url}" ] && ! downloadable "${pinned_url}"; then
        note "${group}" "**${label}**: the pinned \`${current}\` no longer downloads from ${pinned_url} — builds will fail until the pin moves."
    fi

    # A derived pin is right when it equals what the bound SDK asks for - not when it is newest.
    # "Newer exists upstream" is deliberately not a finding here: it is the question that produced
    # an unactionable report about com.vonage:webrtc 145.0.113 while opentok-android-sdk 2.34.1
    # was, correctly, still built against 121.1.101.
    # A platform binding package, published from a sibling repository - this author's own release,
    # not a vendor's. So the question is not "did upstream ship something" but "has this umbrella
    # been re-pinned behind a platform release that already went out". The two directions are not
    # symmetric, and treating them as one is what would make this watcher useless:
    #
    #   pin < published   the umbrella resolves an older platform package than the one on
    #                     nuget.org. Consumers of the umbrella silently get the old binding. This
    #                     is the finding the check exists for.
    #   pin > published   the platform release is not on nuget.org yet. That is the normal
    #                     mid-flight state of a release train - platform repositories publish
    #                     first and the umbrella re-pins behind them - so it is logged and never
    #                     filed, or every train would open an issue on its way through.
    if [ "${kind}" = "nuget" ]; then
        # A prerelease pin. Every pull request publishes <version>-beta.N.M, and one of those left
        # behind in Directory.Build.props means the released umbrella depends on a package built
        # from a branch. This is checked first because `sort -V` ranks a prerelease *above* its own
        # release - "2.34.1.4-beta.7.3" above "2.34.1.4" - so the comparisons below would read it
        # as a pin ahead of upstream and say nothing.
        case "${current}" in
            *-*)
                note "${group}" "**${label}**: pinned the prerelease \`${current}\` while \`${latest}\` is published on nuget.org — a released umbrella must not depend on a beta. Re-pin \`${pin}\` in \`Directory.Build.props\`."
                continue
                ;;
        esac
        if [ "${current}" = "${latest}" ]; then
            echo "    ok  ${label}: pinned ${current}, newest published ${latest}"
        elif newer "${latest}" "${current}"; then
            echo "    -   ${label}: pinned ${current} is ahead of nuget.org's ${latest} — platform release not published yet"
        else
            url="$(download_url nuget "${locator}" "${confirm}" "${latest}" || true)"
            if [ -n "${url}" ] && downloadable "${url}"; then
                note "${group}" "**${label}**: pinned \`${current}\`, but \`${latest}\` is published on nuget.org — re-pin \`${pin}\` in \`Directory.Build.props\`. [nupkg](${url})"
            else
                echo "    -   ${label}: ${latest} is indexed but its .nupkg does not download yet (pinned ${current})"
            fi
        fi
        continue
    fi

    if [ "${kind}" = "maven-pom-dep" ]; then
        set -- ${locator}
        if [ "${latest}" = "${current}" ]; then
            echo "    ok  ${label}: pinned ${current}, matching what $(pin_of "$2") of ${1##*:} declares"
        else
            note "${group}" "**${label}**: pinned \`${current}\`, but \`${1##*:}\` \`$(pin_of "$2")\` declares \`${latest}\` — the two must match, so re-pin to \`${latest}\`."
        fi
        continue
    fi

    # A pin above everything upstream publishes is not "up to date" - it means this row is pointed
    # at the wrong artifact, or the vendor withdrew a release. Reported, because the alternative is
    # a component that reads as permanently fine while being checked against the wrong thing: that
    # is exactly how a dead AgoraAudio_macOS pod (last release 3.7.2) sat under a 4.6.2 pin.
    if newer "${latest}" "${current}"; then
        note "${group}" "**${label}**: pinned \`${current}\`, but upstream publishes nothing newer than \`${latest}\` — the manifest row may be pointed at the wrong artifact, or the release was withdrawn."
        continue
    fi

    if ! newer "${current}" "${latest}"; then
        echo "    ok  ${label}: pinned ${current}, newest published ${latest}"
        continue
    fi

    # Stage 2: an index entry is a claim; the download is the evidence.
    url="$(download_url "${kind}" "${locator}" "${confirm}" "${latest}" || true)"
    if [ -z "${url}" ]; then
        case "${kind}" in
            github-release|github-tag)
                # Components built from source have no archive to HEAD, so the tag or release
                # existing is the whole of the confirmation - and discovery already established
                # that. Named in the manifest's comments wherever it applies.
                note "${group}" "**${label}**: \`${latest}\` is tagged upstream (pinned \`${current}\`) — built from source, so there is no archive to verify."
                ;;
            *)
                note "${group}" "**${label}**: \`${latest}\` is published (pinned \`${current}\`), but no download URL could be resolved for it — check by hand before re-pinning."
                ;;
        esac
    elif downloadable "${url}"; then
        note "${group}" "**${label}**: \`${latest}\` is available (pinned \`${current}\`) — [download](${url})."
    else
        # The 4.6.3 case. Not actionable, so not a finding; visible in the log so that "the vendor
        # announced it, why is there no issue?" has an answer.
        echo "    -   ${label}: ${latest} is indexed but not downloadable yet (pinned ${current})"
    fi
done < "${MANIFEST}"

echo "checked ${checked} component(s)"
