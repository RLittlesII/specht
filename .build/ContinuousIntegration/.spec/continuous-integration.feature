Feature: Continuous integration
  As the reviewer of a pull request
  I want every change built and checked before it merges, and on every operating system the tool runs on once it reaches the main branch
  So that a merge never rests on the author's word, and the main branch never rests on one operating system

  @B-001
  Scenario: A pull request is built
    Given a pull request targeting the main branch that changes code
    When it is opened
    Then the build runs against the pull request's head

  @B-001 @boundary
  Scenario: A documentation change inside a specification folder is built
    Given a pull request that changes only Markdown inside a specification folder
    When integration runs
    Then the build runs against the pull request's head

  @B-001 @boundary
  Scenario: Documentation changed alongside code is built
    Given a pull request that changes documentation and code together
    When integration runs
    Then the build runs against the pull request's head

  @B-001 @boundary
  Scenario: The gates still build the pull request's head when its merge is checked
    Given a pull request that changes code
    When integration runs
    Then every gate but the second self-check runs against the pull request's head

  @B-002
  Scenario: A push to main is built
    When a commit is pushed to the main branch
    Then the build runs against that commit

  @B-002 @boundary
  Scenario: A documentation-only push to main is built
    Given a commit that changes only documentation outside any specification folder
    When it is pushed to the main branch
    Then the build runs against that commit

  @B-003
  Scenario: Every run covers Linux and Windows
    When integration runs
    Then integration runs on Linux and on Windows

  @B-004
  Scenario: Every operating system runs every gate
    Given a commit pushed to the main branch
    When integration runs on one operating system
    Then the format, compile, unit test, integration test, acceptance test and self-check gates and the benchmarks each run as a step of their own

  @B-004 @boundary
  Scenario: A pull request still runs every gate on Linux
    Given a pull request that changes code
    When integration runs on Linux
    Then the format, compile, unit test, integration test, acceptance test and self-check gates and the benchmarks each run as a step of their own

  @B-004 @boundary
  Scenario: A push to main still runs every gate on Windows
    Given a commit pushed to the main branch
    When integration runs on Windows
    Then the format, compile, unit test, integration test, acceptance test and self-check gates and the benchmarks each run as a step of their own

  @B-005
  Scenario: A failing gate fails its operating system's check
    Given a change whose tests fail on Windows only
    When it is pushed to the main branch
    Then the Windows check fails

  @B-006
  Scenario: A specification violation is shown on the diff
    Given a pull request whose own tree carries a specification violation in a file it changes
    When integration runs
    Then the violation is annotated on its file and line in the pull request's diff
    And it is annotated once, from the Linux check only

  @B-006 @boundary
  Scenario: A violation in a file the pull request does not change is not annotated
    Given a pull request that changes no file carrying a specification violation
    And a specification violation elsewhere in the repository
    When integration runs
    Then no violation is annotated on the pull request's diff

  @B-007
  Scenario: Check names do not change between runs
    Given two runs on different commits
    Then each operating system's check has the same name in both

  @B-008 @boundary
  Scenario: Integration never publishes
    When integration runs on a pull request or on the main branch
    Then no package is pushed to any feed

  @B-010
  Scenario: Each operating system is its own check
    When integration runs
    Then the Linux and Windows builds each report as a separate check

  @B-011
  Scenario: A run reads the feed with its own token
    Given this repository's tool manifest names the published checker
    When integration runs
    Then the checker is restored with the run's own token
    And no stored token is read

  @B-012
  Scenario: A documentation-only pull request skips the gates
    Given a pull request that changes only documentation outside any specification folder
    When integration runs
    Then none of the build's gates runs

  @B-013
  Scenario: A documentation-only pull request passes its checks
    Given a pull request that changes only documentation outside any specification folder
    When integration runs
    Then the Linux and Windows checks each pass

  @B-014
  Scenario: A pull request with no known changed files is built
    Given a pull request whose changed files cannot be determined
    When integration runs
    Then the build runs against the pull request's head

  @B-014 @boundary
  Scenario: A pull request that changes no file is built
    Given a pull request that changes no file
    When integration runs
    Then the build runs against the pull request's head

  @B-015
  Scenario: A pull request skips the gates on Windows
    Given a pull request that changes code
    When integration runs on Windows
    Then none of the build's gates runs

  @B-016
  Scenario: A pull request passes its Windows check
    Given a pull request that changes code
    When integration runs
    Then the Windows check passes

  @B-016 @boundary
  Scenario: A Windows-only failure does not fail a pull request's Windows check
    Given a pull request whose tests would fail on Windows only
    When integration runs
    Then the Windows check passes

  @B-017
  Scenario: A violation only the merge carries is reported before the merge
    Given a pull request whose own tree carries no specification violation
    And merging it with the main branch produces one
    When integration runs
    Then the self-check runs a second time, over the pull request merged with the main branch
    And that run reports the violation

  @B-017 @boundary
  Scenario: A change that reaches main after the run is not seen by it
    Given a pull request whose merged tree carried no violation when integration last ran
    And a change that has reached the main branch since and collides with it
    When the pull request is merged without integration running again
    Then the pull request's last run still reports no violation
    And the run on the main branch reports the collision

  @B-017 @boundary
  Scenario: A documentation-only pull request has no merged tree checked
    Given a pull request that changes only documentation outside any specification folder
    When integration runs
    Then the self-check runs over no merged tree

  @B-017 @boundary
  Scenario: A pull request has no merged tree checked on Windows
    Given a pull request that changes code
    When integration runs on Windows
    Then the self-check runs over no merged tree

  @B-018
  Scenario: A push to main is checked once
    When a commit is pushed to the main branch
    Then the self-check runs over that commit
    And the self-check runs over no merged tree

  @B-019
  Scenario: A merge that cannot be computed is reported, not passed
    Given a pull request that could be merged with the main branch when integration started
    And a change that has reached the main branch since and conflicts with it
    When integration runs
    Then the run says that no merged tree was checked
    And the run does not report the merged tree as clean

  @B-020
  Scenario: The merged tree's check annotates nothing
    Given a pull request whose own tree carries a specification violation in a file it changes
    When integration runs
    Then the check of the merged tree annotates nothing on the pull request's diff
    And the violation is annotated once

  @B-020 @boundary
  Scenario: A violation only the merge carries is in the log, not on the diff
    Given a pull request whose own tree carries no specification violation
    And merging it with the main branch produces one in a file the pull request adds
    When integration runs
    Then the violation is in the run's log
    And no violation is annotated on the pull request's diff

  @B-021
  Scenario: The merged tree's violations name repository paths
    Given a pull request whose merged tree carries a specification violation
    When integration runs
    Then the violation names its file by the file's path in the repository

  @B-022
  Scenario: The merged tree's check does not gate before the self-check does
    Given the self-check does not yet gate integration
    And a pull request whose merged tree carries a specification violation
    When integration runs
    Then the Linux check does not fail because of it

  @B-023
  Scenario: A violation the merged tree carries fails the Linux check once the self-check gates
    Given the self-check gates integration
    And a pull request whose own tree carries no specification violation
    And merging it with the main branch produces one
    When integration runs
    Then the Linux check fails

  @B-024
  Scenario: A merge that cannot be computed does not fail the Linux check
    Given the self-check gates integration
    And a pull request that could be merged with the main branch when integration started
    And a change that has reached the main branch since and conflicts with it
    When integration runs
    Then the Linux check does not fail because no merged tree was checked
