# FFmpegKit.Net — repository instructions

## What this repository is

- The umbrella of the FFmpegKit .NET family: one FFmpeg and FFprobe API for .NET MAUI and for plain .NET
  on Android, iOS and macOS.
- It builds exactly two packages per variant, and nothing else:
  - `FFmpegKit.Net.<Variant>` — the cross-platform client (`FFmpegKit`, `FFprobeKit`, `FFmpegKitConfig`,
    `IFFmpegKit`); Android, iOS **and** macOS.
  - `FFmpegKit.Net.<Variant>.Maui` — MAUI app-builder wiring plus the FilePicker helper; Android and iOS only.
- Eight variants, selected by `FFmpegKitBuildType`: `Audio`, `Full`, `FullGpl`, `Https`, `HttpsGpl`, `Min`,
  `MinGpl`, `Video`.
- It builds **no native code and no bindings**. The platform bindings are consumed as already-published
  NuGet packages from sbokatuk/FFmpegKit.Android, sbokatuk/FFmpegKit.iOS and sbokatuk/FFmpegKit.Mac,
  pinned to exact versions in `Directory.Build.props`.
- Version = `$(FFmpegVersion).$(FFmpegKitBindingRevision)` (8.1.2 + 3 → `VersionPrefix` 8.1.2.3). The revision
  belongs to this repository only; the three binding pins version independently of it and of each other.
- The GitHub repository is spelled `FFMpegKit.Net`, but every in-repo reference — solution, package ids,
  URLs — writes `FFmpegKit.Net`. Keep it that way.

## Build and verify

- **macOS is required.** Both packages multi-target Android, iOS and (the client) macOS together, so
  restoring either needs the iOS workload even for an Android-only change; `build/BuildNugets.sh` refuses
  to run off Darwin.
- Install the .NET 9 SDK (`global.json` pins 9.0.100, `rollForward: latestFeature`) **and** the .NET 10
  SDK — the pack runs twice, the second pass from a scratch directory pinning 10.0.100.
- Pack: `build/BuildNugets.sh [version [androidPin iosPin macPin]]` → `./artifacts` (all eight variants, both
  projects, two SDK bands merged by `build/merge-packages.py`).
- Test: `dotnet test tests/FFmpegKit.Net.UnitTests` needs no artifacts;
  `dotnet test tests/FFmpegKit.Net.PackageTests` requires a prior pack.
- `dotnet build FFmpegKit.Net.sln` fails on whichever SDK band is not installed — build individual projects,
  or use the script.
- Look up current binding versions through nuget.org's **search API**, not the flat-container versions list
  (it surfaces unlisted older releases); `docs/BUILD.md` has the exact `curl`.

## Layout

- `src/FFmpegKit.Net`, `src/FFmpegKit.Net.Maui` — the two packable projects; platform code sits under
  `Platforms/{Android,iOS,MacOS}` (no `MacOS` in the MAUI one) and is included per target framework, not by
  the default glob.
- `tests/` — `FFmpegKit.Net.UnitTests` (plain `net9.0`; links the platform-neutral sources directly, since the
  packable project has no desktop TFM), `FFmpegKit.Net.PackageTests` (inspects packed `.nupkg` files, scoped
  to `Full`), `FFmpegKit.Net.DeviceTests` (one project, Android + iOS + macOS heads).
- `samples/FFmpegKit.Net.Sample` — one MAUI app, the same `MainPage.xaml.cs` on both platforms with zero
  per-platform code, consuming the packed `FFmpegKit.Net.Full.Maui` from `./artifacts`.
- `build/` (`BuildNugets.sh`, `merge-packages.py`, `check-upstream.sh`, `upstream.tsv`), `docs/`, `licenses/`,
  `.github/scripts/`, `.github/actions/select-xcode`.
- `FFmpegKit.Net.sln` holds the two `src` projects and UnitTests + PackageTests only. The sample (needs the
  MAUI workload) and DeviceTests (restore the packed nupkg, absent on a fresh clone) stay out on purpose.
- `NuGet.config` exposes `./artifacts` as a local feed, so tests and the sample consume exactly what was
  just packed.

## Conventions

- British spelling in prose and comments ("licence", "behaviour", "recognise"). Keep American spelling in
  identifiers, MSBuild property names and SPDX expressions (`PackageLicenseExpression`, `MIT AND LGPL-3.0-only`).
- Namespaces are `Ffmpegkit.Net` / `Ffmpegkit.Net.Maui`; assemblies are `FFmpegKit.Net.$(FFmpegKitBuildType)`
  (`.Maui`). Never introduce a namespace rooted at `FFmpegKit` — a consumer whose root namespace starts with
  `FFmpegKit` already has to qualify `Ffmpegkit.Net.FFmpegKit.ExecuteAsync(...)`.
