@feature-F2 @epic-0001
Feature: Synthetic beta increments

  @B-001
  Scenario: Increment a separate Feature value
    Given the value is 8
    When it is incremented
    Then the value is 9
