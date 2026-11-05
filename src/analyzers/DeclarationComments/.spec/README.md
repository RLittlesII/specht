---
title: "Specification: Declaration comments"
description: "Reject ordinary line comments on class and method surfaces while retaining XML documentation."
type: feature
id: "F8"
epic: "0201"
spec_status: in-review
status: done
priority: med
value: 0
risk: 0
rank: 0
scored_by: null
scored_on: null
domain: "Code analysis"
author: "spec-author"
milestone: null
children: []
depends_on: []
blocks: []
spikes: []
created: "2026-11-04"
updated: "2026-11-04"
github_issue: null
synced_at: null
---

# Specification: Declaration comments

## 1. Business Goal

<!-- last written by: spec-author, 2026-11-04 -->

Keep class and method surfaces free of ordinary comment prose that repeats design history or narrates implementation, while preserving XML documentation and traceability tags; a compiler diagnostic makes this convention enforceable in editor and build.

## 2. User Needs

<!-- last written by: spec-author, 2026-11-04 -->

| #   | Persona    | Need                                                        | Pain Point Today                                                                 |
| --- | ---------- | ----------------------------------------------------------- | -------------------------------------------------------------------------------- |
| 1   | Maintainer | Reject ordinary line comments on class and method surfaces. | Ordinary commentary can surround valid XML documentation without any diagnostic. |
| 2   | Consumer   | Retain documentation and ordinary reporting controls.       | Removing prose must not require empty summaries or a custom suppression system.  |

### Assumptions

None.

## 3. Acceptance Criteria

<!-- last written by: spec-author, 2026-11-04 -->

| ID    | Claim                                                                                                                                                                                                                                                                  | Source                               | Status |
| ----- | ---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- | ------------------------------------ | ------ |
| B-001 | Given a non-generated class declaration of any accessibility, including nested and partial declarations, this Feature reports each ordinary `//` surface comment once at that comment's source span as `SPCT0013`.                                                     | Owner request, 2026-11-04            | Active |
| B-002 | Given a non-generated method declaration of any accessibility in any containing type, including abstract, interface, explicit-interface and partial methods, this Feature reports each ordinary `//` surface comment once at that comment's source span as `SPCT0013`. | Owner request, 2026-11-04            | Active |
| B-003 | Given a class or method surface containing only `///` XML documentation or no comments, this Feature emits no declaration-comment diagnostic.                                                                                                                          | Owner request, 2026-11-04            | Active |
| B-004 | Given an ordinary `//` comment anywhere in a class or method declaration header, this Feature treats it as a surface comment, including before, inside or between attributes, modifiers and signature tokens, up to the body or declaration terminator.                | Decision 0001                        | Active |
| B-005 | Given ordinary `//` surface comments mixed with `///` XML documentation, this Feature reports every ordinary surface comment regardless of its position relative to the documentation.                                                                                 | Owner example; decision 0001         | Active |
| B-006 | Given comments outside class or method surfaces, this Feature emits no declaration-comment diagnostic for them.                                                                                                                                                        | Decision 0001                        | Active |
| B-007 | Given an ordinary comment block at the start of a file separated from the first following code or documentation by at least one empty line, this Feature excludes that file header from declaration surface comments.                                                  | Decision 0001                        | Active |
| B-008 | Given generated source identified by standard Roslyn generated-code recognition, including generated file names and an auto-generated header, this Feature emits no declaration-comment diagnostic.                                                                    | Decision 0001                        | Active |
| B-009 | Given source without manifest, specification records or valid traceability configuration, this Feature still analyzes its class and method surface comments.                                                                                                           | Decision 0001                        | Active |
| B-010 | Given an ordinary surface comment without a reporting override, this Feature publishes `SPCT0013` as an enabled warning.                                                                                                                                               | Decision 0001                        | Active |
| B-011 | Given standard Roslyn diagnostic configuration or suppression for `SPCT0013`, this Feature honors that reporting control.                                                                                                                                              | 0201-F6 decision 0002; decision 0001 | Active |

