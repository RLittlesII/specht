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
```
