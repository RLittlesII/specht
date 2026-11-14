Feature: Native Gherkin claims
  Under a manifest that selects native claim input, the check builds a
  Feature's claims from the claim tags in the .feature files a claim file set
  names, applies the claim rules to that catalogue, and reports a scenario with
  no claim, a claim tag where none can be declared and a claim file it cannot
  read as rules of the next schema version.

  Background:
    Given a root with a manifest pinned to schema "0.4.0"
    And the manifest's "exclusions" does not hold "/.spec"
    And the manifest declares no epic grammar
    And the manifest sets "claim.source" to "gherkin"
    And "claim.files" holds one claim file set with glob ".spec/features/arithmetic/*.feature" and feature "F1"
    And the manifest leaves "claim.tag" and "ids.claim" at their defaults

  @B-001
  Scenario: Claims come from the claim files a set names
    Given ".spec/features/arithmetic/add.feature" declares scenarios tagged "@B-001" and "@B-002"
    When the check runs
    Then Feature "F1" has the claims "B-001" and "B-002"
    And no claims table is read for Feature "F1"

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
    Then Feature "F1" has the lifecycle "approved"
    And Feature "F1" depends on "F2"
    And the record's frontmatter is held to the Feature schema

  @B-003
  Scenario: No record means a current Feature with no edges
    Given the claim file set names no record
    And the glob matches ".spec/features/arithmetic/add.feature"
    When the check runs
    Then Feature "F1" has the lifecycle "current"
    And Feature "F1" declares no dependency edge

  @B-004
  Scenario Outline: A claim tag declares a claim at its element
    Given ".spec/features/arithmetic/add.feature" carries "@B-001" on a <element> at line <line>
    When the check runs
    Then Feature "F1" has the claim "B-001" located at ".spec/features/arithmetic/add.feature" line <line>

    Examples:
      | element          | line |
      | Feature          | 2    |
      | Rule             | 4    |
      | Scenario         | 7    |
      | Scenario Outline | 12   |

  @B-005
  Scenario: An ordinary tag declares nothing
    Given ".spec/features/arithmetic/add.feature" has a scenario tagged "@B-001" and "@smoke" and "@B-01"
    When the check runs
    Then Feature "F1" has exactly the claim "B-001"
    And no violation names "@smoke" or "@B-01"

  @B-006
  Scenario: An outline declares one claim
    Given ".spec/features/arithmetic/add.feature" has a Scenario Outline tagged "@B-003" with three Examples rows
    When the check runs
    Then Feature "F1" has the claim "B-003" exactly once
    And the claim is located at the Scenario Outline's keyword line

  @B-007
  Scenario: A Rule declares once for its scenarios
    Given ".spec/features/arithmetic/add.feature" has a Rule tagged "@B-004" holding two untagged scenarios
    When the check runs
    Then Feature "F1" has the claim "B-004" exactly once
    And the claim is located at the Rule's keyword line
    And no violation names either scenario

  @B-008
  Scenario: One identifier on two elements is reported at both
    Given ".spec/features/arithmetic/add.feature" tags a scenario "@B-001" at line 7
    And ".spec/features/arithmetic/subtract.feature" tags a scenario "@B-001" at line 5
    When the check runs
    Then the output has the line ".spec/features/arithmetic/add.feature(7): error SPEC4003: duplicate claim id [B-001]"
    And the output has the line ".spec/features/arithmetic/subtract.feature(5): error SPEC4003: duplicate claim id [B-001]"

  @B-009
  Scenario: A claim tag on an Examples block is reported
    Given ".spec/features/arithmetic/add.feature" has an Examples block tagged "@B-005" at line 14
    When the check runs
    Then the output has a line ".spec/features/arithmetic/add.feature(14): error SPEC026:" naming "B-005"
    And Feature "F1" has no claim "B-005"

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
    Then Feature "F1" has the claim "B-001"
    And the scenario is marked planned in the catalogue

  @B-013
  Scenario: A planned Examples block keeps its outline's claim
    Given ".spec/features/arithmetic/add.feature" has a Scenario Outline tagged "@B-003" with an Examples block tagged "@ignore"
    When the check runs
    Then Feature "F1" has the claim "B-003"
    And that block's rows are marked planned in the catalogue

  @B-014
  Scenario: A claim file in another language is reported
    Given ".spec/features/arithmetic/add.feature" opens with "# language: de" at line 1
    When the check runs
    Then the output has a line ".spec/features/arithmetic/add.feature(1): error SPEC027:"
    And Feature "F1" has no claim from that file

  @B-015
  Scenario: A claim file with no Feature line is reported
    Given ".spec/features/arithmetic/notes.feature" has no "Feature:" line
    When the check runs
    Then the output has a line ".spec/features/arithmetic/notes.feature: error SPEC027:"
    And Feature "F1" has no claim from that file

  @B-016
  Scenario: A set matching no file is reported
    Given no file matches ".spec/features/arithmetic/*.feature" outside the exclusions
    When the check runs
    Then the output has a line ".spec/features/arithmetic/*.feature: error SPEC027:"
    And the exit code is 1

  @B-017
  Scenario: One identity declared by a set and a discovered specification
    Given the claim file set names no record
    And the glob matches ".spec/features/arithmetic/add.feature"
    And the layouts discover "src/arithmetic/.spec/README.md" with id "F1"
    When the check runs
    Then the output has a line "src/arithmetic/.spec/README.md(" naming "SPEC3002"
    And the output has a line ".spec/features/arithmetic/add.feature:" naming "SPEC3002"

  @B-017
  Scenario: One identity declared by two sets
    Given the manifest declares a second claim file set with glob ".spec/features/sums/*.feature" and feature "F1"
    And neither set names a record
    And the globs match ".spec/features/arithmetic/add.feature" and ".spec/features/sums/total.feature"
    When the check runs
    Then the output has a line ".spec/features/arithmetic/add.feature:" naming "SPEC3002" with the identifier "F1"
    And the output has a line ".spec/features/sums/total.feature:" naming "SPEC3002" with the identifier "F1"

  @B-018
  Scenario: A file two sets match is reported
    Given the manifest declares a second claim file set with glob ".spec/features/**/add.feature" and feature "F2"
    And ".spec/features/arithmetic/add.feature" exists
    When the check runs
    Then the output has a line ".spec/features/arithmetic/add.feature: error SPEC027:"
    And neither Feature "F1" nor Feature "F2" has a claim from that file

  @B-019
  Scenario: A Feature-level tag that disagrees with its set is reported
    Given the manifest sets "claim.scopes.featureTag" to "@spec.feature:{feature}"
    And ".spec/features/arithmetic/add.feature" tags its Feature "@spec.feature:F2" at line 1
    When the check runs
    Then the output has a line ".spec/features/arithmetic/add.feature(1): error SPEC027:"

  @B-019
  Scenario: A Feature-level tag that agrees, or none, is not reported
    Given the manifest sets "claim.scopes.featureTag" to "@spec.feature:{feature}"
    And ".spec/features/arithmetic/add.feature" tags its Feature "@spec.feature:F1"
    And ".spec/features/arithmetic/subtract.feature" has no Feature-level tag
    When the check runs
    Then no "SPEC027" is reported for either file

  @B-020
  Scenario: A record needs every section but the claims table
    Given the claim file set names the record ".spec/features/arithmetic/README.md"
    And the record holds every manifest section except "3. Acceptance Criteria"
    When the check runs
    Then no "SPEC2001" is reported for the record
    And no "SPEC2002" is reported for the record's claims section

  @B-020
  Scenario: A record missing another section is still reported
    Given the claim file set names the record ".spec/features/arithmetic/README.md"
    And the record lacks the section "9. Traceability Matrix"
    When the check runs
    Then the output has a line for the record naming "SPEC2001"

  @B-021
  Scenario: The companion rules leave a native Feature alone
    Given the claim file set names the record ".spec/features/arithmetic/README.md"
    And three claim files sit beside the record
    When the check runs
    Then no "SPEC4001" is reported for Feature "F1"
    And no "SPEC4002" is reported for Feature "F1"

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
    Then no "SPEC4004" is reported for the record
    And each row's scenario resolves against the set's claim files

  @B-023
  Scenario: A record's matrix missing a native claim is reported
    Given the claim file set names the record ".spec/features/arithmetic/README.md"
    And the claim files declare "B-001" and "B-002" and the record's matrix lists only "B-001"
    When the check runs
    Then the output has a line for the record naming "SPEC4004" with the identifier "B-002"

  @B-024
  Scenario: Without a matrix the trace says none
    Given the claim file set names no record
    When the check runs
    Then no "SPEC4004" and no rule of Feature "0001-F12" is reported for Feature "F1"
    When "specht trace" runs
    Then each claim of Feature "F1" has the status "none"

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
  Scenario: A native Feature is counted under a claims entry in the summary
    Given the glob matches ".spec/features/arithmetic/add.feature"
    When the check runs
    Then the summary lines count 1 under "claims", after the manifest's layouts

  @B-026
  Scenario: A native Feature is counted under a claims entry in the report
    Given the glob matches ".spec/features/arithmetic/add.feature"
    When the check runs with "--json"
    Then the report's layouts count 1 under "claims", after the manifest's layouts

  @B-026
  Scenario: A manifest layout named claims keeps its own entry in the summary
    Given the manifest declares a layout named "claims" that discovers one specification
    And the glob matches ".spec/features/arithmetic/add.feature"
    When the check runs
    Then the summary lines have two "claims" counts, the manifest layout's first

  @B-026
  Scenario: A manifest layout named claims keeps its own entry in the report
    Given the manifest declares a layout named "claims" that discovers one specification
    And the glob matches ".spec/features/arithmetic/add.feature"
    When the check runs with "--json"
    Then the report's layouts have two "claims" entries, the manifest layout's first

  @B-026
  Scenario: A native Feature is reported at its record
    Given the claim file set names the record ".spec/features/arithmetic/README.md"
    And the layouts discover "src/arithmetic/.spec/README.md" with id "F1"
    When the check runs
    Then the output has a line ".spec/features/arithmetic/README.md:" naming "SPEC3002"

  @B-026
  Scenario: A native Feature with no record is reported at its first claim file
    Given the claim file set names no record
    And the glob matches ".spec/features/arithmetic/subtract.feature" and ".spec/features/arithmetic/add.feature"
    And the layouts discover "src/arithmetic/.spec/README.md" with id "F1"
    When the check runs
    Then the output has a line ".spec/features/arithmetic/add.feature:" naming "SPEC3002"

  @B-027
  Scenario: A set's identity carries an epic under the epic grammar
    Given the manifest declares "identifiers.epic"
    And the claim file set has epic "0001" and feature "F1"
    When the check runs
    Then Feature "F1" has the identity "0001" "F1"

  @B-027
  Scenario: A set without an epic under the epic grammar is an invalid manifest
    Given the manifest declares "identifiers.epic"
    And the claim file set has no epic
    When the check runs
    Then the exit code is 3
    And stderr names "claim.files"

  @B-027
  Scenario: A set with an epic under no epic grammar is an invalid manifest
    Given the manifest declares no epic grammar
    And the claim file set has epic "0001"
    When the check runs
    Then the exit code is 3
    And stderr names "claim.files"

  @B-028
  Scenario: An earlier pin is untouched
    Given the manifest is pinned to schema "0.3.1"
    And "claim" has no member beyond "tag" and "tagIgnoreCase"
    When the check runs
    Then the output equals the output of the same tree before this Feature

  @B-028
  Scenario: An earlier pin ignores claim.source
    Given the manifest is pinned to schema "0.3.1"
    And a tree the check passes without "claim.source"
    And the manifest sets "claim.source" to "gherkin"
    When the check runs
    Then the exit code is 0
    And the output equals the output without "claim.source"

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
      | one entry beside no "claim.source"           |

  @B-030
  Scenario: The same tree gives the same output
    Given the glob matches ".spec/features/arithmetic/subtract.feature" and ".spec/features/arithmetic/add.feature"
    When the check runs twice
    Then the two outputs are byte-identical
    And the claims of ".spec/features/arithmetic/add.feature" precede those of ".spec/features/arithmetic/subtract.feature"

  @B-031
  Scenario Outline: A claim source outside the two is an invalid manifest
    Given the manifest sets "claim.source" to <value>
    And the manifest has no "claim.files"
    When the check runs
    Then the exit code is 3
    And stderr names "claim.source"
    And no violation is printed

    Examples:
      | value        |
      | "yaml"       |
      | "Gherkin"    |
      | the number 1 |

  @B-032
  Scenario: A record's claim prefix does not choose the claim grammar
    Given the claim file set names the record ".spec/features/arithmetic/README.md"
    And the record's frontmatter sets "claim_prefix" to "ADD-"
    And ".spec/features/arithmetic/add.feature" has scenarios tagged "@B-001" and "@ADD-002"
    When the check runs
    Then Feature "F1" has exactly the claim "B-001"

  @B-033
  Scenario Outline: A record the check cannot read is reported
    Given the claim file set names the record "<record>"
    And <condition>
    When the check runs
    Then the output has a line "<record>: error SPEC027:"

    Examples:
      | record                              | condition                            |
      | .spec/features/arithmetic/README.md | no file is at that path              |
      | .spec/features/arithmetic/README.md | the file at that path cannot be read |
      | ../outside/README.md                | the path leads outside the root      |

  @B-034
  Scenario: An absolute record is reported at the manifest
    Given the claim file set names a record at an absolute path
    When the check runs
    Then the output has a line ".specht/manifest.json: error SPEC027:"
    And no output line holds an absolute path

  @B-035
  Scenario Outline: A record the check cannot read leaves a record-less Feature
    Given the claim file set names a record that <condition>
    And the glob matches ".spec/features/arithmetic/add.feature"
    When the check runs
    Then Feature "F1" has the lifecycle "current"
    And Feature "F1" declares no dependency edge

    Examples:
      | condition                        |
      | names no file                    |
      | names a file that cannot be read |
      | is an absolute path              |
      | leads outside the root           |

  @B-036
  Scenario: A set with no record and no claim file is no Feature
    Given the claim file set names no record
    And no file matches ".spec/features/arithmetic/*.feature" outside the exclusions
    And the layouts discover "src/arithmetic/.spec/README.md" with id "F1"
    When the check runs
    Then the summary lines count no Feature under "claims"
    And no "SPEC3002" is reported

  @B-037
  Scenario: A record's own identity does not override its set's
    Given the claim file set names the record ".spec/features/arithmetic/README.md"
    And the record's frontmatter sets "id" to "F9"
    When the check runs
    Then Feature "F1" has the identity "F1"
    And no Feature has the identity "F9"

  @B-038
  Scenario Outline: A set identity outside its grammar is an invalid manifest
    Given the manifest <grammar>
    And the claim file set has <identity>
    When the check runs
    Then the exit code is 3
    And stderr names "claim.files"
    And no violation is printed

    Examples:
      | grammar                     | identity                             |
      | declares no epic grammar    | feature "arithmetic"                 |
      | declares "identifiers.epic" | epic "0001" and feature "arithmetic" |
      | declares "identifiers.epic" | epic "1" and feature "F1"            |

  @boundary
  Scenario: A claim is never inferred from a title or a step
    Given ".spec/features/arithmetic/add.feature" has an untagged scenario titled "B-009 adds two numbers"
    When the check runs
    Then Feature "F1" has no claim "B-009"
    And the scenario is reported under "SPEC025"

  @boundary
  Scenario: A record's differing identity is not reported
    Given the claim file set names the record ".spec/features/arithmetic/README.md"
    And the record's frontmatter sets "id" to "F9"
    When the check runs
    Then no violation reports that the record's id differs from its set's feature

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
