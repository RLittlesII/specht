Feature: Setting a rule's level
  As the maintainer bringing an old specification tree under the check
  I want one command that sets one rule to error, warning or off
  So that the manifest changes in exactly the place I meant and stays one the check accepts

  Background:
    Given a repository root holding a manifest that pins schema version 0.1.0

  @B-001
  Scenario Outline: Setting a level writes it into the manifest
    Given the manifest sets the out-of-order-section rule to error
    When the out-of-order-section rule is set to <level>
    Then the manifest sets the out-of-order-section rule to <level>

    Examples:
      | level   |
      | warning |
      | off     |

  @B-002
  Scenario: Setting a level succeeds
    When the out-of-order-section rule is set to warning
    Then the exit code is 0

  @B-003
  Scenario: Setting a level changes only that rule's entry
    Given the manifest sets the out-of-order-section rule to error
    When the out-of-order-section rule is set to warning
    Then the manifest differs from before only in that rule's level

  @B-003 @boundary
  Scenario: Setting the level a rule already has changes nothing
    Given the manifest sets the out-of-order-section rule to warning
    When the out-of-order-section rule is set to warning
    Then the manifest is byte-for-byte as it was

  @B-004
  Scenario: Setting a rule the manifest does not name adds its entry
    Given the manifest sets a level for the missing-frontmatter rule and none for the out-of-order-section rule
    When the out-of-order-section rule is set to warning
    Then the manifest sets the out-of-order-section rule to warning
    And every other setting in the manifest keeps its value

  @B-005
  Scenario: Setting a rule in a manifest with no rule settings adds them
    Given the manifest carries no rule settings
    When the out-of-order-section rule is set to warning
    Then the manifest's rule settings hold the out-of-order-section rule at warning and nothing else
    And every other setting in the manifest keeps its value

  @B-006 @boundary
  Scenario: Setting a level touches no other file
    Given a snapshot of every file under the root
    When the out-of-order-section rule is set to warning
    Then no file under the root other than the manifest was created, modified or deleted

  @B-006 @boundary
  Scenario: Setting a level never moves the pinned version
    When the out-of-order-section rule is set to warning
    Then the manifest still pins schema version 0.1.0

  @B-007
  Scenario: A rule the pinned version does not hold is a not-found failure
    Given a snapshot of every file under the root
    When a rule that version 0.1.0 does not hold is set to warning
    Then the standard error names that rule
    And no file under the root has changed
    And the exit code is 4

  @B-008
  Scenario: A level that is not one of the three is a not-found failure
    Given a snapshot of every file under the root
    When the out-of-order-section rule is set to a level named loud
    Then the standard error names loud and the three levels
    And no file under the root has changed
    And the exit code is 4

  @B-009
  Scenario: Setting a level under a missing root is a missing-input failure
    Given a root path that is not a directory
    When the out-of-order-section rule is set to warning against that root
    Then nothing is written
    And the exit code is 2

  @B-010 @boundary
  Scenario: Setting a level without a manifest is a missing-input failure
    Given a root with no manifest
    When the out-of-order-section rule is set to warning against that root
    Then no manifest is written
    And the exit code is 2

  @B-011
  Scenario Outline: Setting a level in an invalid manifest is invalid configuration
    Given a root whose manifest <is rejected>
    And a snapshot of every file under the root
    When the out-of-order-section rule is set to warning against that root
    Then no file under the root has changed
    And the exit code is 3

    Examples:
      | is rejected                           |
      | is not well-formed                    |
      | carries a key the tool does not know  |
      | pins a version the tool does not ship |

  @B-012
  Scenario: The manifest left behind is one the check accepts
    Given the manifest carries no rule settings
    When the out-of-order-section rule is set to off
    Then the check accepts the manifest

  @B-013
  Scenario: No derived path in the output is absolute
    Given a root with no manifest
    When the out-of-order-section rule is set to warning against that root given as an absolute path
    Then every path the output names other than those typed is relative to the root and uses forward slashes
