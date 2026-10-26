Feature: Native Gherkin claims
  Under a manifest that selects native claim input, the check builds a
  Feature's claims from the claim tags in the .feature files a claim file set
  names, applies the claim rules to that catalogue, and reports a scenario with
  no claim, a claim tag where none can be declared and a claim file it cannot
  read as rules of the next schema version.

  Background:
    Given a root with a manifest pinned to schema "0.3.0"
    And the manifest sets "claim.source" to "gherkin"
    And "claim.files" holds one claim file set with glob ".spec/features/arithmetic/*.feature" and feature "arithmetic"
    And the manifest leaves "claim.tag" and "identifiers.claim" at their defaults

  @B-001
  Scenario: Claims come from the claim files a set names
    Given ".spec/features/arithmetic/add.feature" declares scenarios tagged "@B-001" and "@B-002"
    When the check runs
    Then Feature "arithmetic" has the claims "B-001" and "B-002"
    And no claims table is read for Feature "arithmetic"

  @B-002
  Scenario: Without the selection nothing changes
    Given the manifest has no "claim.source"
    And the manifest has no "claim.files"
    When the check runs
    Then every claim comes from a specification's claims table
    And the claim file set declares no Feature

  @B-003
  Scenario: A record supplies lifecycle and edges
    Given the claim file set names the record ".spec/features/arithmetic/README.md"
    And the record's frontmatter sets "spec_status" to "approved" and "depends_on" to "F2"
    When the check runs
    Then Feature "arithmetic" has the lifecycle "approved"
    And Feature "arithmetic" depends on "F2"
    And the record's frontmatter is held to the Feature schema

  @B-003
  Scenario: No record means a current Feature with no edges
    Given the claim file set names no record
    When the check runs
    Then Feature "arithmetic" has the lifecycle "current"
    And Feature "arithmetic" declares no dependency edge

  @B-004
  Scenario Outline: A claim tag declares a claim at its element
    Given ".spec/features/arithmetic/add.feature" carries "@B-001" on a <element> at line <line>
    When the check runs
    Then Feature "arithmetic" has the claim "B-001" located at ".spec/features/arithmetic/add.feature" line <line>

    Examples:
      | element          | line |
      | Feature          | 1    |
      | Rule             | 4    |
      | Scenario         | 7    |
      | Scenario Outline | 12   |

  @B-005
  Scenario: An ordinary tag declares nothing
    Given ".spec/features/arithmetic/add.feature" has a scenario tagged "@B-001" and "@smoke" and "@B-01"
    When the check runs
    Then Feature "arithmetic" has exactly the claim "B-001"
    And no violation names "@smoke" or "@B-01"

  @B-006
  Scenario: An outline declares one claim
    Given ".spec/features/arithmetic/add.feature" has a Scenario Outline tagged "@B-003" with three Examples rows
    When the check runs
    Then Feature "arithmetic" has the claim "B-003" exactly once
    And the claim is located at the Scenario Outline's keyword line

  @B-007
  Scenario: A Rule declares once for its scenarios
    Given ".spec/features/arithmetic/add.feature" has a Rule tagged "@B-004" holding two untagged scenarios
    When the check runs
    Then Feature "arithmetic" has the claim "B-004" exactly once
    And the claim is located at the Rule's keyword line
    And no violation names either scenario

  @B-008
  Scenario: One identifier on two elements is reported at both
    Given ".spec/features/arithmetic/add.feature" tags a scenario "@B-001" at line 7
    And ".spec/features/arithmetic/subtract.feature" tags a scenario "@B-001" at line 5
    When the check runs
    Then the output has the line ".spec/features/arithmetic/add.feature(7): error SPEC030: duplicate claim id [B-001]"
    And the output has the line ".spec/features/arithmetic/subtract.feature(5): error SPEC030: duplicate claim id [B-001]"

  @B-009
  Scenario: A claim tag on an Examples block is reported
    Given ".spec/features/arithmetic/add.feature" has an Examples block tagged "@B-005" at line 14
    When the check runs
    Then the output has a line ".spec/features/arithmetic/add.feature(14): error SPEC026:" naming "B-005"
    And Feature "arithmetic" has no claim "B-005"

  @B-010
  Scenario: A scenario with no claim is reported
    Given ".spec/features/arithmetic/add.feature" has a scenario at line 9 with no claim tag of its own, its Rule or its Feature
    When the check runs
    Then the output has a line ".spec/features/arithmetic/add.feature(9): warning SPEC025:"
    And the exit code is 0 without "--strict"

  @B-010
  Scenario: An inherited claim satisfies the untagged scenario rule
    Given ".spec/features/arithmetic/add.feature" tags its Feature "@B-006" and holds an untagged scenario
    When the check runs
    Then no violation is reported for the scenario

  @B-011
  Scenario: Reordering and retitling keep every claim
    Given a catalogue built from ".spec/features/arithmetic/add.feature"
    And the file's scenarios are reordered and retitled with their tags kept
    When the check runs again
    Then every claim has the same identifier, Feature identity and lifecycle as before

  @B-012
  Scenario Outline: Lifecycle tags give the claim its status
    Given the manifest sets "claim.withdrawnTags" to "@withdrawn" and "claim.supersededTags" to "@superseded"
    And ".spec/features/arithmetic/add.feature" has a scenario tagged "@B-001" <and tags>
    When the check runs
    Then the claim "B-001" has the status "<status>"

    Examples:
      | and tags                       | status     |
      | and nothing else               | Active     |
      | and "@withdrawn"               | Withdrawn  |
      | and "@superseded"              | Superseded |
      | and "@withdrawn" "@superseded" | Superseded |

  @B-013
  Scenario: A planned scenario keeps its claim
    Given the manifest leaves "claim.plannedTags" at its default
    And ".spec/features/arithmetic/add.feature" has a scenario tagged "@B-001" and "@ignore"
    When the check runs
    Then Feature "arithmetic" has the claim "B-001"
    And the scenario is marked planned in the catalogue

  @B-013
  Scenario: A planned Examples block keeps its outline's claim
    Given ".spec/features/arithmetic/add.feature" has a Scenario Outline tagged "@B-003" with an Examples block tagged "@ignore"
    When the check runs
    Then Feature "arithmetic" has the claim "B-003"
    And that block's rows are marked planned in the catalogue

  @B-014
  Scenario: A claim file in another language is reported
    Given ".spec/features/arithmetic/add.feature" opens with "# language: de" at line 1
    When the check runs
    Then the output has a line ".spec/features/arithmetic/add.feature(1): error SPEC027:"
    And Feature "arithmetic" has no claim from that file

  @B-015
  Scenario: A claim file with no Feature line is reported
    Given ".spec/features/arithmetic/notes.feature" has no "Feature:" line
    When the check runs
    Then the output has a line ".spec/features/arithmetic/notes.feature(0): error SPEC027:"
    And Feature "arithmetic" has no claim from that file

  @B-016
  Scenario: A set matching no file is reported
    Given no file matches ".spec/features/arithmetic/*.feature" outside the exclusions
    When the check runs
    Then the output has a line ".spec/features/arithmetic/*.feature(0): error SPEC027:"
    And the exit code is 1

  @B-017
  Scenario: One identity declared by a set and a discovered specification
    Given the manifest declares no epic grammar
    And the layouts discover "src/arithmetic/.spec/README.md" with id "arithmetic"
    When the check runs
    Then the output has a line "src/arithmetic/.spec/README.md(" naming "SPEC012"
    And the output has a line naming "SPEC012" for the claim file set

  @B-017
  Scenario: One identity declared by two sets
    Given the manifest declares a second claim file set with glob ".spec/features/sums/*.feature" and feature "arithmetic"
    When the check runs
    Then the output has two lines naming "SPEC012" with the identifier "arithmetic"

  @B-018
  Scenario: A file two sets match is reported
    Given the manifest declares a second claim file set with glob ".spec/features/**/add.feature" and feature "sums"
    And ".spec/features/arithmetic/add.feature" exists
    When the check runs
    Then the output has a line ".spec/features/arithmetic/add.feature(0): error SPEC027:"
    And neither Feature "arithmetic" nor Feature "sums" has a claim from that file

  @B-019
  Scenario: A Feature-level tag that disagrees with its set is reported
    Given the manifest sets "claim.scopes.featureTag" to "@spec.feature:{feature}"
    And ".spec/features/arithmetic/add.feature" tags its Feature "@spec.feature:sums" at line 1
    When the check runs
    Then the output has a line ".spec/features/arithmetic/add.feature(1): error SPEC027:"

  @B-019
  Scenario: A Feature-level tag that agrees, or none, is not reported
    Given the manifest sets "claim.scopes.featureTag" to "@spec.feature:{feature}"
    And ".spec/features/arithmetic/add.feature" tags its Feature "@spec.feature:arithmetic"
    And ".spec/features/arithmetic/subtract.feature" has no Feature-level tag
    When the check runs
    Then no "SPEC027" is reported for either file

  @B-020
  Scenario: A record needs every section but the claims table
    Given the claim file set names the record ".spec/features/arithmetic/README.md"
    And the record holds every manifest section except "3. Acceptance Criteria"
    When the check runs
    Then no "SPEC010" is reported for the record
    And no "SPEC013" is reported for the record's claims section

  @B-020
  Scenario: A record missing another section is still reported
    Given the claim file set names the record ".spec/features/arithmetic/README.md"
    And the record lacks the section "9. Traceability Matrix"
    When the check runs
    Then the output has a line for the record naming "SPEC010"

  @B-021
  Scenario: The companion rules leave a native Feature alone
    Given the claim file set names the record ".spec/features/arithmetic/README.md"
    And three claim files sit beside the record
    When the check runs
    Then no "SPEC020" is reported for Feature "arithmetic"
    And no "SPEC021" is reported for Feature "arithmetic"

  @B-022
  Scenario: A claim file is not a stray companion
    Given ".spec/features/arithmetic/add.feature" has no specification beside it
    When the check runs
    Then no "SPEC024" is reported for ".spec/features/arithmetic/add.feature"

  @B-023
  Scenario: A record's matrix is held to the native claims
    Given the claim file set names the record ".spec/features/arithmetic/README.md"
    And the record's "9. Traceability Matrix" has one row per claim in the claim files
    When the check runs
    Then no "SPEC031" is reported for the record
    And each row's scenario resolves against the set's claim files

  @B-023
  Scenario: A record's matrix missing a native claim is reported
    Given the claim file set names the record ".spec/features/arithmetic/README.md"
    And the claim files declare "B-001" and "B-002" and the record's matrix lists only "B-001"
    When the check runs
    Then the output has a line for the record naming "SPEC031" with the identifier "B-002"

  @B-024
  Scenario: Without a matrix the trace says none
    Given the claim file set names no record
    When the check runs
    Then no "SPEC031" and no rule of Feature "0001-F12" is reported for Feature "arithmetic"
    When "specht trace" runs
    Then each claim of Feature "arithmetic" has the status "none"

  @B-025
  Scenario Outline: The scenarios carrying a claim
    Given ".spec/features/arithmetic/add.feature" carries "@B-001" on <element>
    When the check runs
    Then the scenarios of claim "B-001" are <scenarios>

    Examples:
      | element                  | scenarios                                       |
      | a Scenario               | that Scenario                                   |
      | a Scenario Outline       | that Scenario Outline                           |
      | a Rule with two children | both scenarios under the Rule                   |
      | the Feature              | every Scenario and Scenario Outline in the file |

  @B-026
  Scenario: A native Feature is counted and listed by its record
    Given the claim file set names the record ".spec/features/arithmetic/README.md"
    When the check runs with "--json"
    Then the summary counts Feature "arithmetic" among "features"
    And the report lists Feature "arithmetic" at ".spec/features/arithmetic/README.md"

  @B-026
  Scenario: A native Feature with no record is listed by its first claim file
    Given the claim file set names no record
    And the glob matches ".spec/features/arithmetic/subtract.feature" and ".spec/features/arithmetic/add.feature"
    When the check runs with "--json"
    Then the report lists Feature "arithmetic" at ".spec/features/arithmetic/add.feature"

  @B-027
  Scenario: A set's identity carries an epic under the epic grammar
    Given the manifest declares "identifiers.epic"
    And the claim file set has epic "0001" and feature "arithmetic"
    When the check runs
    Then Feature "arithmetic" has the identity "0001" "arithmetic"

  @B-027
  Scenario: A set with an epic under no epic grammar is an invalid manifest
    Given the manifest declares no epic grammar
    And the claim file set has epic "0001"
    When the check runs
    Then the exit code is 3
    And stderr names "claim.files"

  @B-028
  Scenario: An earlier pin is untouched
    Given the manifest is pinned to schema "0.2.0"
    And "claim" has no member beyond "tag" and "tagIgnoreCase"
    When the check runs
    Then the output equals the output of the same tree before this Feature

  @B-028
  Scenario: An earlier pin refuses the new members as today
    Given the manifest is pinned to schema "0.2.0"
    And the manifest sets "claim.source" to "gherkin"
    When the check runs
    Then the exit code is 3
    And stderr names "claim.source"

  @B-029
  Scenario Outline: An invalid claim file set exits before any rule
    Given the manifest's "claim.files" is <shape>
    When the check runs
    Then the exit code is 3
    And stderr names "claim.files"
    And no violation is printed

    Examples:
      | shape                                        |
      | absent                                       |
      | an empty list                                |
      | one entry with no "glob"                     |
      | one entry with no "feature"                  |
      | one entry beside "claim.source" "markdown"   |

  @B-030
  Scenario: The same tree gives the same output
    Given the glob matches ".spec/features/arithmetic/subtract.feature" and ".spec/features/arithmetic/add.feature"
    When the check runs twice
    Then the two outputs are byte-identical
    And the claims of ".spec/features/arithmetic/add.feature" precede those of ".spec/features/arithmetic/subtract.feature"

  @boundary
  Scenario: A claim is never inferred from a title or a step
    Given ".spec/features/arithmetic/add.feature" has an untagged scenario titled "B-009 adds two numbers"
    When the check runs
    Then Feature "arithmetic" has no claim "B-009"
    And the scenario is reported under "SPEC025"

  @boundary
  Scenario: No file is written
    Given the claim file set names no record
    When the check runs
    Then no file under the root is created or changed

  @boundary
  Scenario: Nothing is read from codeAnalysis
    Given the manifest's "codeAnalysis" section sets "records.claimSource" to "gherkin" and nothing under "claim"
    When the check runs
    Then every claim comes from a specification's claims table
    And no verdict depends on the "codeAnalysis" section

  @boundary
  Scenario: The analyzer is untouched
    Given a consumer that also runs the analyzer with the same manifest
    When the check runs
    Then the analyzer's diagnostics, policy and export are what they were before this Feature
