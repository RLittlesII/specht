Feature: The rules command
  As the maintainer deciding which rules a repository is held to
  I want every rule of the pinned version listed with what it is and what it is set to
  So that I choose a level for a rule before a violation introduces it to me

  Background:
    Given a repository root holding a manifest that pins schema version 0.1.0

  @B-001
  Scenario: Every rule of the pinned version is listed once
    When the rules are listed
    Then the listing has one entry for each rule of version 0.1.0 and no other entry

  @B-002
  Scenario: A rule is listed with its family
    When the rules are listed
    Then the out-of-order-section rule is listed in the structure family

  @B-003
  Scenario: A rule is listed with its default severity
    When the rules are listed
    Then the out-of-order-section rule is listed with the default severity error

  @B-004
  Scenario Outline: A rule the manifest sets is listed at that level
    Given the manifest sets the out-of-order-section rule to <level>
    When the rules are listed
    Then the out-of-order-section rule is listed with the effective level <level>

    Examples:
      | level   |
      | error   |
      | warning |
      | off     |

  @B-005
  Scenario: A rule the manifest does not set is listed at its own severity
    Given the manifest sets no level for the out-of-order-section rule
    When the rules are listed
    Then the out-of-order-section rule is listed with the effective level error

  @B-006
  Scenario: A rule switched off is still listed
    Given the manifest sets the out-of-order-section rule to off
    When the rules are listed
    Then the listing has an entry for the out-of-order-section rule

  @B-007
  Scenario: The listing agrees with the rule's page
    When the rules are listed
    And the out-of-order-section rule is explained
    Then the family and the default severity listed for it are the ones its explanation gives

  @B-008
  Scenario: Rules are listed in order of rule id
    When the rules are listed
    Then each entry's rule id follows the one before it in ordinal order

  @B-009
  Scenario: The JSON form is one document
    When the rules are listed as JSON
    Then the standard output parses as exactly one JSON document

  @B-010
  Scenario: The JSON form carries what the text form prints
    Given the manifest sets the out-of-order-section rule to warning
    When the rules are listed as text and as JSON
    Then both forms carry the same rules, families, default severities and effective levels in the same order

  @B-011
  Scenario: Listing succeeds whatever the levels are
    Given the manifest sets the out-of-order-section rule to off
    When the rules are listed
    Then the exit code is 0

  @B-012
  Scenario: A missing root is a missing-input failure
    Given a root path that is not a directory
    When the rules are listed against that root
    Then the standard error names the root as it was given
    And the standard output is empty
    And the exit code is 2

  @B-013
  Scenario: A root without a manifest is a missing-input failure
    Given a root with no manifest
    When the rules are listed against that root
    Then the standard error names the manifest path
    And the standard output is empty
    And the exit code is 2

  @B-014
  Scenario Outline: A rejected manifest is an invalid-manifest failure
    Given a root whose manifest <is rejected>
    When the rules are listed against that root
    Then the standard error names what the check names for that manifest
    And the standard output is empty
    And the exit code is 3

    Examples:
      | is rejected                                                                  |
      | is not well-formed                                                           |
      | sets a rule the pinned version does not hold                                 |
      | pins a version the tool does not ship                                        |
      | records an upstream schema source and explicitly selects the embedded source |

  @B-015
  Scenario: Messages about the tool go to standard error
    When the rules are listed
    Then the standard output carries only the listing

  @B-016 @boundary
  Scenario: A run writes nothing
    Given a snapshot of every file under the root
    When the rules are listed
    Then no file under the root was created, modified or deleted

  @B-016 @boundary
  Scenario: Listing never changes a rule's level
    Given the manifest sets the out-of-order-section rule to warning
    When the rules are listed
    Then the manifest is byte-for-byte as it was

  @B-017
  Scenario: No derived path in the output is absolute
    Given a root with no manifest
    When the rules are listed against that root given as an absolute path
    Then every path the output names other than those typed is relative to the root and uses forward slashes

  @B-018
  Scenario: The same input gives the same output
    When the rules are listed twice
    Then both runs print the same standard output and exit with the same code

  @B-019
  Scenario: The text form is one tab-separated line per rule
    Given the manifest sets the out-of-order-section rule to warning
    When the rules are listed
    Then the out-of-order-section rule is on one line holding its rule id, family, default severity and effective level in that order
    And the default severity and the effective level are in lower case
