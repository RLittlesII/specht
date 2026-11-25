---
title: Specht roadmap
description: Selected release outcomes, the agreement work they still need, and the maintenance, research and intake kept outside release promises.
type: reference
---

# Roadmap

[`.issue/.goal`](../.issue/.goal) owns release membership and order. Local
`.issue/` records own delivery state; specifications own behavior. GitHub issues
and milestones mirror `main`. Run the
[queue](../.agents/skills/specht-conventions/references/delivery.md#choosing-the-next-item)
for current priorities, dependency holds and shared write sets. Parent Features
and epics summarize their children; they are not additional implementation work.

## 0.8.0 — Review groundwork

The intended outcome is trustworthy specification references and duplicate-id
diagnostics. The selected work does not yet constitute the implementation plan
for that outcome: qualified-key resolution is a specification spike, and the
duplicate-id task needs agreed delivery slices. Completing an agreement is not
shipping its resolver or rules.

| Work                                                  | Agreement and delivery path                                                                                                                                                                                                                                                   |
| ----------------------------------------------------- | ----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| Qualified specification keys, epic `0228` candidate C | [0307](../.issue/0307-specify-qualified-key-resolution.yml) agrees the supported keys, ambiguous and missing outcomes, and lookup versus validation boundaries. Delivery items follow that agreement.                                                                         |
| Duplicate-id detection, epic `0101` and CI `0055-F2`  | [0460](../.issue/0460-decompose-id-uniqueness-release-agreements.yml) reconciles the five cases with current agreements and cuts claim-citing children. [0128](../.issue/0128-detect-an-id-claimed-twice.yml) retains the owner's release placement and depends on that work. |

Before implementation, replace the unsliced finish line with the agreed delivery
children in the goal file. The release is complete only when those selected
claims are proven and delivered. Presentation, all linting and formatting are
not implied by these entries; their wider epics still need decomposition.

## 0.9.0 — Onboarding report and documentation

A newcomer can understand the findings from one check and read generated command
and API documentation at a stable site. This release selects the onboarding
report `0001-F15`, not the wider resumable journey proposed by epic `0196`.

| Feature or slice                                                                | Delivery path                                                                                                  | Agreement boundary                                                                                                                        |
| ------------------------------------------------------------------------------- | -------------------------------------------------------------------------------------------------------------- | ----------------------------------------------------------------------------------------------------------------------------------------- |
| [Onboarding report `0001-F15`](../src/tool/Features/Onboarding/.spec/README.md) | Terminal command `0385`, then Markdown report `0386`                                                           | Resolve OQ-2, the command's exit-code contract, before implementation. The report explains one check and changes no check verdict.        |
| [Documentation build `0002-F1`](.spec/README.md)                                | Public documentation enforcement `0404` and DocFX API generation `0405`, then default-build integration `0406` | Design the existing claims; agreement edits to the shared specification are sequenced.                                                    |
| [Documentation site `0002-F2`](site/.spec/README.md)                            | API generation `0405`, then site build `0407`, then Pages publication `0408`                                   | Keep site generation and publication separate; the site uses DocFX.                                                                       |
| Checker's stated limit                                                          | [0313](../.issue/0313-state-the-checkers-limit-in-user-docs.yml)                                               | Explain structural validation without promising behavioral correctness.                                                                   |
| Analyzer adoption guidance                                                      | [0449](../src/codeanalysis/.issue/0449-document-consumer-analysis-setup.yml)                                   | Use the documentation remedy allowed by outside report #639; cover the related setup guidance in #651 without changing analysis defaults. |

The finish line is the selected items and their claim coverage, not closure of
an epic or the count of its children. Maintenance does not enter this release
implicitly when a new child is filed.

## Outside reports with local owners

The original reports remain provenance and discussion. Their local records own
next steps, holds and delivery; the mirror does not adopt or close an unmarked
outside issue. A report is not declared fixed from a source inspection alone.

| Report                                                                                     | Local owner                                                                                              | Reconciliation                                                                                                                                                                                                                                                                      |
| ------------------------------------------------------------------------------------------ | -------------------------------------------------------------------------------------------------------- | ----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| [#639 — XML binding parsing setup](https://github.com/RLittlesII/specht/issues/639)        | [0449](../src/codeanalysis/.issue/0449-document-consumer-analysis-setup.yml), selected for 0.9.0         | The request permits documentation instead of automatic parse-option changes. Explain documentation parsing and the CS1591 consequence; validate the guidance with a synthetic consumer.                                                                                             |
| [#640 — required implementation evidence](https://github.com/RLittlesII/specht/issues/640) | [0450](../src/analyzers/Reconciliation/.issue/0450-reconcile-implementation-evidence-exports.yml)        | The current exporter preserves roles on ordinary methods but omits type-level documentation bindings. Agree and deliver a current production-compilation export path; missing links must not count as coverage.                                                                     |
| [#651 — analyzer adoption](https://github.com/RLittlesII/specht/issues/651)                | [0451](../src/codeanalysis/.issue/0451-reconcile-analyzer-consumer-adoption-report.yml) and its children | `0434` already delivered policy-reader reasons under `0201-F6` B-029. Generic reasons elsewhere remain decision 0008's boundary. `0449` owns documentation; the children own configuration and policy questions. The parent records all nine points and the reporter's corrections. |
| [#667 — boundaries within one assembly](https://github.com/RLittlesII/specht/issues/667)   | [0458](../.issue/0458-specify-analyzer-folder-boundaries.yml)                                            | Agree import restrictions and confinements under epic `0201`, including their decomposition and configuration ownership. The consumer's provisional table is not a shipped contract.                                                                                                |
| [#923 — uppercase Snowflake base32](https://github.com/RLittlesII/specht/issues/923)       | [0459](../.issue/0459-specify-uppercase-snowflake-base32.yml)                                            | Agree the opt-in `0001-F19` extension and schema compatibility; preserve numeric value, width, timestamp and existing defaults in both hosts.                                                                                                                                       |

The confirmed integration gaps and reported consumer breakage warrant triage;
this does not silently assign their unresolved implementation to a release.
After reproduction and agreement, schedule explicit delivery items in the goal
file.

## Maintenance outside release finish lines

These groups keep the backlog navigable without creating release promises or
storing a second copy of delivery state. Check each linked item before pickup.

| Group                                   | Work                                                                                                                                                                        |
| --------------------------------------- | --------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| CI and mirror reliability               | `0447` repairs four-digit rule annotations; `0340` closes removed items' mirrored issues through the existing agreement. Both share the generated-workflow lane.            |
| Specification and review hygiene        | `0446` clears orphan-companion warnings; `0314` improves the review handoff.                                                                                                |
| Manifest agreement completion `0001-F5` | `0349` handles rule-level casing; `0350` handles by-record traceability. Sequence the shared specification edits and verify its full coverage before closing parent `0010`. |
| Shared test support                     | `0168` precedes host-factory cleanup `0144`; `0207` and `0218` require coordination on shared test and build files.                                                         |
| Engine structure                        | `0342` covers cohesive rule groups; `0439` needs the namespace-layout ruling. Structural stages retain ADR-0027's no-regression guard.                                      |
| Cold-start memory `0109-F4`             | `0206` retains the negative-feasibility hold until an observer satisfies C-7; benchmark-process allocations are not tool-process peak memory.                               |

Open Feature rollups `0063`, `0293`, `0203`, `0010` and `0384` remain summaries.
Finishing their last currently open child does not prove every claim in the
agreement. The report contract `0033` needs its remaining coverage reconciled;
dependency-update Feature `0072` retains its deferral.

## Research and decisions

| Area                              | Agreement work                                                                                                                                                                                                                                                 |
| --------------------------------- | -------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| Analysis policy and integration   | `0452` covers absent policy, `0453` schema kinds, `0454` custom exclusions, `0455` lifecycle vocabulary, `0456` runner opt-out and `0457` undeclared projects. Keep explicit enforcement policy and record any changed boundary in the owning agreement first. |
| New structural checks             | `0308` covers sibling links; `0309` test-reference placement; `0311` amendment comments; `0315` outcomes that cannot fail; `0316` constraint tracing; `0317` terminology; `0318` warning baselines; `0358` stable code citations.                              |
| Runtime and configuration choices | `0141` evaluates editorconfig settings; `0170` evaluates AOT. Neither investigation authorizes adoption.                                                                                                                                                       |
| Expected input failures           | `0151` awaits the owner's ruling on proposed [ADR-0009](../.spec/adr/0009-input-failures-stay-exceptions-until-a-native-union-can-carry-them.md). The research record exists; no exception-to-value migration is authorized.                                   |
| Mermaid type references           | `0424` remains an investigation to perform. PR #793 filed the item only; it supplied neither the proposed ADR nor experiment evidence.                                                                                                                         |

The wider review surfaces `0228`, presentation `0194`, linting `0101`, guided
onboarding `0196`, persistent index `0132-F1`, JSON format `0193` and service
`0324` require their own agreed slices. The Action epic `0142` and code-analysis
epic `0201` keep their authored deferrals; selected intake investigation does
not adopt those epics wholesale. Their agreements, rather than a theme's name,
determine what can be scheduled.

## Keeping the plan current

Update the goal file and this page together when selecting delivery work. Do
not put a container into a release to capture future children automatically.
Remove a completed release section in the same change that advances the
development version, as [Releasing](releasing.md) describes. Let the mirror
close its milestone from `main`; do not close or move mirrored records by hand.
