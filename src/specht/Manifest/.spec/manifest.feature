Feature: The manifest carries the roles
  As the maintainer of a repository on the specification model
  I want the sections, markers, keys and grammars the rules read to come from a manifest I own
  So that the same rules gate my tree with my names, and hooked's run is unchanged

  Background:
    Given a repository root holding a manifest and the three frontmatter schemas
    And the manifest is the default one this repository checks itself with

  @B-001
  Scenario: A section is read by role
    Given the manifest renames the claims section to "3. Claims" in its section list and its claims role
    And the root holds a specification whose claims table sits under "3. Claims"
    When the check runs
    Then the claims are read from that section
    And no missing-section violation is reported

  @B-002
  Scenario: Table headers are read by role
    Given the manifest's headers for the matrix role name the third column "Proof"
    And the root holds a specification whose traceability table has a "Proof" column in third place
    And the root holds a second specification whose traceability table has a "Test" column in third place
    When the check runs
    Then the table-header rule reports the second specification
    And it does not report the first

  @B-002
  Scenario: Table headers follow a renamed section
    Given the manifest renames the matrix section to "9. Coverage" in its section list and its matrix role
    And the root holds a specification whose "9. Coverage" table lacks one of the matrix role's columns
    When the check runs
    Then the table-header rule reports that specification

  @B-003
  Scenario: Markers are read from the manifest
    Given the manifest's missing-test cell value is "TBD"
    And the root holds an approved specification with a "TBD" cell in its traceability table
    When the check runs
    Then the approved-with-missing-coverage rule reports that row

  @B-004
  Scenario: Frontmatter keys are read from the manifest
    Given the frontmatter schemas are read from the root
    And the manifest and the root's Feature schema both name the children key "tasks"
    And the root holds a specification whose "tasks" names an item that does not exist
    When the check runs
    Then the unresolved-child rule reports it

  @B-005
  Scenario: Identity and edge forms are read from the manifest
    Given the frontmatter schemas are read from the root
    And the manifest and the root's Feature schema both declare the qualified edge form with a colon between the epic and the Feature id
    And the root holds a specification whose dependency edge is written "0002:F1"
    When the check runs
    Then the edge resolves to Feature F1 of epic 0002
    And no frontmatter violation is reported

  @B-006
  Scenario: Item ids follow the task grammar
    Given the frontmatter schemas are read from the root
    And the manifest's task grammar and the root's frontmatter schemas all carry a three-digit sequence
    And the root holds items "0001-001" and "0001-002" beside a specification
    When the check runs
    Then both items are read with epic "0001"
    And the per-epic sequence is reported contiguous
    And no frontmatter violation is reported

  @B-007
  Scenario: Claim tags follow the claim grammar
    Given the manifest's claim grammar accepts a four-digit claim number
    And the root holds a specification declaring claim "B-0001"
    And its companion carries a scenario tagged "B-0001"
    When the check runs
    Then the tag resolves to the claim
    And no unresolved-tag violation is reported

  @B-008
  Scenario: Frontmatter schemas are loaded by the manifest's file names
    Given the frontmatter schemas are read from the root
    And the root holds a specification with no violations
    And the manifest names the Feature schema file "feature.json"
    And the schema folder holds that file and not the default name
    When the check runs
    Then every specification's frontmatter is checked against "feature.json"

  @B-009
  Scenario: Messages name the manifest's values
    Given the manifest renames the claims section to "3. Claims" in its section list and its claims role
    And the root holds a specification with no "3. Claims" section
    When the check runs
    Then the missing-section message names "3. Claims"
    And it names the manifest by its path under the root

  @B-010
  Scenario: A rule's severity is lowered from the manifest
    Given the manifest sets the out-of-order-section rule to warning severity
    And the root holds a specification with its sections out of order
    When the check runs
    Then the violation is reported at warning severity
    And the exit code is 0

  @B-011
  Scenario: A rule is disabled from the manifest
    Given the manifest disables the out-of-order-section rule
    And the root holds a specification with its sections out of order
    When the check runs
    Then nothing is reported for that rule
    And the count of rules evaluated excludes it

  @B-012
  Scenario: An unknown key is invalid
    Given the manifest carries a key the engine does not know
    When the check runs
    Then the manifest is rejected
    And the rejection names that key

  @B-013
  Scenario: An unknown rule id is invalid
    Given the manifest lists rule settings for an id outside the pinned version's vocabulary
    When the check runs
    Then the manifest is rejected
    And the rejection names that rule id

  @B-014
  Scenario: A grammar that does not compile is invalid
    Given the manifest's claim grammar is not a valid expression
    When the check runs
    Then the manifest is rejected
    And the rejection names the claim grammar

  @B-015
  Scenario: A role naming an absent section is invalid
    Given the manifest maps the claims role to a title that is not in its section list
    When the check runs
    Then the manifest is rejected
    And the rejection names the claims role

  @B-016
  Scenario: The default manifest reproduces the baseline verdicts
    Given the baseline tree the engine's tests build
    And the golden report of the violations the engine gave on that tree when the copy landed
    When the check runs with the default manifest
    Then the same violations are reported, in the same order, each with the same rule id, severity, file, line, identifier and message

  @B-017 @boundary
  Scenario: A rule cannot be added from the manifest
    Given the manifest declares a rule by a type name and a library file
    And that library file exists under the root
    And the library's rule leaves a mark under the root when it runs
    When the check runs
    Then the mark does not exist

  @B-018 @boundary
  Scenario: An invalid manifest stops the run before any rule
    Given the manifest carries a key the engine does not know
    And the root holds a specification with three violations
    When the check runs
    Then the manifest is rejected
    And none of the three violations is reported

  @B-019
  Scenario: A key left out of the manifest takes the default
    Given the manifest declares no claim grammar
    And the root holds a specification declaring claim "B-0001"
    When the check runs
    Then the malformed-claim-id rule reports "B-0001" against the default claim grammar

  @B-020
  Scenario: An annotation key is ignored
    Given the manifest carries a "$comment" key and a "$schema" key
    And the root holds a specification with no violations
    When the check runs
    Then the manifest is accepted
    And no violation is reported

  @B-021
  Scenario: A malformed exclusion entry is invalid
    Given the manifest's exclusion list holds the entry "build/output"
    When the check runs
    Then the manifest is rejected
    And the rejection names "build/output"

  @B-022
  Scenario: A rejected manifest exits with the invalid-manifest code
    Given the manifest carries a key the engine does not know
    When the tool runs as a command on the root
    Then the exit code is 3

  @B-023
  Scenario: A rejected manifest's only output is the rejection, on stderr
    Given the manifest carries a key the engine does not know
    When the tool runs as a command on the root
    Then the standard error carries the rejection, naming that key
    And the standard output is empty

  @B-024
  Scenario: With epics, identity is the epic and the id
    Given the manifest declares the epic grammar
    And the root holds a specification declaring epic "0001" and id "F2"
    When the check runs
    Then the specification is read as "0001-F2"

  @B-025
  Scenario: Without epics, identity is the id alone
    Given the manifest declares no epic grammar
    And the root holds a specification declaring id "F2" and no epic
    When the check runs
    Then the specification is read as "F2"

  @B-025 @boundary
  Scenario: Without epics, the folder a specification sits in gives it no epic
    Given the manifest declares no epic grammar
    And the root holds a specification declaring id "F2" and no epic, in a folder named "0001-epic"
    When the check runs
    Then the specification is read as "F2"

  @B-026
  Scenario: Without epics, a repeated id is a duplicate identity
    Given the manifest declares no epic grammar
    And the root holds two specifications in different folders, both declaring id "F2" and no epic
    When the check runs
    Then the duplicate-identity rule reports "F2" once on each specification
    And each report names both paths

  @B-027
  Scenario: Without epics, no specification is left out of the identity and edge checks
    Given the manifest declares no epic grammar
    And the root holds a specification declaring id "F2" and no epic, which depends on "F9"
    And no specification declares id "F9"
    When the check runs
    Then the unresolved-dependency rule reports "F9" on that specification

  @B-028
  Scenario: Without epics, a local edge resolves by id
    Given the manifest declares no epic grammar
    And the root holds a specification "F1" that depends on "F2"
    And a specification "F2" that lists "F1" as blocked by it
    When the check runs
    Then no dependency violation is reported

  @B-029 @boundary
  Scenario: Without epics, a qualified edge names no Feature
    Given the manifest declares no epic grammar
    And the root holds a specification "F1" that depends on "0002/F2"
    And a specification "F2" that lists "F1" as blocked by it
    When the check runs
    Then the unresolved-dependency rule reports "0002/F2" on specification "F1"

  @B-030
  Scenario: Without epics, an item's parent is its Feature's id
    Given the manifest declares no epic grammar
    And the root holds a specification "F2" with an item beside it whose parent is "F2"
    When the check runs
    Then no item-parent violation is reported

  @B-031
  Scenario: With epics, a bare parent is reported
    Given the manifest declares the epic grammar
    And the root holds a specification declaring epic "0001" and id "F2", with an item beside it whose parent is "F2"
    When the check runs
    Then the item-parent rule reports that item

  @B-032
  Scenario: Without epics, items are numbered per Feature
    Given the manifest declares no epic grammar
    And the manifest's task grammar carries no epic part
    And the root holds specifications "F1" and "F2", each with its own items numbered 01 and 02
    When the check runs
    Then no item-sequence violation is reported

  @B-033
  Scenario: Without epics, no epic schema file is needed
    Given the frontmatter schemas are read from the root
    And the manifest declares no epic grammar
    And the root holds no epic frontmatter schema
    When the check runs
    Then the root is checked
    And nothing is reported about a missing schema

  @B-034
  Scenario: Without epics, epic files are not checked
    Given the manifest declares no epic grammar
    And the root holds an epic file whose frontmatter would fail the epic schema
    When the check runs
    Then the epic-frontmatter rule reports nothing

  @B-035
  Scenario: With epics, a missing epic is reported
    Given the manifest declares the epic grammar
    And the root holds a co-located specification declaring id "F2" and no epic
    When the check runs
    Then a missing-epic violation is reported on that specification

  @B-039
  Scenario: Table headers keyed by a section title are invalid
    Given the manifest declares table headers under "9. Traceability Matrix" and not under a role
    When the check runs
    Then the manifest is rejected
    And the rejection names "9. Traceability Matrix"

  @B-040
  Scenario: An empty marker is invalid
    Given the manifest's draft sign-off marker is empty
    When the check runs
    Then the manifest is rejected
    And the rejection names the draft marker
