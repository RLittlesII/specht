#!/usr/bin/env bash
set -euo pipefail

repository="$(cd "$(dirname "$0")/../.." && pwd)"
cd "$repository"

artifacts="$repository/.artifacts/analysis-dogfood"
packages="$artifacts/packages"
cache="$artifacts/cache"
mkdir -p "$packages"
rm -f "$packages"/Specht.CodeAnalysis.*.nupkg

export DOTNET_CLI_USE_MSBUILD_SERVER=0
export MSBUILDDISABLENODEREUSE=1

global_packages="$(dotnet nuget locals global-packages --list)"
global_packages="${global_packages#global-packages: }"
rm -rf "$cache"
mkdir -p "$cache"
cat > "$artifacts/nuget.config" <<EOF
<?xml version="1.0" encoding="utf-8"?>
<configuration>
  <packageSources>
    <clear />
    <add key="dogfood" value="$packages" />
    <add key="installed" value="$global_packages" />
  </packageSources>
  <packageSourceMapping>
    <clear />
    <packageSource key="dogfood"><package pattern="Specht.CodeAnalysis" /></packageSource>
    <packageSource key="installed"><package pattern="*" /></packageSource>
  </packageSourceMapping>
</configuration>
EOF

export NUGET_PACKAGES="$cache"
dotnet restore src/codeanalysis/codeanalysis.csproj --configfile "$artifacts/nuget.config" \
  -maxcpucount:1 -p:NuGetAudit=false -p:SpechtCodeAnalysisDogfood=false
dotnet build src/codeanalysis/codeanalysis.csproj --configuration Release --no-restore \
  --disable-build-servers -maxcpucount:1 -p:NuGetAudit=false -p:SpechtCodeAnalysisDogfood=false
dotnet pack src/codeanalysis/codeanalysis.csproj --configuration Release --no-build --no-restore \
  --disable-build-servers --output "$packages" -maxcpucount:1 -p:NuGetAudit=false -p:SpechtCodeAnalysisDogfood=false

package=("$packages"/Specht.CodeAnalysis.*.nupkg)
if [[ ${#package[@]} != 1 || ! -f ${package[0]} ]]; then
  echo 'Expected one packed Specht.CodeAnalysis package.' >&2
  exit 1
fi
version="${package[0]##*/Specht.CodeAnalysis.}"
version="${version%.nupkg}"

dotnet restore src/analyzers/analyzers.csproj --configfile "$artifacts/nuget.config" \
  -maxcpucount:1 -p:NuGetAudit=false -p:SpechtCodeAnalysisDogfood=true \
  -p:SpechtCodeAnalysisDogfoodVersion="$version"
dotnet build src/analyzers/analyzers.csproj --configuration Release --no-restore \
  --disable-build-servers -maxcpucount:1 -p:SpechtCodeAnalysisDogfood=true \
  -p:SpechtCodeAnalysisDogfoodVersion="$version" -p:ReportAnalyzer=true
