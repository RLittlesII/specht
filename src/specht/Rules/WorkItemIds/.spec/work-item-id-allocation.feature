Feature: Work-item id allocation
  As whoever follows a work-item id to the item it names
  I want every work-item id claimed by one file and the sequence file never behind the ids present
  So that a bare id names one item and a number taken twice is caught by the check

  Background:
    Given a repository whose manifest pins a schema version that carries the work-item id rules
    And the manifest declares the repository's work-item files and names its sequence file

  @B-001
  Scenario Outline: A work-item id claimed twice is reported
    Given two work items numbered 0118 that sit in <placement>
    When the check runs
    Then a claimed-twice finding names 0118

    Examples:
      | placement         |
      | one folder        |
      | different folders |

  @B-001
  Scenario: An id a branch and its base both took is reported in the merged tree
    Given a branch and its base each added a different work item numbered 0134
    And the tree being checked is the merge of the two
    When the check runs
    Then a claimed-twice finding names 0134

  @B-002
  Scenario Outline: Every claimant carries a finding
    Given <count> work items numbered 0118, each in its own folder
    When the check runs
    Then <count> claimed-twice findings are reported, one on each of those work items

    Examples:
      | count |
      | two   |
      | three |

  @B-003
  Scenario: The finding names the id and every claimant
    Given three work items numbered 0118, each in its own folder
    When the check runs
    Then each claimed-twice finding names 0118 and the paths of all three work items

  @B-004
  Scenario: Claimants are named in one order everywhere
    Given three work items numbered 0118, each in its own folder
    When the check runs
    Then each claimed-twice finding names the three paths in the same order, sorted by path

  @B-005
  Scenario: Every path in a finding is relative to the repository
    Given two work items numbered 0118, each in its own folder
    When the check runs
    Then every path the claimed-twice findings name starts at the repository root and is written with forward slashes

  @B-006
  Scenario: Ids each claimed once report nothing
    Given work items numbered 0117, 0118 and 0119
    When the check runs
    Then no claimed-twice finding is reported

  @B-006 @boundary
  Scenario: An id the base took is not seen in the branch's own tree
    Given a branch and its base each added a different work item numbered 0134
    And the tree being checked is the branch's own, which holds one of them
    When the check runs
    Then no claimed-twice finding is reported

  @B-007 @boundary
  Scenario: A copy of the tree in an excluded folder adds no claimant
    Given a work item numbered 0118
    And a copy of that work item under a folder the manifest excludes
    When the check runs
    Then no claimed-twice finding is reported

  @B-007 @boundary
  Scenario: A work item in an excluded folder does not raise the highest id
    Given work items numbered 0117 and 0118
    And the sequence file holds 0118
    And a work item numbered 0140 under a folder the manifest excludes
    When the check runs
    Then no sequence finding is reported

  @B-008
  Scenario: A work item that takes an epic's id is reported
    Given an epic numbered 0101
    And a work item numbered 0101
    When the check runs
    Then a claimed-twice finding names 0101, the work item and the epic

  @B-009 @boundary
  Scenario: Ids that differ only by leading zeros are two ids
    Given the manifest's work-item ids may be written with or without leading zeros
    And work items numbered 118 and 0118
    When the check runs
    Then no claimed-twice finding is reported

  @B-010 @boundary
  Scenario: A task id used twice is left to the child-item rule
    Given two child items beside a specification that carry the task id 0007-02
    When the check runs
    Then no claimed-twice finding is reported
    And SPEC044 reports 0007-02 once

  @B-011
  Scenario: A recorded pair reports nothing
    Given two work items numbered 0118, each in its own folder
    And the manifest records that exactly those two files share 0118
    When the check runs
    Then no claimed-twice finding is reported

  @B-011
  Scenario: The recorded files carry no finding when a third file takes the id
    Given two work items numbered 0118, each in its own folder
    And the manifest records that exactly those two files share 0118
    And a third work item numbered 0118 in another folder
    When the check runs
    Then no claimed-twice finding is reported on either recorded work item

  @B-012
  Scenario: A further claimant of a recorded id is reported
    Given two work items numbered 0118, each in its own folder
    And the manifest records that exactly those two files share 0118
    And a third work item numbered 0118 in another folder
    When the check runs
    Then one claimed-twice finding is reported, on the third work item

  @B-013
  Scenario Outline: A recorded path that claims nothing makes the record stale
    Given the manifest records that two files share 0118
    And <state of one recorded path>
    When the check runs
    Then a stale-record finding is reported on the manifest

    Examples:
      | state of one recorded path                             |
      | no file is at one of the recorded paths                |
      | the file at one of the recorded paths is numbered 0119 |

  @B-014
  Scenario: The stale finding names the id and the path
    Given the manifest records that two files share 0118
    And no file is at one of the recorded paths
    When the check runs
    Then the stale-record finding names 0118 and the recorded path where no file is

  @B-015
  Scenario Outline: A malformed shared-id record is invalid configuration
    Given the manifest records a shared id as <entry>
    When the check runs
    Then the manifest is rejected as invalid, naming that entry

    Examples:
      | entry                                        |
      | an id that is not a work-item id             |
      | an id with one path                          |
      | an id with the same path twice               |
      | an id with an absolute path                  |
      | an id with a path that leaves the repository |

  @B-016
  Scenario: A rejected shared-id record exits with the invalid-manifest code
    Given the manifest records a shared id with one path
    When the check runs
    Then the run exits with code 3

  @B-017
  Scenario: A missing sequence file is reported
    Given work items numbered 0117 and 0118
    And no file is where the manifest says the sequence file is
    When the check runs
    Then a sequence finding on the manifest says the sequence file is missing

  @B-018
  Scenario Outline: A sequence file that is not one id is reported
    Given work items numbered 0117 and 0118
    And the sequence file holds <content>
    When the check runs
    Then a sequence finding on the sequence file says it does not hold one work-item id

    Examples:
      | content                         |
      | nothing                         |
      | two ids                         |
      | text that is not a work-item id |

  @B-019
  Scenario: A sequence file behind the highest work item is reported
    Given work items numbered 0117 and 0118
    And the sequence file holds 0117
    When the check runs
    Then a sequence finding on the sequence file says it is behind

  @B-020
  Scenario: The finding names what the sequence file holds and what is ahead of it
    Given work items numbered 0117 and 0118
    And the sequence file holds 0117
    When the check runs
    Then the sequence finding names 0117, 0118 and the work item numbered 0118

  @B-021
  Scenario: A sequence file behind the highest epic is reported
    Given work items numbered 0117 and 0118
    And an epic numbered 0132
    And the sequence file holds 0118
    When the check runs
    Then a sequence finding on the sequence file says it is behind

  @B-022
  Scenario: A sequence file at the highest id reports nothing
    Given work items numbered 0117 and 0118
    And the sequence file holds 0118
    When the check runs
    Then no sequence finding is reported

  @B-023 @boundary
  Scenario: A sequence file ahead of every id reports nothing
    Given work items numbered 0117 and 0118
    And the sequence file holds 0126, reserving the numbers between
    When the check runs
    Then no sequence finding is reported

  @B-024 @boundary
  Scenario: A skipped number is not a finding
    Given work items numbered 0064 and 0066, and none numbered 0065
    And the sequence file holds 0066
    When the check runs
    Then no finding is reported about 0065

  @B-025
  Scenario: A sequence file that is not one id gets one finding
    Given work items numbered 0117 and 0118
    And the sequence file holds two ids, both lower than 0118
    When the check runs
    Then one sequence finding is reported

  @B-026
  Scenario: A sequence file with no ids present reports nothing
    Given no work item and no epic
    And the sequence file holds 0002
    When the check runs
    Then no sequence finding is reported

  @B-027 @boundary
  Scenario: A repository that declares no work items is not checked
    Given the manifest declares no work-item files instead
    And two tracker files numbered 0118 that the manifest declares nothing about
    When the check runs
    Then no work-item id finding is reported

  @B-028 @boundary
  Scenario: A repository that names no sequence file gets no sequence finding
    Given the manifest names no sequence file instead
    And work items numbered 0117 and 0118
    When the check runs
    Then no sequence finding is reported

  @B-029 @boundary
  Scenario: The shipped manifest declares no work items
    When the manifest the tool ships for that schema version is read
    Then it declares no work-item files, names no sequence file and records no shared id

  @B-030
  Scenario: Work-item id findings are errors by default
    Given the manifest sets no severity for the work-item id rules
    And two work items numbered 0118, each in its own folder
    And the sequence file holds 0117
    When the check runs
    Then the claimed-twice findings and the sequence finding are reported as errors

  @B-031
  Scenario: Disabled work-item id rules report nothing
    Given the manifest disables the work-item id rules
    And two work items numbered 0118, each in its own folder
    And the sequence file holds 0117
    When the check runs
    Then no work-item id finding is reported

  @B-032 @boundary
  Scenario: A repository pinned to 0.1.0 is not checked for work-item ids
    Given the manifest pins schema version 0.1.0 instead
    And two work items numbered 0118, each in its own folder
    When the check runs
    Then no work-item id finding is reported
