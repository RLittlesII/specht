Feature: Reading through MCP
  As a coding agent in a repository that pins specht
  I want the check's verdict, the rules, what the pinned schema version demands and what the tree holds through MCP
  So that I stop following copied skills that drift and scraping the command's output

  Background:
    Given a repository whose manifest pins a shipped schema version
    And two Feature specifications, one of them with a violation the check reports
    And a session with the tool's MCP server on that repository

  @B-001
  Scenario: The verdict is the check's JSON report
    When the verdict is asked for
    Then the answer is exactly what the check writes as its JSON report for the same repository

  @B-001 @boundary
  Scenario: The verdict is unchanged when the answer is reported as unpinned
    Given the repository's local tool manifest no longer lists the tool
    And a global tool is installed
    When the verdict is asked for
    Then the part of the result holding the verdict is exactly what the check writes as its JSON report for the same repository

  @B-002 @boundary
  Scenario: The verdict is for the whole tree only
    When the server lists what the verdict can be asked for with
    Then nothing it lists names a specification or a path

  @B-003
  Scenario: A rule's text is what explaining it prints
    When the text of a rule the pinned version has is asked for
    Then the answer is exactly what explaining that rule prints

  @B-004
  Scenario: The rule list is what listing the rules prints
    When the rule list is asked for
    Then the answer is exactly what listing the rules as JSON prints for the same repository

  @B-005
  Scenario: The manifest is what printing the manifest prints
    When the manifest is asked for
    Then the answer is exactly what printing the manifest as JSON prints for the same repository

  @B-006
  Scenario: The tree's facts are what the facts command prints
    When the tree's facts are asked for
    Then the answer is exactly what the facts command prints as JSON for the same repository

  @B-007
  Scenario: Claim coverage is what the trace prints
    When claim coverage is asked for
    Then the answer is exactly what the trace prints as JSON for the same repository

  @B-008
  Scenario: The shipped schema versions are what listing them prints
    When the shipped schema versions are asked for
    Then the answer is exactly what listing the shipped schema versions as JSON prints for the same repository

  @B-009
  Scenario: The frontmatter schemas and templates are what printing them prints
    When the frontmatter schemas and the templates are asked for
    Then the answer is exactly what printing them prints for the pinned version

  @B-010
  Scenario Outline: An argument the command takes is passed to it
    When <answer> is asked for with <argument>
    Then the answer is exactly what the command prints given that argument

    Examples:
      | answer       | argument                                              |
      | the manifest | the request for the tool's shipped defaults           |
      | the verdict  | the request to read the frontmatter schemas from disk |

  @B-011
  Scenario: A tree with violations is an answer, not an error
    When the verdict is asked for
    Then the answer carries the violation
    And the answer is not marked as an error

  @B-012
  Scenario Outline: A failure the command reports is an error answer
    Given <situation>
    When <answer> is asked for
    Then the answer is marked as an error
    And its text is the message the command writes on standard error

    Examples:
      | situation                                    | answer                |
      | the repository's manifest is removed         | the verdict           |
      | the manifest is replaced by one the check rejects | the rule list    |
      | a rule id the pinned version does not have   | that rule's text      |

  @B-013
  Scenario Outline: Every answer is for the version the repository pins
    Given the repository pins schema version <version>
    When each answer is asked for
    Then each is exactly what its command prints under that pin

    Examples:
      | version |
      | 0.1.0   |
      | 0.2.1   |

  @B-014
  Scenario: Moving the pin between two calls changes the second answer
    Given the rule list has been asked for once
    When the pin is moved to another shipped version
    And the rule list is asked for again
    Then the second answer is exactly what listing the rules prints under the new pin

  @B-015
  Scenario: An edit between two calls is in the second answer
    Given the verdict has been asked for once
    When the violation the check reports is fixed in its specification
    And the verdict is asked for again
    Then the second answer is exactly what the check writes as its JSON report for the edited repository

  @B-016
  Scenario: Every answer writes nothing
    When each answer is asked for
    Then no file under the root or elsewhere has been created, modified or deleted

  @B-017 @boundary
  Scenario: The server offers only answers the command line gives
    When the answers the server offers are listed
    Then each is one of the verdict, a rule's text, the rule list, the manifest, the tree's facts, claim coverage, the shipped schema versions, and the frontmatter schemas and templates

  @B-017 @boundary
  Scenario: The server offers no write and no work queue
    When the answers the server offers are listed
    Then none initialises, upgrades, pins, formats, sets a rule's level or writes a report
    And none answers for the work queue

  @B-018
  Scenario: A Feature specification built from the answers alone passes
    Given another repository holding only the manifest that initialising writes
    When a Feature specification and its scenarios are built from the server's answers alone
    Then the verdict on that repository reports no violation
