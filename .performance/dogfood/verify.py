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

for name in (
    ".specht/manifest.json",
    ".specht/schema/frontmatter/feature.schema.json",
    ".specht/schema/frontmatter/task.schema.json",
    ".specht/schema/frontmatter/epic.schema.json",
    ".spec/adr/0017-compiler-loaded-components-target-netstandard.md",
):
    target = CONSUMER / name
    target.parent.mkdir(parents=True, exist_ok=True)
    shutil.copy2(ROOT / name, target)

shutil.copytree(ROOT / "src/analyzers", CONSUMER / "src/analyzers", ignore=shutil.ignore_patterns("bin", "obj", ".issue"))
for name in ("FrontmatterTextReader.cs", "FrontmatterText.cs", "MarkdownTextReader.cs", "MarkdownTextSection.cs"):
    target = CONSUMER / "src/specht/Model" / name
    target.parent.mkdir(parents=True, exist_ok=True)
    shutil.copy2(ROOT / "src/specht/Model" / name, target)

environment = os.environ.copy()
environment["NUGET_PACKAGES"] = str(ARTIFACTS / "consumer-cache")
environment["DOTNET_CLI_USE_MSBUILD_SERVER"] = "0"
environment["MSBUILDDISABLENODEREUSE"] = "1"
project = str(CONSUMER / "src/analyzers/analyzers.csproj")
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


def build(label: str, expected: str | None = None) -> None:
    invoke(label, ["dotnet", "build", project, "--configuration", "Release", "--no-restore", "--disable-build-servers", *properties], expected)


invoke("restore", ["dotnet", "restore", project, "--configfile", str(ARTIFACTS / "nuget.config"), *properties])
build("baseline")

source = CONSUMER / "src/analyzers/Bindings/BindingResolver.cs"
original_source = source.read_text()
assert 'claim="B-015"' in original_source
source.write_text(original_source.replace('claim="B-015"', 'claim="B-999"', 1))
build("source edit", "SPCT0004")
source.write_text(original_source)
build("source restored")

feature = CONSUMER / "src/analyzers/Bindings/.spec/README.md"
original_feature = feature.read_text()
assert "| B-015 |" in original_feature
feature.write_text(original_feature.replace("| B-015 |", "| B-998 |", 1))
build("Feature edit", "SPCT0004")
feature.write_text(original_feature)
build("Feature restored")

manifest = CONSUMER / ".specht/manifest.json"
configuration = json.loads(manifest.read_text())
configuration["codeAnalysis"]["features"][0]["feature"] = "F999"
manifest.write_text(json.dumps(configuration, indent=2))
build("manifest edit", "SPCT0001")
