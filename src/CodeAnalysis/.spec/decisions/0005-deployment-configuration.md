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

**2026-10-18, repository owner:** schema ownership and version belong to the
consumer repository. Consume what it supplies, including bring-your-own schemas.
A consumer at `0.1.0` need not upgrade because the analyzer knows `0.2.0`; the
version string alone does not identify a Specht schema.

The earlier requirement to introduce analysis settings and ADR grammar through a
new Specht schema version is reversed. The owner confirmed that the new
`codeAnalysis` manifest section is still required; adding it does not require
schema `0.3.0` or a consumer pin change. Do not substitute a shipped schema for
the supplied consumer schema or reject it merely
because its version is outside the Specht catalog. Validate actual supplied
inputs; genuinely missing or invalid inputs still fail explicitly.

Rejected: making analyzer adoption contingent on Specht schema ownership or an
upgrade. That would dictate the consumer's schema and prevent consumers with
valid independent contracts from adopting the analyzer. The accepted cost is
reading the consumer's configured schema and mappings rather than relying on a
Specht version lookup. The owner selected existing consumer configuration through explicit mappings:
build integration supplies configuration and mappings. The new analysis section
contains analyzer settings while existing consumer schema metadata is read through
explicit mappings. F6 OQ-1 records the resolution; the remaining installation, release, build-input
and reporting decisions stand.