- The csproj/props files are heavily commented and explain *why*, not what. Preserve that style and extend it
  when adding a property; delete a comment only with the thing it describes.
- Exact `[x.y.z.r]` `PackageReference` ranges for the bindings, values coming from `Directory.Build.props`.
- Per-variant `obj/<Variant>/` and `bin/<Variant>/` — the eight variants are built sequentially in one working
  copy and must not cross-contaminate.
- `Nullable`, `ImplicitUsings` and `GenerateDocumentationFile` are on in `src`. `CheckEolTargetFramework=false`
  is deliberate: net8 ships on purpose.
- The iOS/macOS Registrar workaround (dotnet/macios#22071) lives in the binding repositories and reaches apps
  transitively — do not re-implement it here.

## CI and release flow

- Pull request → `pr.yml` resolves `<FFmpegVersion>-beta.<pr>.<run>` and calls `build.yml`, which always packs
  on macos-latest and — with the default `verify: true` — also runs UnitTests + PackageTests, the sample build
  matrix and the Android emulator, iOS simulator and macOS host smoke tests, then publishes the betas to
  nuget.org. Forks build and test only.
- Release: merge a `docs/release-notes/<version>.md` to **master** → `auto-release.yml` tags `v<version>` and
  dispatches `release.yml` → the `guard` job proves the tagged commit is an ancestor of the default branch →
  `build.yml` with `verify: false` → push to nuget.org → create the GitHub release, notes taken from that
  file when present. Merging a release note *is* the release.
- Never bypass the guard job and never release from an unmerged branch: a release skips re-verification only
  because the pull request verified that same commit.
- The default branch is `master` here; the sibling platform repositories use `main`. Adjust when copying
  workflow snippets between them.
- Publishing uses nuget.org trusted publishing (`NuGet/login@v1`, the single secret `NUGET_USER`,
  `environment: nuget.org`). Do not add API-key secrets.
- `upstream-drift.yml` checks the three pins daily via `build/upstream.tsv` and `build/check-upstream.sh`
  (runnable locally as `DRIFT_DIR=/tmp/drift ./build/check-upstream.sh`). A pin behind nuget.org is a finding;
  a pin ahead is the normal mid-release-train state.

## Testing

- Run `dotnet test tests/FFmpegKit.Net.UnitTests` for every change.
- Touching packaging, pins or a `.csproj`: pack first, then run PackageTests, and update the pins mirrored in
  `tests/FFmpegKit.Net.PackageTests/Packages.cs`.
- Touching the client's platform code: run the device tiers, which take `[VARIANT] VERSION [TARGET_FRAMEWORK]`:
  - `FFMPEGKIT_DEVICE_RID=android-arm64 ./.github/scripts/run-device-tests.sh Video 8.1.2.3 net9.0-android35.0`
  - `./.github/scripts/run-simulator-tests.sh Video 8.1.2.3 net9.0-ios18.0`
  - `./.github/scripts/run-mac-host-tests.sh Video 8.1.2.3 net9.0-macos15.0`

## Hard rules

- Never commit native binaries and never add a binding project here. Platform work belongs in the sibling
  repositories; this repository only re-pins.
- Never float or widen a binding pin. Keep exact `[x.y.z.r]` ranges, change them only in
  `Directory.Build.props`, and never assume the Android, iOS and Mac pins move together.
- Pin stable versions only. The platform repositories publish `-beta.<pr>.<run>` builds to nuget.org; a
  `-beta.*` pin must never reach a release-note merge or a tagged release.
- Never add the sample or DeviceTests to the solution, and never point the tests or the sample at nuget.org
  instead of the `./artifacts` feed.
- Never add a Mac Catalyst target framework or promise Catalyst support, and never add `net*-macos` to the
  MAUI package.
- Never hand-edit merged package output — fix `build/merge-packages.py`. A merge that drops a dependency group
  ships a broken package that only PackageTests catch.
- Keep the licence machinery intact: the per-variant `MIT AND …` expression plus both licence texts packed,
  and no repository-wide `PackageLicenseExpression`.
- Version bumps: a new binding revision bumps `FFmpegKitBindingRevision`; a new FFmpeg line changes
  `FFmpegVersion` and all three pins, deliberately. The release-note file name must equal the version.

## References

- `docs/BUILD.md` — the authoritative build and test reference.
- sbokatuk/FFmpegKit.Android, sbokatuk/FFmpegKit.iOS, sbokatuk/FFmpegKit.Mac — native sources, binding code
  and their own versioning. Questions about any of them belong there, not here.
- The archived arthenica/ffmpeg-kit wiki remains the reference for the underlying FFmpegKit API.

Trust these instructions, and search the codebase only when something here is incomplete or wrong.
