Feature: Id generation
  As an author on a branch cut in parallel with others
  I want the pinned specht to mint a new id for a record type my repository opts in
  So that no other branch takes the same id, and the types I did not opt in keep their own numbering

  Background:
    Given a repository root with a valid Specht manifest

  @B-001 @boundary
  Scenario Outline: A type not opted in gets no id
    Given a manifest that <opts in>
    When a claim id is requested
    Then nothing is written on standard output

    Examples:
      | opts in                     |
      | opts no type in             |
      | opts only lesson numbers in |

  @B-002 @boundary
  Scenario: A type not opted in is refused
    Given a manifest that opts only lesson numbers in
    When a claim id is requested
    Then the request ends with a failure code the check already gives

  @B-003
  Scenario: A minted id is the whole output
    Given a manifest that opts claim ids in to Nano ID
    When a claim id is requested
    Then standard output is one line holding only the minted id

  @B-004
  Scenario: A printed id ends in success
    Given a manifest that opts claim ids in to Nano ID
    When a claim id is requested
    Then the request ends in success

  @B-005
  Scenario: A Nano ID has the configured size
    Given a manifest that opts claim ids in to Nano ID with a size of 10
    When a claim id is requested
    Then the minted id is 10 characters long after its prefix

  @B-006
  Scenario: A Nano ID draws only from the configured alphabet
    Given a manifest that opts claim ids in to Nano ID with an alphabet of lowercase letters and digits
    When a claim id is requested
    Then every character of the minted id after its prefix is a lowercase letter or a digit

  @B-007
  Scenario: A Snowflake id carries the time since its epoch
    Given a manifest that opts ADR numbers in to Snowflake with an epoch of 2026-01-01
    And the time is noon on 2026-10-23, universal time
    When an ADR number is requested
    Then the minted number, decoded, holds the milliseconds from 2026-01-01 to noon on 2026-10-23

  @B-008
  Scenario: Each type follows its own settings
    Given a manifest that opts claim ids in to Nano ID with a size of 8
    And it opts lesson numbers in to Nano ID with a size of 12
    When a claim id and a lesson number are requested
    Then the claim id is 8 characters long after its prefix
    And the lesson number is 12 characters long after its prefix

  @B-009
  Scenario: A minted id starts with its type's prefix
    Given a manifest that opts ADR numbers in with the prefix "ADR-"
    When an ADR number is requested
    Then the minted number starts with "ADR-"

  @B-010
  Scenario: An id the tree already holds is never handed out
    Given a manifest that opts claim ids in to Nano ID
    And a specification already holding the claim id that would be minted first
    When a claim id is requested
    Then the minted id is one no specification in the tree holds

  @B-011 @boundary
  Scenario: An id its type's grammar rejects is not printed
    Given a manifest that opts claim ids in to Snowflake
    And its claim grammar admits only three-digit claim numbers
    When a claim id is requested
    Then nothing is written on standard output

  @B-012 @boundary
  Scenario: An id its type's grammar rejects is refused
    Given a manifest that opts claim ids in to Snowflake
    And its claim grammar admits only three-digit claim numbers
    When a claim id is requested
    Then the request ends with a failure code the check already gives

  @B-013 @boundary
  Scenario Outline: A request writes nothing
    Given a manifest that <opts in>
    And specifications that already hold claim ids
    When a claim id is requested
    Then no file under the root or elsewhere has been created, modified or deleted

    Examples:
      | opts in                        |
      | opts claim ids in to Nano ID   |
      | opts claim ids in to Snowflake |
      | opts no type in                |

  @B-014 @boundary
  Scenario: Opting types in changes no verdict
    Given a tree whose check reports a violation
    And a manifest that opts claim ids in to Nano ID
    When the tree is checked
    Then the violations reported are the ones the same tree gets with the id settings removed

  @B-015
  Scenario: Id settings the pinned version rejects are invalid
    Given a manifest pinned to a schema version that has no id settings
    And the manifest sets them
    When a claim id is requested
    Then the request ends as it does for an invalid manifest
