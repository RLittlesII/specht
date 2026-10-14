---
title: "Decision 0002: document the authenticated global install"
description: "Apply OQ-3's permitted default with a named user-level NuGet source, environment credentials and an explicit global install source"
type: decision
---

# Decision 0002: document the authenticated global install

**Date:** 2026-10-14
**Decided by:** spec-author, applying the default explicitly permitted by OQ-3 and item 0088.

## The call

The README's global path registers the feed as `specht` in user-level NuGet
configuration, supplies `NuGetPackageSourceCredentials_specht` at run time,
and passes the feed with `--add-source`. It can run outside a consumer repository.
The local path commits a credential-free `nuget.config` and a tool manifest;
an empty repository creates that manifest with `--output .config` before installing.

## Why

An unnamed `--add-source` alone does not establish the source name that NuGet's
environment credential uses. OQ-3 already permits the named user-level source
and environment credential default; no new owner decision is asserted.

## Rejected

- A credential in a configuration file or an install argument: conflicts with C-1.
- A global install depending on a repository's package source: fails outside that repository.
- A second installation guide: conflicts with C-4.

## Affects

B-001, B-004 to B-006, B-008; C-1 to C-4; OQ-3. No claim or constraint changes.

Native behavior: [NuGet environment credentials](https://learn.microsoft.com/en-us/nuget/consume-packages/consuming-packages-authenticated-feeds),
[named sources](https://learn.microsoft.com/en-us/dotnet/core/tools/dotnet-nuget-add-source)
and [GitHub Packages authentication](https://docs.github.com/en/packages/working-with-a-github-packages-registry/working-with-the-nuget-registry).