## 4. Constraints

<!-- last written by: spec-author, 2026-11-04 -->

| ID  | Constraint                                                                                                                                                                                                | Rules Out                                                                                                                                                                                                 |
| --- | --------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- | --------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| C-1 | Analysis is deterministic, offline and read-only.                                                                                                                                                         | Repository crawling, network access and source edits during analysis.                                                                                                                                     |
| C-2 | The checked declarations are CSharp `class` declarations and method declarations, independently of accessibility.                                                                                         | Treating constructors, local functions, records, structs, interfaces, delegates, properties, fields or events as additional checked declaration kinds; methods inside any containing type remain checked. |
| C-3 | Surface comments are ordinary line-comment trivia in a selected declaration's leading material and header; the header ends before its opening body brace, expression-body arrow or terminating semicolon. | Method-body, expression-body, class-member-body and post-declaration trailing comments; comments belonging only to a prior token or directive.                                                            |
| C-4 | A file-header exemption requires a leading ordinary comment block before the file's first code or XML documentation, followed by a line containing only whitespace or nothing.                            | Exempting declaration commentary solely because it is at the top of a file, or treating a blank line later in a declaration header as a file header.                                                      |
| C-5 | `SPCT0013` follows analyzer package releases and standard reporting controls.                                                                                                                             | Changing the checker's pinned `SPEC` vocabulary or introducing a manifest switch, custom suppression syntax or required specification inputs for this rule.                                               |
| C-6 | The rule distinguishes comment syntax rather than judging prose or XML content.                                                                                                                           | Requiring a summary, validating XML, banning block comments or interpreting comment text to infer design intent.                                                                                          |

## 5. Out of Scope

<!-- last written by: spec-author, 2026-11-04 -->

| #   | Item                                                                                     | Exclusion Reason                                                                                                         |
| --- | ---------------------------------------------------------------------------------------- | ------------------------------------------------------------------------------------------------------------------------ |
| 1   | Ordinary comments within bodies or after a completed declaration                         | The requested boundary is the declaration surface; B-006 documents this limit.                                           |
| 2   | Other declaration kinds, including constructors, local functions and record declarations | C-2 limits the new rule to classes and methods.                                                                          |
| 3   | Suppression explanations on directives                                                   | They belong to the directive, preserve the suppression-reason convention, and are covered by B-006.                      |
| 4   | Requiring XML documentation or summaries                                                 | B-003 permits traceability-only XML and uncommented declarations; existing documentation enforcement has its own scope.  |
| 5   | XML binding validation and evidence completeness                                         | 0201-F2 and 0201-F6 retain those responsibilities.                                                                       |
| 6   | Code fixes, automatic deletion or conversion to XML                                      | This change reports unwanted comments; it does not infer which prose should survive.                                     |
| 7   | General coding-convention analyzer families                                              | This is the explicit declaration-comment exception recorded in decision 0001, not an expansion to unrelated conventions. |
| 8   | Block-comment restrictions                                                               | The request concerns ordinary `//` and XML `///`; C-6 leaves other comment syntax unchanged.                             |

## 6. Concern Separation

<!-- last written by: implementer, 2026-11-04 -->

| Concern                                           | Owner                                   | Boundary                                                                |
| ------------------------------------------------- | --------------------------------------- | ----------------------------------------------------------------------- |
| Declaration selection and surface trivia          | `DeclarationCommentAnalyzer`            | Source syntax only, without binding resolution or specification inputs. |
| Generated-code recognition and reporting controls | Roslyn analyzer driver                  | Standard generated-code flags, diagnostic severity and suppression.     |
| XML bindings and evidence                         | Existing 0201-F2 and 0201-F6 components | No change to their semantics or input requirements.                     |

## 7. Technical Design

<!-- last written by: implementer, 2026-11-04 -->

