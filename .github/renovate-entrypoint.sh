#!/bin/bash
set -euo pipefail

install-tool dotnet "$DOTNET_BUILD_SDK_VERSION"
install-tool dotnet "$DOTNET_SDK_VERSION"
runuser -u ubuntu renovate
