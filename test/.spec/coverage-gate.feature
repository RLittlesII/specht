Feature: The coverage gate
  As the reviewer of a pull request
  I want the code a change adds to be held to a coverage target
  So that untested code cannot merge, and a change is never blocked for code it did not touch

  @B-001
  Scenario: A test run writes coverage
    When the tests run through the build
    Then a coverage report is written for each test project in the build's coverage output

  @B-002
  Scenario: Integration uploads coverage
    When integration runs on any operating system
    Then that run's coverage is sent to the coverage service for its commit

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
