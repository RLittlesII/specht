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
    Given the manifest names the traceability table's third column "Proof"
    And the root holds a specification whose traceability table has a "Proof" column in third place
    When the check runs
    Then no table-header violation is reported

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
    Given the manifest's claim grammar is not a valid expression
    When the check runs
    Then the exit code is 3

  @B-023
  Scenario: A rejected manifest's only output is the rejection, on stderr
    Given the manifest's claim grammar is not a valid expression
    When the check runs
    Then stderr carries the rejection
    And stdout is empty
