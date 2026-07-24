#!/usr/bin/env python3
"""Merge target-framework assets from one set of NuGet packages into another.

No single .NET SDK can build net8, net9 and net10 together for a given platform: each SDK's
Android or iOS workload supports only the current TFM and the previous one. Every
variant-parameterized project (both bindings, the cross-platform client, the MAUI package) is
therefore built in two passes (see BuildNugets.sh) and merged here into one package.

For every package in PRIMARY, any lib/<tfm>/ tree that exists in the matching ADDITIONAL package
but not in PRIMARY is copied across, and a dependency group is added to the nuspec so NuGet
advertises the new target framework. Everything else comes from PRIMARY unchanged.

Usage: merge-packages.py PRIMARY_DIR ADDITIONAL_DIR OUTPUT_DIR
"""

from __future__ import annotations

import re
import shutil
import sys
import zipfile
from pathlib import Path

BOM = b"\xef\xbb\xbf"


def target_frameworks(package: zipfile.ZipFile) -> set[str]:
    """Every <tfm> that has a lib/<tfm>/ folder in the package."""
    found = set()
    for name in package.namelist():
        parts = name.split("/")
        if len(parts) > 2 and parts[0] == "lib" and parts[2]:
            found.add(parts[1])
    return found


def dependency_group(nuspec: str, tfm: str) -> str:
    """The <group targetFramework="tfm">...</group> block for one framework, verbatim.

    Packages with no dependencies for that framework (the two bindings, which reference nothing)
    have no group to find, so an empty self-closing one is synthesized. Packages that do have
    real PackageReferences for it (the cross-platform client, the MAUI package) must carry their
    actual <dependency> children across - an empty group would silently drop the dependency for
    just the merged-in framework, while the one packed directly for it works, which is exactly
    the kind of one-TFM-only bug that is easy to miss by testing a single band.
    """
    pattern = re.compile(
        rf'<group targetFramework="{re.escape(tfm)}"[^>]*/>'
        rf'|<group targetFramework="{re.escape(tfm)}"[^>]*>.*?</group>',
        re.DOTALL,
    )
    match = pattern.search(nuspec)
    return match.group(0) if match else f'<group targetFramework="{tfm}" />'


def add_dependency_groups(nuspec: str, additional_nuspec: str, frameworks: list[str]) -> str:
    """Add the dependency group for each framework, copied from additional_nuspec.

    Edited as text rather than via ElementTree so that the rest of the nuspec - including
    attribute order and the xmlns declaration NuGet emits - is preserved byte for byte.
    """
    groups = "".join(f"\n      {dependency_group(additional_nuspec, tfm)}" for tfm in frameworks)

    if "</dependencies>" in nuspec:
        # Consume the existing indentation so the closing tag stays on its own tidy line.
        return re.sub(r"\s*</dependencies>", f"{groups}\n    </dependencies>", nuspec, count=1)

    # No <dependencies> element at all: add one at the end of <metadata>.
    if "</metadata>" in nuspec:
        return nuspec.replace(
            "</metadata>",
            f"    <dependencies>{groups}\n    </dependencies>\n  </metadata>",
            1,
        )

    raise ValueError("nuspec has no </metadata> element")


def merge(primary_path: Path, additional_path: Path, output_path: Path) -> list[str]:
    with zipfile.ZipFile(primary_path) as primary, zipfile.ZipFile(additional_path) as additional:
        missing = sorted(target_frameworks(additional) - target_frameworks(primary))
        if not missing:
            shutil.copy2(primary_path, output_path)
            return []

        carried = [
            name
            for name in additional.namelist()
            if any(name.startswith(f"lib/{tfm}/") for tfm in missing)
        ]

        additional_nuspec_name = next(n for n in additional.namelist() if n.endswith(".nuspec"))
        additional_nuspec = additional.read(additional_nuspec_name).decode("utf-8-sig")

        with zipfile.ZipFile(output_path, "w", zipfile.ZIP_DEFLATED) as merged:
            for item in primary.infolist():
                data = primary.read(item.filename)
                if item.filename.endswith(".nuspec"):
                    had_bom = data.startswith(BOM)
                    rewritten = add_dependency_groups(data.decode("utf-8-sig"), additional_nuspec, missing)
                    data = rewritten.encode("utf-8")
                    if had_bom:
                        data = BOM + data
                merged.writestr(item, data)

            for name in carried:
                merged.writestr(additional.getinfo(name), additional.read(name))

    return missing


def main(argv: list[str]) -> int:
    if len(argv) != 4:
        print(__doc__, file=sys.stderr)
        return 2

    primary_dir, additional_dir, output_dir = (Path(p) for p in argv[1:])
    output_dir.mkdir(parents=True, exist_ok=True)

    packages = sorted(p for ext in ("*.nupkg", "*.snupkg") for p in primary_dir.glob(ext))
    if not packages:
        print(f"error: no packages found in {primary_dir}", file=sys.stderr)
        return 1

    failed = False
    for package in packages:
        # Package file names are <id>.<version>.<ext>, identical across both passes.
        counterpart = additional_dir / package.name
        if not counterpart.exists():
            print(f"error: {package.name} has no counterpart in {additional_dir}", file=sys.stderr)
            failed = True
            continue

        added = merge(package, counterpart, output_dir / package.name)
        print(f"{package.name}: added {', '.join(added) if added else 'nothing'}")

    return 1 if failed else 0


if __name__ == "__main__":
    sys.exit(main(sys.argv))
