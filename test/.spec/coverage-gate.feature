Feature: The coverage gate
  As the reviewer of a pull request
  I want the code a change adds to be held to a coverage target
  So that untested new code is flagged on the pull request, and a change is never blocked for code it did not touch

  @B-001
  Scenario: A test run writes coverage
    When the tests run through the build
    Then a coverage report is written for each test project in the build's coverage output

  @B-002
  Scenario: Integration uploads coverage
    When integration runs the build's gates
    Then the Linux run's coverage is sent to the coverage service for its commit

  @B-002 @boundary
  Scenario: The Windows run uploads no coverage
    When integration runs the build's gates
    Then the Windows run's tests run
    And none of the Windows run's coverage is sent to the coverage service

  @B-002 @boundary
  Scenario: A documentation-only pull request uploads no coverage
    Given a pull request whose integration run skips the build's gates
    When integration runs
    Then no coverage is sent to the coverage service

  @B-003
  Scenario: Under-tested new code fails the patch status
    Given a pull request whose changed product lines are 79% covered
    When its coverage is reported
    Then the patch coverage status fails

  @B-004
  Scenario: Tested new code passes the patch status
    Given a pull request whose changed product lines are 80% covered
    When its coverage is reported
    Then the patch coverage status passes

  @B-005 @boundary
  Scenario: A lower total is reported and does not block
    Given a pull request that lowers the repository's total coverage
    And its changed lines meet the patch target
    When its coverage is reported
    Then the project coverage status shows the total and the change
    And the project coverage status passes

  @B-006 @boundary
  Scenario: Changes outside the product are not measured
    Given a pull request that changes only test code
    When its coverage is reported
    Then none of its changed lines count toward the patch coverage

  @B-007
  Scenario: A failed upload warns and does not fail the run
    Given the coverage service cannot be reached
    When integration runs and every gate passes
    Then each operating system's check passes
    And the run reports a warning that the coverage upload failed

  @B-008
  Scenario: A change with no measured lines passes the patch status
    Given a pull request that changes no product line
    And integration runs the build's gates on it
    When its coverage is reported
    Then the patch coverage status passes
