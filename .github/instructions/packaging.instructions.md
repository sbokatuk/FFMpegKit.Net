---
applyTo: "src/**/*.csproj, Directory.Build.props, build/**"
---

# Packaging rules

- Binding pins live **only** in `Directory.Build.props` (`FFmpegKitAndroidPackageVersion`,
  `FFmpegKitIosPackageVersion`, `FFmpegKitMacPackageVersion`) and are referenced as exact `[$(pin)]` ranges.
  Never float, widen or inline one, never pin a `-beta.*` build, and never assume the three move together.
- Mirror any pin change in `tests/FFmpegKit.Net.PackageTests/Packages.cs`, which asserts the packed nuspec
  declares that exact version.
- Target frameworks are SDK-band-switched by `FFmpegKitSdkBand` and must stay in step with
  `docs/BUILD.md` and `Packages.cs`:

  | | `net9` band | `net10` band |
  | --- | --- | --- |
  | Android | `net8.0-android34.0`, `net9.0-android35.0` | `net10.0-android36.0` |
  | iOS | `net8.0-ios18.0`, `net9.0-ios18.0` | `net10.0-ios26.0` |
  | macOS | `net8.0-macos14.0`, `net9.0-macos15.0` | `net10.0-macos26.0` |

  The platform version in each target framework matches what the external packages target — a mismatch fails
  restore. No Mac Catalyst, ever, and `net*-macos` stays off `FFmpegKit.Net.Maui`.
- Keep the per-variant `BaseIntermediateOutputPath`/`BaseOutputPath` (`obj/<Variant>/`, `bin/<Variant>/`): the
  eight variants are packed sequentially in one working copy.
- Because no SDK builds all three bands, `build/BuildNugets.sh` packs twice and `build/merge-packages.py`
  merges. The merge must carry `lib/<tfm>` trees **and** the matching nuspec dependency group with its
  `<dependency>` children — an empty group silently drops the binding for the merged-in framework. Fix the
  script; never edit a packed `.nupkg` by hand.
- Pack `FFmpegKit.Net` before `FFmpegKit.Net.Maui`: the MAUI package takes a `PackageReference` on the client
  at the same version, resolved from the `./artifacts` feed.
- `Microsoft.Maui.Controls` floors are per band and deliberately literal — never replace them with
  `$(MauiVersion)`, which drifts with the runner image and trips NU1605 for consumers.
- Licensing is per variant: `MIT AND GPL-3.0-only` for `*Gpl` build types, `MIT AND LGPL-3.0-only` otherwise,
  with `LICENSE` plus the matching `licenses/*.txt` packed into `licenses/`. No repository-wide
  `PackageLicenseExpression`.
- Adding a component to the drift watch means adding a row to `build/upstream.tsv`, not editing
  `build/check-upstream.sh`.
- These files carry long explanatory comments on purpose. Keep them accurate and add one for any new property
  whose reason is not obvious from its name.
