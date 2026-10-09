Feature: Continuous integration
  As the reviewer of a pull request
  I want every change built and checked on every operating system the tool runs on
  So that a merge never rests on the author's word or on one machine

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
    Given a pull request that changes code
    When integration runs on one operating system
    Then the format, compile, test and self-check gates each run, and the benchmarks

  @B-005
  Scenario: A failing gate fails its operating system's check
    Given a change whose tests fail on Windows only
    When integration runs
    Then the Windows check fails

  @B-006
  Scenario: A specification violation is shown on the diff
    Given a pull request that changes a file carrying a specification violation
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
