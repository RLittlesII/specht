Feature: Continuous integration
  As the reviewer of a pull request
  I want every change built and checked on every operating system the tool runs on
  So that a merge never rests on the author's word or on one machine

  @B-001
  Scenario: A pull request is built
    When a pull request targeting the main branch is opened
    Then the build runs against the pull request's head

  @B-002
  Scenario: A push to main is built
    When a commit is pushed to the main branch
    Then the build runs against that commit

  @B-003
  Scenario: Every run covers Linux and Windows
    When integration runs
    Then the build runs on Linux and on Windows

  @B-004
  Scenario: Every operating system runs every gate
    When integration runs on one operating system
    Then the format, compile, test, self-check and pack gates each run

  @B-005
  Scenario: A failing gate fails its operating system's check
    Given a change whose tests fail on Windows only
    When integration runs
    Then the Windows check fails

  @B-006
  Scenario: A specification violation is shown on the diff
    Given a pull request that breaks a specification rule
    When integration runs
    Then the violation is annotated on its file and line in the pull request's diff

  @B-007
  Scenario: Check names do not change between runs
    Given two runs on different commits
    Then each operating system's check has the same name in both

  @B-008 @boundary
  Scenario: Integration never publishes
    When integration runs on a pull request or on the main branch
    Then no package is pushed to any feed

  @B-009
  Scenario: A stale workflow fails the run
    Given a commit whose build declares a step one of its committed workflows lacks
    When integration runs
    Then the run fails

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
