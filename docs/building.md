---
title: "Building specht"
description: "How to build, test and pack this repository"
type: reference
---

# Building

Requires the .NET SDK pinned in [`global.json`](../global.json).

```sh
dotnet tool restore
./build.sh             # compile and test
./build.sh Pack        # specht.tool.<version>.nupkg
./build.sh Specht   # run specht on this repository's own specifications
./build.sh LockSchemaVersion # record the newest schema version's digest once
```

`LockSchemaVersion` adds the newest shipped schema version to
`src/tool/schema/versions.lock.json`. Commit the lock before publishing that
version. Each entry is one SHA-256 over the version's schemas, manifest,
templates and rule pages. Running the target again leaves an existing entry
and the lock's bytes unchanged, even if a locked file was edited.
