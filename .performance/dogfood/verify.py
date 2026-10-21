#!/usr/bin/env python3
"""Exercise the packed analyzer against disposable repository inputs."""

import json
import os
import shutil
import subprocess
from pathlib import Path


ROOT = Path(__file__).resolve().parents[2]
ARTIFACTS = ROOT / ".artifacts/analysis-dogfood"
CONSUMER = ARTIFACTS / "consumer"
PACKAGES = list((ARTIFACTS / "packages").glob("Specht.CodeAnalysis.*.nupkg"))
if len(PACKAGES) != 1:
    raise SystemExit("Run bash .performance/dogfood/run.sh first.")

VERSION = PACKAGES[0].name.removeprefix("Specht.CodeAnalysis.").removesuffix(".nupkg")
shutil.rmtree(CONSUMER, ignore_errors=True)
CONSUMER.mkdir(parents=True)
for name in ("Directory.Build.props", "Directory.Packages.props", "global.json", "version.json"):
    source = ROOT / name
    if source.exists():
        shutil.copy2(source, CONSUMER / name)

for name in (".spec/adr/0017-compiler-loaded-components-target-netstandard.md",):
    target = CONSUMER / name
    target.parent.mkdir(parents=True, exist_ok=True)
    shutil.copy2(ROOT / name, target)

shutil.copytree(ROOT / ".specht", CONSUMER / ".specht")
for name in ("analyzers", "specht", "tool"):
    shutil.copytree(ROOT / "src" / name, CONSUMER / "src" / name,
                    ignore=shutil.ignore_patterns("bin", "obj", ".issue"))
shutil.copytree(ROOT / "docs/rules", CONSUMER / "docs/rules")
shutil.copytree(ROOT / ".assets/logos", CONSUMER / ".assets/logos")

environment = os.environ.copy()
environment["NUGET_PACKAGES"] = str(ARTIFACTS / "consumer-cache")
environment["DOTNET_CLI_USE_MSBUILD_SERVER"] = "0"
environment["MSBUILDDISABLENODEREUSE"] = "1"
projects = {
    "analyzer": CONSUMER / "src/analyzers/analyzers.csproj",
    "engine": CONSUMER / "src/specht/specht.csproj",
    "CLI": CONSUMER / "src/tool/tool.csproj",
}
properties = [
    "-maxcpucount:1",
    "-p:NuGetAudit=false",
    "-p:SpechtCodeAnalysisDogfood=true",
    f"-p:SpechtCodeAnalysisDogfoodVersion={VERSION}",
    "-v:minimal",
]


def invoke(label: str, command: list[str], expected: str | None = None) -> None:
    result = subprocess.run(command, env=environment, cwd=ROOT, capture_output=True, text=True, timeout=90, check=False)
    output = result.stdout + result.stderr
    correct = result.returncode == (0 if expected is None else 1) and (expected is None or expected in output)
    if not correct:
        print(output[-6000:])
        raise SystemExit(f"{label}: expected {expected or 'a clean build'}, got exit {result.returncode}")
    print(f"{label}: {expected or 'clean'}")


def build(name: str, label: str, expected: str | None = None) -> None:
    invoke(label, ["dotnet", "build", str(projects[name]), "--configuration", "Release", "--no-restore",
                   "--disable-build-servers", *properties], expected)


for name, project in projects.items():
    invoke(f"{name} restore", ["dotnet", "restore", str(project), "--configfile", str(ARTIFACTS / "nuget.config"), *properties])
    build(name, f"{name} baseline")

source = CONSUMER / "src/analyzers/Bindings/BindingResolver.cs"
original_source = source.read_text()
assert 'claim="B-015"' in original_source
source.write_text(original_source.replace('claim="B-015"', 'claim="B-999"', 1))
build("analyzer", "source edit", "SPCT0004")
source.write_text(original_source)
build("analyzer", "source restored")

feature = CONSUMER / "src/analyzers/Bindings/.spec/README.md"
original_feature = feature.read_text()
assert "| B-015 |" in original_feature
feature.write_text(original_feature.replace("| B-015 |", "| B-998 |", 1))
build("analyzer", "Feature edit", "SPCT0004")
feature.write_text(original_feature)
build("analyzer", "Feature restored")

for name, relative in (
    ("engine", "src/specht/Rules/FeatureFileRule.cs"),
    ("CLI", "src/tool/ExitCodes.cs"),
):
    selected = CONSUMER / relative
    original = selected.read_text()
    binding = '/// <implements epic="0001" feature="F1" claim="B-001" />' if name == "engine" else \
        '/// <implements epic="0001" feature="F2" claim="B-003" />'
    assert binding in original
    selected.write_text(original.replace(binding, binding.replace('claim="B-00', 'claim="B-99'), 1))
    build(name, f"{name} invalid binding", "SPCT0004")
    selected.write_text(original.replace(binding, "", 1))
    build(name, f"{name} missing binding", "SPCT0007")
    selected.write_text(original)
    build(name, f"{name} restored")

manifest = CONSUMER / ".specht/manifest.json"
configuration = json.loads(manifest.read_text())
configuration["codeAnalysis"]["features"][0]["feature"] = "F999"
manifest.write_text(json.dumps(configuration, indent=2))
build("analyzer", "manifest edit", "SPCT0001")
