#!/bin/bash
set -euo pipefail

install-tool dotnet "$DOTNET_SDK_VERSION"
runuser -u ubuntu renovate