`DeclarationCommentAnalyzer.cs` registers a CSharp syntax-tree action selecting
class and method declarations, with concurrent execution enabled and
generated-code analysis disabled. For each
declaration it examines ordinary line-comment trivia within the span from its
leading material through the header, stopping before the body brace,
expression-body arrow or semicolon. Structured trivia is not traversed, so XML
and directive explanations retain their own syntax boundaries. Each offending
trivia produces one diagnostic at its own location; no semantic model or file
system access is needed.

The initial ordinary comment block is exempt only when the compilation unit's
leading trivia contains an empty or whitespace-only line after that block and
before code or XML documentation. This cutoff is computed once per syntax tree;
later blank lines do not exempt declaration comments.

| Diagnostic | Meaning                                            | Default         | Controls                                               |
| ---------- | -------------------------------------------------- | --------------- | ------------------------------------------------------ |
| `SPCT0013` | Ordinary line comment on a class or method surface | Enabled warning | Standard Roslyn severity and suppression; no code fix. |

The diagnostic is delivered in the existing analyzer assembly and documented in
`src/codeanalysis/docs/SPCT0013.md`; the checker schema and manifest are unchanged.

## 8. Testing Strategy

<!-- last written by: test-writer, 2026-11-04 -->

Unit tests in `test/analysis.tests/DeclarationComments/` use synthetic CSharp
sources and the existing analyzer harness. Cases assert exact comment spans,
counts, relative paths, accessibility, nested and partial declarations, methods
in different containing types, header positions and mixed XML. Separate cases
exercise XML-only surfaces, other declarations, bodies, trailing trivia, file
headers, generated source and standard reporting controls. Compiler errors in
fixture source fail the surface and allowed-location tests.

The literal Feature link in `test/acceptance/acceptance.csproj` runs the companion
scenarios through `DeclarationCommentSteps`, which compiles synthetic sources
and invokes the real analyzer driver without specification inputs. Scenarios
assert exact spans and counts as well as severity and suppression.

Red verification on `net11.0` against an empty analyzer callback: 75 unit cases,
44 failing because required diagnostics were absent and 31 passing boundary or
contract cases. The 39 acceptance cases then produced 22 missing-diagnostic failures and 17
passing boundary cases against the same scaffold. The implementation passed all
75 initial unit cases and all 39 acceptance cases. A subsequent
semicolon-terminated-class case failed with a missing diagnostic and drove the
class delimiter correction; the added primary-constructor-header case passed.
Final verification results are recorded in the delivery report.

## 9. Traceability Matrix

<!-- last written by: test-writer, 2026-11-04 -->

