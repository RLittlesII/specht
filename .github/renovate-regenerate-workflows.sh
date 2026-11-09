#!/usr/bin/env bash
set -euo pipefail

./build.sh --generate-configuration GitHubActions_ci --host GitHubActions
for workflow in main mirror release renovate; do
  dotnet run --project .build/.build.csproj --no-build -- --generate-configuration "GitHubActions_$workflow" --host GitHubActions
done
