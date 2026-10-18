---
title: "Decision 0005: Deployment and configuration"
description: "Deploy Specht.CodeAnalysis privately, align release versions and keep analysis policy in the versioned manifest"
type: decision
---

# Decision 0005: Deployment and configuration

**Date:** 2026-10-17
**Decided by:** Repository owner

## The call

Publish `Specht.CodeAnalysis` as a private NuGet PackageReference carrying the
analyzer and code-fix assemblies and build integration, without an application
runtime dependency. ADR-0016's non-packable component boundaries remain in force.

Use the repository release version alongside `specht.tool`. Compatibility is
checked through manifest/schema and evidence/report contract versions rather
than exact installed package-version equality. A new analyzer release does not
change a consumer's schema pin.

Put analysis policy in a versioned `codeAnalysis` section of
`.specht/manifest.json`: selected projects/target frameworks and Features,
required-authoring scopes, required links, exemptions and record discovery.
Reuse existing identifier grammars. Introduce these settings through a new schema
version; preserve existing pins and the checker behavior of older versions.

MSBuild owns repository-root selection and input delivery. Standard Roslyn
configuration owns diagnostic severity and suppression; do not duplicate those
settings in the analysis policy. Concrete field shapes and build item/property
contracts are recorded in this Feature's technical design.

## Why

One private installation deploys the capability coherently without changing the
consumer application's runtime closure. Aligned releases simplify distribution;
contract versions allow independently installed compatible packages to cooperate.
Manifest-owned policy preserves the existing configuration model and ID grammars.

## Rejected

- Independent release numbering for this capability: adds coordination without
  improving contract-based interoperability.
- Require exact package-version equality: rejects compatible producer/consumer
  combinations even when their wire contracts agree.
- A second analysis-policy file or regex configuration: duplicates consumer
  configuration and permits grammar drift.
- Add settings to an old published schema in place: changes the meaning of an
  existing consumer pin.

The accepted cost is a new versioned manifest section and package/build integration,
with compatibility checks against the explicit contracts.

## Affects

0201-F6 B-019 through B-024 and OQ-1/OQ-2; epic 0201 OQ-1 and detailed input
configuration. ADR-0017 records the separately approved compiler-target exception.

## Reversal

None.
