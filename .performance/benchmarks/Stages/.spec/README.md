---
title: "Specification: Engine stage benchmarks"
description: "One benchmark per engine stage ADR-0005 names - the manifest and schema load, discovery, the frontmatter reader, the document parse, the model build, each rule, and the evaluation of a built model - each measured alone, against tree size where the stage's input grows, with the file system named wherever a stage reads it"
type: feature
id: "F2"
epic: "0109"
spec_status: draft
status: blocked
priority: med
value: 0
risk: 0
rank: 0
scored_by: null
scored_on: null
domain: "Performance measurement"
author: "spec-author"
milestone: null
children: []
depends_on: ["F1"]
blocks: []
spikes: []
created: "2026-10-09"
updated: "2026-10-09"
github_issue: null
synced_at: null
---

# Specification: Engine stage benchmarks

## 1. Business Goal

<!-- last written by: spec-author, 2026-10-09 -->

A check's time is one number, and nothing says which stage spends it: a slower check could be the walk, the YAML reader, the Markdown parser, one rule or the coordinator, and ADR-0005's stage boundaries and ADR-0004's map lookup were chosen partly for their cost with no measurement behind either. This Feature removes that failure state: every stage the engine is built from has a benchmark of its own that measures that stage's call and nothing around it, so a cost can be traced to the stage that incurs it and a design choice made for cost can be held to it.

## 2. User Needs

<!-- last written by: spec-author, 2026-10-09 -->

| #   | Persona                           | Need                                                                     | Pain Point Today                                      |
| --- | --------------------------------- | ------------------------------------------------------------------------ | ----------------------------------------------------- |
| 1   | The developer changing the engine | See the cost of the stage they changed, before and after the change      | Only the whole check can be timed, and only by hand   |
| 2   | `benchmarker`                     | A benchmark to cite for each stage-level cost concern                    | No benchmark exists to put in a `### Performance` row |
| 3   | `implementer`                     | Know whether an ADR's cost consequence holds before writing the next ADR | ADR-0004 and ADR-0005 assert costs nobody measured    |

### Assumptions

