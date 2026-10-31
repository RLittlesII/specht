Feature: The manifest command
  As the maintainer configuring a repository's specification check
  I want to see the configuration the check reads, and the defaults the tool ships
  So that a value my manifest leaves out is as visible as one it declares

  Background:
    Given a repository root holding a manifest that pins schema version 0.1.0

  @B-001
  Scenario: The effective manifest is printed
    Given the root holds specifications with violations
    When the effective manifest is printed
    Then the standard output carries the effective manifest of that root

  @B-002
  Scenario: Printing the effective manifest succeeds
    When the effective manifest is printed
    Then the exit code is 0

  @B-003
  Scenario: A declared value is printed as declared
    Given the manifest renames the claims section
    When the effective manifest is printed
    Then the claims section carries the name the manifest gives it

  @B-004
  Scenario: A value left out is printed at its default
    Given the manifest leaves out the name of the claims section
    When the effective manifest is printed
    Then the claims section carries the name the default manifest of version 0.1.0 gives it

  @B-005
  Scenario: A rule with no level is printed at its own severity
    Given the manifest sets no level for the out-of-order-section rule
    When the effective manifest is printed
    Then the out-of-order-section rule carries the level of its own severity

  @B-006
  Scenario: A manifest without a version is printed as version 0.1.0
    Given a repository root holding a manifest that pins no schema version
    When the effective manifest is printed
    Then the schema version printed is 0.1.0

  @B-007
  Scenario: The effective manifest gives the check's verdict
    Given the root holds specifications with violations
    And the manifest leaves out the name of the claims section
    When the effective manifest is printed as JSON and written as the manifest of a copy of the root
    Then the check reports for the copy what it reports for the root

  @B-008
  Scenario: The default manifest is printed
    When the default manifest is printed
    Then the standard output carries the default manifest of the newest version the tool ships

  @B-009
  Scenario Outline: The default manifest does not depend on the root
    Given <a root>
    When the default manifest is printed against that root
    Then the standard output is what it is against a root with a valid manifest

    Examples:
      | a root                                   |
      | a root with no manifest                  |
      | a root whose manifest is not well-formed |
      | a root path that is not a directory      |

  @B-010
  Scenario: Printing the default manifest succeeds without a root
    Given a root path that is not a directory
    When the default manifest is printed against that root
    Then the exit code is 0

  @B-011
  Scenario Outline: The JSON form is one document
    When <which> manifest is printed as JSON
    Then the standard output parses as exactly one JSON document

    Examples:
      | which         |
      | the effective |
      | the default   |

  @B-012
  Scenario: The default manifest is the embedded one
    Given the manifest the tool embeds for the newest version it ships
    When the default manifest is printed as JSON
    Then every key and value printed equals that embedded manifest

  @B-013
  Scenario: A missing root is a missing-input failure
    Given a root path that is not a directory
    When the effective manifest is printed against that root
    Then the standard error names the root as it was given
    And the standard output is empty
    And the exit code is 2

  @B-014
  Scenario: A root without a manifest is a missing-input failure
    Given a root with no manifest
    When the effective manifest is printed against that root
    Then the standard error names the manifest path
    And the standard output is empty
    And the exit code is 2

  @B-015
  Scenario Outline: A rejected manifest is an invalid-manifest failure
    Given a root whose manifest <is rejected>
    When the effective manifest is printed against that root
    Then the standard error names what the check names for that manifest
    And the standard output is empty
    And the exit code is 3

    Examples:
      | is rejected                                                                  |
      | is not well-formed                                                           |
      | carries a key the tool does not know                                         |
      | pins a version the tool does not ship                                        |
      | records an upstream schema source and explicitly selects the embedded source |

  @B-016
  Scenario: Messages about the tool go to standard error
    When the effective manifest is printed
    Then the standard output carries only the manifest

  @B-017 @boundary
  Scenario: A run writes nothing
    Given the manifest leaves out the name of the claims section
    And a snapshot of every file under the root
    When the effective manifest is printed
    Then no file under the root was created, modified or deleted

  @B-017 @boundary
  Scenario: Printing the default manifest never creates a manifest
    Given a root with no manifest
    And a snapshot of every file under the root
    When the default manifest is printed against that root
    Then no file under the root was created, modified or deleted

  @B-018
  Scenario: No derived path in the output is absolute
    Given a root with no manifest
    When the effective manifest is printed against that root given as an absolute path
    Then every path the output names other than those typed is relative to the root and uses forward slashes

  @B-019
  Scenario: The same input gives the same output
    When the effective manifest is printed twice
    Then both runs print the same standard output and exit with the same code

  @B-020
  Scenario: The text form is the JSON document, indented, in order
    When the effective manifest is printed as text and as JSON
    Then the text is the document the JSON form prints, indented
    And its keys are in the order the default manifest holds them
    And its rule settings are in ascending order of rule id

  @B-021
  Scenario Outline: The JSON form validates against its published schema
    When <which> manifest is printed as JSON
    Then the document validates against the published schema for the manifest document

    Examples:
      | which         |
      | the effective |
      | the default   |
