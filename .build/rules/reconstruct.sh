#!/usr/bin/env bash
set -euo pipefail
root="$1"
cd "$root"
for version in $(sed -n 's/^ *"\([0-9][0-9.]*\)":.*/\1/p' src/tool/schema/versions.lock.json); do
  folder="docs/rules/v$version"
  if [ -d "$folder" ]; then
    continue
  fi
  removed=$(git rev-list -1 HEAD -- "$folder")
  if [ -z "$removed" ]; then
    echo "no commit holds $folder" >&2
    exit 1
  fi
  git archive "$removed^" "$folder" | tar -x
done
