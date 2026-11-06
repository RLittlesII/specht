Feature: Comment size
  As the reader of a specification
  I want a bulky comment reported when my repository asks for it
  So that I read the current content without an amendment narrative beside it

  Background:
    Given a repository whose manifest pins a schema version that carries the comment size rule

  @B-001
  Scenario Outline: A comment over the line limit is reported
    Given comment size checking is on with the default limits
    And a specification holding <comment>
    When the check runs
    Then one comment size finding is reported, on the line the comment opens on

    Examples:
      | comment                                                                                          |
      | a four-line comment that announces an amendment, lists two changed rows and names their benefits |
      | a two-line comment of 40 characters                                                              |
      | a three-line banner of repeated equals signs around the word "Amended"                           |

  @B-002
  Scenario: A one-line comment over the character limit is reported
    Given comment size checking is on with the default limits
    And a specification holding a one-line comment of 201 characters
    When the check runs
    Then one comment size finding is reported, on the line the comment opens on

  @B-003
  Scenario: A comment over both limits gets one finding
    Given comment size checking is on with the default limits
    And a specification holding a six-line comment of 600 characters
    When the check runs
    Then exactly one comment size finding is reported for that comment

  @B-004
  Scenario Outline: A short one-line comment is not reported
    Given comment size checking is on with the default limits
    And a specification holding the one-line comment "<comment>"
    When the check runs
    Then no comment size finding is reported

    Examples:
      | comment                    |
      | <!-- prettier-ignore -->   |
      | <!-- specht-item: 0045 --> |
      | <!-- specht-milestone -->  |

  @B-004 @boundary
  Scenario: A one-line amendment comment within the limits is not reported here
    Given comment size checking is on with the default limits
    And a specification holding a one-line comment of 119 characters that opens with "Amended" right after a section stamp
    When the check runs
    Then no comment size finding is reported

  @B-005
  Scenario Outline: A stamp is never a comment size finding
    Given comment size checking is on with the default limits
    And a specification with a section stamp <stamp>
    When the check runs
    Then no comment size finding is reported

    Examples:
      | stamp                                              |
      | of one role and one date                           |
      | of one role, one date and a note of 900 characters |
      | that lists two dates                               |

  @B-006
  Scenario Outline: A comment quoted as code is not read
    Given comment size checking is on with the default limits
    And a specification whose prose quotes, as <code>, a comment over both limits
    When the check runs
    Then no comment size finding is reported

    Examples:
      | code                   |
      | a fenced code block    |
      | an indented code block |
      | inline code            |

  @B-007
  Scenario: Two comments of one size get one verdict whatever they say
    Given comment size checking is on with the default limits
    And a specification holding a three-line comment of 150 characters that opens with "Amended"
    And another three-line comment of 150 characters that opens with "Context"
    When the check runs
    Then one comment size finding is reported for each of the two comments

  @B-007 @boundary
  Scenario: A comment is not asked who, when or why
    Given comment size checking is on with the default limits
    And a specification holding a one-line comment that names no date, no author, no authorization and no affected key
    When the check runs
    Then no comment size finding is reported

  @B-008
  Scenario: A comment between generated markers is judged like any other
    Given comment size checking is on with the default limits
    And a specification holding a one-line start marker, then a three-line comment, then a one-line end marker
    When the check runs
    Then one comment size finding is reported, on the line the three-line comment opens on
    And no comment size finding is reported for either marker

  @B-009
  Scenario: A template guidance comment kept in a copy is judged by its size
    Given comment size checking is on with the default limits
    And a specification copied from the shipped Feature template that keeps only its eight-line opening guidance comment
    When the check runs
    Then one comment size finding is reported, on the line that guidance comment opens on

  @B-010
  Scenario Outline: A comment inside a paragraph or a table cell is judged like any other
    Given comment size checking is on with the default limits
    And a specification holding, inside <place>, a comment of 250 characters
    When the check runs
    Then one comment size finding is reported, on the line that comment opens on

    Examples:
      | place                        |
      | a sentence of a paragraph    |
      | one cell of the claims table |

  @B-011
  Scenario: Two comments on one line are judged one by one
    Given comment size checking is on with the default limits
    And a specification holding one line with two comments of 150 characters each
    When the check runs
    Then no comment size finding is reported

  @B-012
  Scenario Outline: Without the key nothing is reported
    Given the manifest does not set the comment size key
    And the manifest <setting>
    And a specification holding a ten-line comment
    When the check runs
    Then no comment size finding is reported

    Examples:
      | setting                                 |
      | sets no level for the comment size rule |
      | sets the comment size rule to error     |
      | sets the comment size rule to warning   |

  @B-012 @boundary
  Scenario Outline: Neither init nor upgrade turns the check on
    Given <start>
    When <command>
    Then the manifest does not set the comment size key

    Examples:
      | start                                             | command                                         |
      | a repository with no manifest                     | the manifest is written by init                 |
      | a manifest that does not set the comment size key | the repository is brought up to date by upgrade |

  @B-013
  Scenario Outline: The default limits are one line and 200 characters
    Given the manifest sets the comment size key with no limit
    And a specification holding <comment>
    When the check runs
    Then <outcome>

    Examples:
      | comment                              | outcome                              |
      | a one-line comment of 200 characters | no comment size finding is reported  |
      | a one-line comment of 201 characters | one comment size finding is reported |
      | a two-line comment of 40 characters  | one comment size finding is reported |

  @B-014
  Scenario Outline: The manifest's limits are the ones applied
    Given the manifest sets the comment size line limit to 3 and the character limit to 400
    And a specification holding <comment>
    When the check runs
    Then <outcome>

    Examples:
      | comment                                | outcome                              |
      | a three-line comment of 300 characters | no comment size finding is reported  |
      | a four-line comment of 300 characters  | one comment size finding is reported |
      | a one-line comment of 401 characters   | one comment size finding is reported |

  @B-015
  Scenario Outline: A limit that is not a positive whole number is rejected
    Given the manifest sets the comment size <limit> to <value>
    When the check runs
    Then the manifest is reported invalid, naming the comment size key
    And the run exits with the invalid-manifest code

    Examples:
      | limit           | value |
      | line limit      | 0     |
      | line limit      | -1    |
      | character limit | 2.5   |
      | character limit | "ten" |

  @B-016
  Scenario: Comment size findings are warnings by default
    Given comment size checking is on with the default limits
    And the manifest sets no level for the comment size rule
    And a specification holding a four-line comment
    When the check runs
    Then the comment size finding is reported as a warning

  @B-016
  Scenario Outline: A comment size warning fails the run only when warnings are made to fail
    Given comment size checking is on with the default limits
    And the manifest sets no level for the comment size rule
    And a specification whose only violation is a four-line comment
    When the check runs <mode>
    Then the run <outcome>

    Examples:
      | mode                     | outcome  |
      | with no further option   | succeeds |
      | with warnings made fatal | fails    |

  @B-017
  Scenario: A disabled comment size rule reports nothing
    Given comment size checking is on with the default limits
    And the manifest disables the comment size rule
    And a specification holding a four-line comment
    When the check runs
    Then no comment size finding is reported

  @B-018
  Scenario: A comment size rule raised to an error reports errors
    Given comment size checking is on with the default limits
    And the manifest sets the comment size rule to error
    And a specification holding a four-line comment
    When the check runs
    Then the comment size finding is reported as an error

  @B-019
  Scenario Outline: The comment size key is rejected under a pin of 0.3.0 or earlier
    Given the manifest pins schema version <version> instead
    And the manifest sets the comment size key with no limit
    And a specification holding a ten-line comment
    When the check runs
    Then the manifest is reported invalid, naming the comment size key
    And no comment size finding is reported

    Examples:
      | version |
      | 0.3.0   |
      | 0.1.0   |

  @B-020
  Scenario Outline: Comments are judged in either specification layout
    Given comment size checking is on with the default limits
    And a specification in the <layout> layout holding a four-line comment
    When the check runs
    Then one comment size finding is reported for that specification

    Examples:
      | layout     |
      | co-located |
      | legacy     |

  @B-021
  Scenario Outline: Markdown that is not a Feature specification is not judged
    Given comment size checking is on with the default limits
    And <file> holding a ten-line comment
    When the check runs
    Then no comment size finding is reported

    Examples:
      | file                   |
      | an epic file           |
      | a decision record      |
      | an architecture record |
      | a lesson               |
      | a template             |
      | a documentation page   |

  @B-022
  Scenario: The finding's path is relative to the repository
    Given comment size checking is on with the default limits
    And a specification holding a four-line comment
    When the check runs
    Then the comment size finding names the specification by a path that starts at the repository root and is written with forward slashes

  @B-023
  Scenario Outline: The finding says how large the comment is and which limit it exceeds
    Given comment size checking is on with the default limits
    And a specification holding <comment>
    When the check runs
    Then the comment size finding names <lines> lines and <characters> characters, and the <limits> it exceeds

    Examples:
      | comment                               | lines | characters | limits                         |
      | a four-line comment of 150 characters | 4     | 150        | line limit                     |
      | a one-line comment of 240 characters  | 1     | 240        | character limit                |
      | a six-line comment of 600 characters  | 6     | 600        | line limit and character limit |

  @B-024
  Scenario: The finding proposes no replacement text
    Given comment size checking is on with the default limits
    And a specification holding a four-line comment
    When the check runs
    Then the comment size finding offers no text to put in the comment's place

  @B-025
  Scenario: The check leaves the specification as it was
    Given comment size checking is on with the default limits
    And a specification holding a four-line comment
    When the check runs
    Then every file of the repository is as it was before the check ran