| ID  | Assumption                                                                                                                                                                                                                                                                                 |
| --- | ------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------ |
| A-1 | The stages are those ADR-0005 § Decision (a) names. Until item `0107` lands they are reached through today's entry points.                                                                                                                                                                 |
| A-2 | The model is constructible in memory (item `0104`), and evaluation takes a model (item `0105`'s split of the runner), but evaluation still reads the disk: the companion-file rule reads each `.feature` file while it evaluates (`src/specht/Rules/FeatureFileRule.cs:41`; ADR-0005 (a)). |
| A-3 | The tree sizes and the generated tree are `0109-F1`'s (`0109-F1` decision 0004).                                                                                                                                                                                                           |

## 3. Acceptance Criteria

<!-- last written by: spec-author, 2026-10-09 -->

| ID    | Claim                                                                                                                                                                                                    | Source                                                                                                                                                                                                                | Status    |
| ----- | -------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- | --------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- | --------- |
| B-001 | Given a generated tree, this Feature measures loading its manifest and schemas.                                                                                                                          | owner, 2026-10-09; C-1                                                                                                                                                                                                | Active    |
| B-002 | Given a generated tree at each size, this Feature measures discovering its specifications.                                                                                                               | owner, 2026-10-09; C-2                                                                                                                                                                                                | Active    |
| B-003 | Given one generated specification, this Feature measures reading its frontmatter.                                                                                                                        | owner, 2026-10-09; C-3                                                                                                                                                                                                | Active    |
| B-004 | Given one generated specification, this Feature measures parsing its Markdown into the engine's document model.                                                                                          | owner, 2026-10-09; C-4                                                                                                                                                                                                | Active    |
| B-005 | Given a generated tree at each size, this Feature measures building the engine's model of it.                                                                                                            | owner, 2026-10-09; C-5                                                                                                                                                                                                | Active    |
| B-006 | Given the engine's rule set and a generated tree on disk at each size, this Feature measures each rule in it over a model built before the measurement.                                                  | owner, 2026-10-09; C-6                                                                                                                                                                                                | Active    |
| B-007 | Given a model built before the measurement from a generated tree on disk at each size, this Feature measures evaluating it - rule selection, every rule, grading, ordering and the report - as one call. | ADR-0005 (a); C-7                                                                                                                                                                                                     | Active    |
| B-008 | Given the engine registered in its container, this Feature measures resolving the coordinator apart from running it.                                                                                     | Withdrawn 2026-10-09: ADR-0005 states no resolution cost, and a second resolve of a singleton measures a cached lookup; the per-process composition cost is inside `0109-F4`'s start-to-exit finding (decision 0001). | Withdrawn |
| B-009 | Given a rule added to the engine's rule set, this Feature measures it with no change to any benchmark.                                                                                                   | C-6                                                                                                                                                                                                                   | Active    |

## 4. Constraints

<!-- last written by: spec-author, 2026-10-09 -->

| ID   | Constraint                                                                                                                                                                                                                                                                        | Rules Out                                                                                                                                                                  |
| ---- | --------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- | -------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| C-1  | The manifest and schema load's time and allocation per load is a reported finding, never a threshold; it is a fixed cost that does not scale with tree size.                                                                                                                      | A gate on it; measuring it inside a whole check only; a size parameter on a cost that has none.                                                                            |
| C-2  | Discovery's time and allocation is a reported finding against the number of specifications in the tree, never a threshold.                                                                                                                                                        | A gate on it; a single-size measurement.                                                                                                                                   |
| C-3  | The frontmatter read's time and allocation per specification is a reported finding, never a threshold; its input is one specification of the generated shape.                                                                                                                     | A gate on it; a whole tree read in its place.                                                                                                                              |
| C-4  | The Markdown parse's time and allocation per specification is a reported finding, never a threshold; its input is one specification's text, read before the measurement.                                                                                                          | A gate on it; a disk read inside the measured parse.                                                                                                                       |
| C-5  | The model build's time and allocation is a reported finding against the number of specifications, never a threshold.                                                                                                                                                              | A gate on it; a single-size measurement.                                                                                                                                   |
| C-6  | Each rule's time and allocation is a reported finding against the number of specifications in the model, never a threshold, with one result per rule in the engine's rule set; a rule that reads the disk reads the generated tree, and its finding names the file system (C-10). | A gate on it; one figure for all rules together; a hand-kept list of rules that a new rule can be missing from.                                                            |
| C-7  | Evaluation's time and allocation over a model built before the measurement is a reported finding against the number of specifications, never a threshold; it reads the generated tree's `.feature` files, and its finding names the file system (C-10).                           | A gate on it; building the model inside the measurement; measuring selection, grading or ordering apart, which no constraint asks for.                                     |
| C-8  | Withdrawn 2026-10-09: no ADR or constraint states a resolution cost (decision 0001; B-008).                                                                                                                                                                                       | Nothing.                                                                                                                                                                   |
| C-9  | The measured call is the stage's alone: the fixture, the container, the model and any text a stage takes as input are built before the measurement.                                                                                                                               | Fixture generation, container resolution or a disk read inside a measured call that does not itself read the disk.                                                         |
| C-10 | A stage that reads the disk is measured against a tree on disk, and its finding says it includes the file system and its cache.                                                                                                                                                   | Presenting a disk-reading stage's number as the engine's cost alone; measuring such a stage against a substitute file system and calling it the disk cost.                 |
| C-11 | A stage is measured through the entry point the engine calls it by, disk reads included.                                                                                                                                                                                          | A production type's visibility, signature or shape changed for a benchmark; a benchmark-only seam in `src/`; a change to `src/` to keep a rule or evaluation off the disk. |

## 5. Out of Scope

<!-- last written by: spec-author, 2026-10-09 -->

| #   | Item                                                                                        | Exclusion Reason                                                                                                                                                                                                                     |
| --- | ------------------------------------------------------------------------------------------- | ------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------ |
| 1   | A whole check from a root on disk                                                           | `0109-F3`.                                                                                                                                                                                                                           |
| 2   | The command-line host, its process and its output                                           | `0109-F4`.                                                                                                                                                                                                                           |
| 3   | Selection, grading and ordering each measured apart                                         | No constraint asks; B-007 measures them together as evaluation. An amendment when one does.                                                                                                                                          |
| 4   | The fault-isolation wrapper's own overhead (ADR-0005 (c))                                   | ADR-0005 states no cost for it; a constraint is owed by `spec-author` when one is asked for.                                                                                                                                         |
| 5   | The libraries beneath a stage - YamlDotNet, Markdig, JsonSchema.Net - measured on their own | A stage's finding includes them; their own cost is their maintainers'.                                                                                                                                                               |
| 6   | Discovery's git-listing mode (ADR-0001 stage E)                                             | Not built; an amendment when it is.                                                                                                                                                                                                  |
| 7   | Comparing two implementations of a stage                                                    | No alternative exists; a `Baseline` pair is the `benchmarker`'s when an ADR proposes one.                                                                                                                                            |
| 8   | Stage timing or tracing inside the product                                                  | ADR-0005 option 8: a claim asking for stage-level timing or tracing would earn `IStage<TIn, TOut>`. These benchmarks live outside `src/` and change nothing in it (C-11), so they are not that claim and do not meet that condition. |
| 9   | The cost of building the container and resolving the engine                                 | Decision 0001: inside `0109-F4`'s start-to-exit finding; no constraint states it alone (B-008, Withdrawn).                                                                                                                           |

## 6. Concern Separation

<!-- last written by: spec-author, 2026-10-09 -->

Pending: owned by `implementer`, written after agreement.

## 7. Technical Design

<!-- last written by: spec-author, 2026-10-09 -->

Pending: owned by `implementer`, written after agreement.

## 8. Testing Strategy

<!-- last written by: spec-author, 2026-10-09 -->

Pending: owned by `test-writer`, written after agreement.

## 9. Traceability Matrix

<!-- last written by: spec-author, 2026-10-09 -->

| Claim ID | Scenario                                               | Test    | Status    |
| -------- | ------------------------------------------------------ | ------- | --------- |
| B-001    | Loading the manifest and schemas is measured           | Missing | Missing   |
| B-002    | Discovery is measured at every tree size               | Missing | Missing   |
| B-003    | Reading one specification's frontmatter is measured    | Missing | Missing   |
| B-004    | Parsing one specification is measured                  | Missing | Missing   |
| B-005    | Building the model is measured at every tree size      | Missing | Missing   |
| B-006    | Every rule is measured at every tree size              | Missing | Missing   |
| B-007    | Evaluating a model is measured at every tree size      | Missing | Missing   |
| B-008    | Resolving the engine is measured apart from running it | Missing | Withdrawn |
| B-009    | A new rule is measured without a new benchmark         | Missing | Missing   |

## 10. Lessons / Spec Deltas

<!-- last written by: spec-author, 2026-10-09 -->

- 2026-10-09, spec-reviewer finding on commit `4b23e76`: B-006, B-007, C-6 and C-7 said rules and evaluation run over an in-memory model, but the companion-file rule reads `.feature` files during evaluation. They now measure against a generated tree on disk and name the file system. B-008 and C-8 withdrawn (decision 0001). § 5 rows 8 and 9 added.

## 11. Open Questions

<!-- last written by: spec-author, 2026-10-09 -->

None.

## 12. Sign-off

<!-- last written by: spec-author, 2026-10-09 -->

| Section | Status | Reviewer      | Note                   |
| ------- | ------ | ------------- | ---------------------- |
| 1-5     | 🟡     | spec-reviewer | Awaiting first review. |

## Tasks

None yet. Cut from § 3 after agreement.

## Scoring

| Field | Value | Basis          |
| ----- | ----- | -------------- |
| value | 0     | Not yet scored |
| risk  | 0     | Not yet scored |
