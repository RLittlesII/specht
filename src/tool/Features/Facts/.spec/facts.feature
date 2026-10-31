Feature: The facts command
  As an agent or a maintainer working in a repository that pins specht
  I want what the specification tree holds given as structured data
  So that I stop reading and searching Markdown to learn where a claim stands

  Background:
    Given a repository whose manifest pins schema version 0.2.0
    And two Feature specifications, each with claims, constraints, out-of-scope rows and open questions

  @B-001
  Scenario: The facts cover every specification the check discovers
    When the facts are given
    Then they hold both specifications, in the order the check discovers them

  @B-002
  Scenario: The JSON facts validate against the published schema
    When the facts are given as JSON
    Then the output is one JSON document
    And the document validates against the published facts schema

  @B-003
  Scenario: A specification is given with its path, identity and statuses
    When the facts are given as JSON
    Then each specification carries its path relative to the root, its qualified Feature id, its document maturity and its delivery status

  @B-004
  Scenario: A specification is given with its dependencies
    Given the first specification depends on the second
    When the facts are given as JSON
    Then the first specification lists the second among what it depends on
    And the second lists the first among what it blocks

  @B-005
  Scenario: A claim is given with its text, status and line
    When the facts are given as JSON
    Then each claim carries its id, its text, its status and the line it is on

  @B-006
  Scenario: A constraint is given with what it rules out
    When the facts are given as JSON
    Then each constraint carries its id, its text, what it rules out and the line it is on

  @B-007
  Scenario: An out-of-scope row is given with its reason
    When the facts are given as JSON
    Then each out-of-scope row carries its number, its item, its reason and the line it is on

  @B-008
  Scenario Outline: An open question is given with whether it is resolved
    Given an open question whose resolution reads "<resolution>"
    When the facts are given as JSON
    Then the open question carries its id, its question, what it blocks, its resolution and the line it is on
    And it is given as <state>

    Examples:
      | resolution                                   | state      |
      | Open                                         | unresolved |
      | Open. Proceeds on the default.               | unresolved |
      | Resolved 2026-10-30 by the owner: the answer | resolved   |

  @B-009
  Scenario: A lessons and deltas entry is given with its text and line
    Given the first specification records two lessons and deltas entries
    When the facts are given as JSON
    Then the first specification carries both entries, each with its text and the line it is on

  @B-010
  Scenario: A sign-off row is given with its reviewer and note
    When the facts are given as JSON
    Then each sign-off row carries its section, its status, its reviewer, its note and the line it is on

  @B-011
  Scenario: The records beside a specification are given by number and title
    Given the first specification has one decision record, one architecture decision record and one lesson beside it
    When the facts are given as JSON
    Then the first specification carries three records
    And each record carries its kind, its number, its title and its path relative to the root

  @B-012
  Scenario: A withdrawn claim is still given
    Given a claim of the first specification is withdrawn
    When the facts are given as JSON
    Then that claim is given with the withdrawn status

  @B-013
  Scenario: The facts succeed on a tree the check fails
    Given the first specification has two claims sharing one id
    When the facts are given
    Then the command succeeds
    And both claims are given

  @B-014 @boundary
  Scenario: The facts carry no verdict
    Given the first specification has a violation the check reports
    When the facts are given as JSON
    Then the document carries no violation, no count and no verdict

  @B-015
  Scenario: A missing section leaves only its own facts out
    Given the first specification has no out-of-scope section
    When the facts are given as JSON
    Then the first specification carries no out-of-scope rows
    And it still carries its claims, constraints and open questions

  @B-016
  Scenario: A section the manifest renames is read under its new name
    Given the manifest names the constraints section "4. Limits"
    And the first specification's constraints sit under "4. Limits"
    When the facts are given as JSON
    Then the first specification's constraints are given

  @B-017
  Scenario: The facts have one shape under every pin
    Given the same specifications under a manifest pinning schema version 0.1.0
    When the facts are given as JSON under each pin
    Then both documents carry the same members

  @B-018
  Scenario: A path selects one specification
    When the facts are given for the first specification's path
    Then they hold the first specification only

  @B-019
  Scenario: A directory selects every specification under it
    Given both specifications sit under one directory
    When the facts are given for that directory
    Then they hold both specifications

  @B-020
  Scenario: A path that selects nothing fails
    When the facts are given for a path that holds no specification
    Then the command fails with the not-found exit code
    And nothing is written to the standard output

  @B-021
  Scenario Outline: The facts fail on a broken root as the check does
    Given <condition>
    When the facts are given
    Then the command exits with code <code>

    Examples:
      | condition                                          | code |
      | the root does not exist                            | 2    |
      | the root holds no manifest                         | 2    |
      | the manifest is invalid                            | 3    |
      | the manifest pins a version the tool does not ship | 3    |

  @B-022
  Scenario: The same tree gives the same facts
    When the facts are given twice
    Then both outputs are byte-identical

  @B-023
  Scenario: The facts write nothing
    When the facts are given
    Then no file under the root or elsewhere is created, modified or deleted

  @B-024
  Scenario: The text form gives one line per fact
    When the facts are given as text
    Then each claim, constraint, out-of-scope row and open question is one line
    And each line begins with its specification's path relative to the root

  @B-025 @boundary
  Scenario: The facts carry no claim coverage
    Given a claim with a traceability row and a scenario carrying its tag
    When the facts are given as JSON
    Then the claim carries no traceability row, no scenario and no test

  @B-026 @boundary
  Scenario: An epic file is not given
    Given the repository holds an epic file the check discovers
    When the facts are given
    Then they hold the two Feature specifications and not the epic
