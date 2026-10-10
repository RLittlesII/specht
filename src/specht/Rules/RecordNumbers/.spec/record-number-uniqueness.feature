Feature: Record number uniqueness
  As whoever follows a decision, ADR or lesson number to the record it names
  I want every number in a record folder claimed by one file
  So that a cited number names one record and a number taken twice is caught by the check

  Background:
    Given a repository whose manifest pins a schema version that carries the record number rule
    And the manifest declares nothing about records

  @B-001
  Scenario: A record number claimed twice is reported
    Given a Feature whose decisions folder holds two records numbered 0003
    When the check runs
    Then a claimed-twice finding names 0003

  @B-001
  Scenario: A number a branch and its base both took is reported in the merged tree
    Given a branch and its base each added a different decision record numbered 0007 to one Feature
    And the tree being checked is the merge of the two
    When the check runs
    Then a claimed-twice finding names 0007

  @B-002
  Scenario Outline: Every claimant carries a finding
    Given a Feature whose decisions folder holds <count> records numbered 0003
    When the check runs
    Then <count> claimed-twice findings are reported, one on each of those records

    Examples:
      | count |
      | two   |
      | three |

  @B-003
  Scenario: The finding names the number and every claimant
    Given a Feature whose decisions folder holds three records numbered 0003
    When the check runs
    Then each claimed-twice finding names 0003 and the paths of all three records

  @B-004
  Scenario: Claimants are named in one order everywhere
    Given a Feature whose decisions folder holds three records numbered 0003
    When the check runs
    Then each claimed-twice finding names the three paths in the same order, sorted by path

  @B-005
  Scenario: Every path in a finding is relative to the repository
    Given a Feature whose decisions folder holds two records numbered 0003
    When the check runs
    Then every path the claimed-twice findings name starts at the repository root and is written with forward slashes

  @B-006
  Scenario: Records each numbered once report nothing
    Given a Feature whose decisions folder holds records numbered 0001, 0002 and 0003
    When the check runs
    Then no claimed-twice finding is reported

  @B-006 @boundary
  Scenario: A number the base took is not seen in the branch's own tree
    Given a branch and its base each added a different decision record numbered 0007 to one Feature
    And the tree being checked is the branch's own, which holds one of them
    When the check runs
    Then no claimed-twice finding is reported

  @B-007 @boundary
  Scenario Outline: The same number in two folders is not a duplicate
    Given a record numbered 0001 in <one folder>
    And a record numbered 0001 in <another folder>
    When the check runs
    Then no claimed-twice finding is reported

    Examples:
      | one folder                     | another folder                     |
      | one Feature's decisions folder | another Feature's decisions folder |
      | a Feature's ADR folder         | the same Feature's lessons folder  |
      | a Feature's ADR folder         | the repository-wide ADR folder     |
      | the repository-wide ADR folder | the repository-wide lessons folder |

  @B-008 @boundary
  Scenario Outline: A file that is not named as a record claims no number
    Given a Feature whose decisions folder holds a record named 0003-first-call.md
    And a file named <name> in that folder
    When the check runs
    Then no claimed-twice finding is reported

    Examples:
      | name                 |
      | README.md            |
      | .gitkeep             |
      | 0003.md              |
      | 0003_second-call.md  |
      | 0003-second-call.txt |
      | 00030-second-call.md |
      | draft-0003-second.md |

  @B-009 @boundary
  Scenario: A file in a subfolder of a record folder claims no number
    Given a Feature whose decisions folder holds a record numbered 0003
    And a subfolder of that decisions folder holds a file named as a record numbered 0003
    When the check runs
    Then no claimed-twice finding is reported

  @B-010 @boundary
  Scenario: Numbers that differ only by leading zeros are two numbers
    Given the manifest's record numbers may be written with or without leading zeros instead
    And a Feature whose decisions folder holds records numbered 3 and 0003
    When the check runs
    Then no claimed-twice finding is reported

  @B-011 @boundary
  Scenario: The number is read from the file name, not from the record
    Given a Feature whose decisions folder holds records numbered 0003 and 0004
    And the titles of both records say they are decision 0003
    When the check runs
    Then no claimed-twice finding is reported

  @B-012 @boundary
  Scenario: A skipped number is not a finding
    Given a Feature whose decisions folder holds records numbered 0001 and 0003, and none numbered 0002
    When the check runs
    Then no record number finding is reported

  @B-013
  Scenario Outline: Record folders are read beside a specification in every layout
    Given a Feature specified in <layout> whose decisions folder holds two records numbered 0003
    When the check runs
    Then a claimed-twice finding names 0003

    Examples:
      | layout                         |
      | the features layout            |
      | the epics layout               |
      | a layout the manifest declares |

  @B-014 @boundary
  Scenario: A folder named like a record folder elsewhere is not checked
    Given a folder named adr that sits beside no specification and is not a repository-wide record folder
    And that folder holds two files named as records numbered 0003
    When the check runs
    Then no claimed-twice finding is reported

  @B-015 @boundary
  Scenario: A record folder under an excluded folder is not checked
    Given a copy of a Feature under a folder the manifest excludes
    And the copy's decisions folder holds two records numbered 0003
    When the check runs
    Then no claimed-twice finding is reported

  @B-016
  Scenario: The repository-wide record folders are checked though the folder above them is excluded
    Given the manifest excludes the folder the repository-wide ADR folder sits in
    And the repository-wide ADR folder holds two records numbered 0003
    When the check runs
    Then a claimed-twice finding names 0003

  @B-017 @boundary
  Scenario Outline: A record folder that is not there reports nothing
    Given <a repository without one of its record folders>
    When the check runs
    Then no record number finding is reported

    Examples:
      | a repository without one of its record folders      |
      | a Feature with no decisions folder                  |
      | a repository with no repository-wide lessons folder |

  @B-018
  Scenario Outline: The three record folders beside a specification are checked by default
    Given a Feature whose <folder> folder holds two records numbered 0003
    When the check runs
    Then a claimed-twice finding names 0003

    Examples:
      | folder    |
      | decisions |
      | adr       |
      | lessons   |

  @B-019
  Scenario Outline: The repository-wide record folders are checked by default
    Given the repository-wide <folder> folder holds two records numbered 0003
    When the check runs
    Then a claimed-twice finding names 0003

    Examples:
      | folder  |
      | adr     |
      | lessons |

  @B-020
  Scenario: A date-named file is a record by default
    Given a Feature whose decisions folder holds files named 2024-10-01-review.md and 2024-11-05-review.md
    When the check runs
    Then a claimed-twice finding names 2024 and both files

  @B-020
  Scenario: A record's name may end at the hyphen by default
    Given a Feature whose decisions folder holds files named 0003-first-call.md and 0003-.md
    When the check runs
    Then a claimed-twice finding names 0003 and both files

  @B-021
  Scenario Outline: A record folder the manifest adds is checked
    Given the manifest also names <folder> as a record folder
    And that folder holds two records numbered 0003
    When the check runs
    Then a claimed-twice finding names 0003

    Examples:
      | folder                                     |
      | a folder named rfcs beside a specification |
      | the repository-wide folder docs/adr        |

  @B-022
  Scenario: The shipped manifest names the record folders
    When the manifest the tool ships for that schema version is read
    Then it names the three record folders beside a specification, the two repository-wide record folders, the record file shape and the record number grammar

  @B-023
  Scenario: Record number findings are errors by default
    Given the manifest sets no severity for the record number rule
    And a Feature whose decisions folder holds two records numbered 0003
    When the check runs
    Then the claimed-twice findings are reported as errors

  @B-024
  Scenario: A disabled record number rule reports nothing
    Given the manifest disables the record number rule
    And a Feature whose decisions folder holds two records numbered 0003
    When the check runs
    Then no record number finding is reported

  @B-025 @boundary
  Scenario: A repository pinned to 0.1.0 is not checked for record numbers
    Given the manifest pins schema version 0.1.0 instead
    And a Feature whose decisions folder holds two records numbered 0003
    When the check runs
    Then no record number finding is reported

  @B-026
  Scenario: A record number is four digits by default
    Given a Feature whose decisions folder holds files named 0003-first-call.md and 0003-second-call.md
    When the check runs
    Then a claimed-twice finding names 0003 and both files

  @B-026 @boundary
  Scenario Outline: A run of digits that is not four long is no record number by default
    Given a Feature whose decisions folder holds files named <one> and <another>
    When the check runs
    Then no claimed-twice finding is reported

    Examples:
      | one                 | another              |
      | 003-first-call.md   | 003-second-call.md   |
      | 00030-first-call.md | 00030-second-call.md |

  @B-027
  Scenario: A record number grammar the manifest declares is read
    Given the manifest's record numbers are three digits instead
    And a Feature whose decisions folder holds files named 003-first-call.md and 003-second-call.md
    When the check runs
    Then a claimed-twice finding names 003 and both files

  @B-028
  Scenario: A record file shape the manifest declares is read
    Given the manifest declares that a record is named with a record number, a hyphen, any text and a text-file extension
    And a Feature whose decisions folder holds files named 0003-first-call.txt and 0003-second-call.txt
    When the check runs
    Then a claimed-twice finding names 0003 and both files