| Claim ID | Scenario                                                                     | Test                                                                                                                                                                                                                                                  | Status  |
| -------- | ---------------------------------------------------------------------------- | ----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- | ------- |
| B-001    | `@B-001` Class surface comments                                              | `DeclarationCommentSteps`; `DeclarationCommentAnalyzerUnitTests.SurfaceComments_WhenAnalyzedWithoutSpecificationInputs_ShouldReportEachCommentOnceAtItsSpan; PartialDeclarations_WhenAnalyzed_ShouldReportEachPartIndependently`                      | Covered |
| B-002    | `@B-002` Method surface comments                                             | `DeclarationCommentSteps`; `DeclarationCommentAnalyzerUnitTests.SurfaceComments_WhenAnalyzedWithoutSpecificationInputs_ShouldReportEachCommentOnceAtItsSpan`                                                                                          | Covered |
| B-003    | `@B-003` XML documentation remains allowed                                   | `DeclarationCommentSteps`; `DeclarationCommentAnalyzerUnitTests.DocumentationAndOtherLocations_WhenAnalyzed_ShouldProduceNoDeclarationCommentDiagnostic`                                                                                              | Covered |
| B-004    | `@B-004` Comments throughout the declaration header                          | `DeclarationCommentSteps`; `DeclarationCommentAnalyzerUnitTests.SurfaceComments_WhenAnalyzedWithoutSpecificationInputs_ShouldReportEachCommentOnceAtItsSpan`                                                                                          | Covered |
| B-005    | `@B-005` XML documentation does not hide ordinary comments                   | `DeclarationCommentSteps`; `DeclarationCommentAnalyzerUnitTests.SurfaceComments_WhenAnalyzedWithoutSpecificationInputs_ShouldReportEachCommentOnceAtItsSpan`                                                                                          | Covered |
| B-006    | `@B-006` Other comment locations remain outside the rule                     | `DeclarationCommentSteps`; `DeclarationCommentAnalyzerUnitTests.DocumentationAndOtherLocations_WhenAnalyzed_ShouldProduceNoDeclarationCommentDiagnostic`                                                                                              | Covered |
| B-007    | `@B-007` Separate file header; A blank line does not exempt a method comment | `DeclarationCommentSteps`; `DeclarationCommentAnalyzerUnitTests.SurfaceComments_WhenAnalyzedWithoutSpecificationInputs_ShouldReportEachCommentOnceAtItsSpan; DocumentationAndOtherLocations_WhenAnalyzed_ShouldProduceNoDeclarationCommentDiagnostic` | Covered |
| B-008    | `@B-008` Generated source is excluded                                        | `DeclarationCommentSteps`; `DeclarationCommentAnalyzerUnitTests.GeneratedSource_WhenAnalyzed_ShouldProduceNoDeclarationCommentDiagnostic`                                                                                                             | Covered |
| B-009    | `@B-009` No specification inputs required                                    | `DeclarationCommentSteps`; `DeclarationCommentAnalyzerUnitTests.SurfaceComments_WhenAnalyzedWithoutSpecificationInputs_ShouldReportEachCommentOnceAtItsSpan`                                                                                          | Covered |
| B-010    | `@B-010` Default warning                                                     | `DeclarationCommentSteps`; `DeclarationCommentAnalyzerUnitTests.DiagnosticContract_WhenLoaded_ShouldPublishAnEnabledWarningWithHelp`                                                                                                                  | Covered |
| B-011    | `@B-011` Standard reporting controls                                         | `DeclarationCommentSteps`; `DeclarationCommentAnalyzerUnitTests.ReportingOverride_WhenAnalyzed_ShouldHonorTheCompilerControl; SuppressionPragma_WhenAnalyzed_ShouldSuppressTheCommentDiagnostic`                                                      | Covered |

## 10. Lessons / Spec Deltas

<!-- last written by: spec-author, 2026-11-04 -->

| Trigger                                                                                      | Change                                                                                                            | Affected IDs               |
| -------------------------------------------------------------------------------------------- | ----------------------------------------------------------------------------------------------------------------- | -------------------------- |
| Owner request, 2026-11-04: XML documentation permits surrounding ordinary declaration prose. | Add a dedicated declaration-comment diagnostic and a narrow exception to epic 0201's generic-convention boundary. | B-001 to B-011; C-1 to C-6 |

## 11. Open Questions

<!-- last written by: spec-author, 2026-11-04 -->

None.

## 12. Sign-off

<!-- last written by: spec-reviewer, 2026-11-04 -->

| Section | Status | Reviewer      | Note                                                                                                                                                                                                                                                                                                                                                                                                                                                |
| ------- | ------ | ------------- | --------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| 1–12    | 🟢     | spec-reviewer | 2026-11-04: No remaining findings against B-001 to B-011 and C-1 to C-6. Exact spans, declaration scope, XML allowance, file-header exemption and standard reporting controls match decision 0001. The focused net11.0 runs pass 77 unit cases and 39 scenarios without skips; the full build passes all gates, with 10,006 passing cases and 230 existing skips across configured frameworks. CI and merge are not certified by this local review. |

## Tasks

| Item   | Claims         |
| ------ | -------------- |
| `0359` | B-001 to B-011 |

## Scoring

| Field | Value | Basis           |
| ----- | ----- | --------------- |
| value | 0     | Not yet scored. |
| risk  | 0     | Not yet scored. |
