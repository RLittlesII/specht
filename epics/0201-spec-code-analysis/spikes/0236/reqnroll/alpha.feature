@feature-F1 @epic-0001
Feature: Synthetic alpha increments

  @B-001
  Scenario: Increment a fixed value
    Given the value is 2
    When it is incremented
    Then the value is 3

  @B-002
  Scenario Outline: Increment boundary values
    Given the value is <input>
    When it is incremented
    Then the value is <output>

    Examples:
      | input | output |
      | 0     | 1      |
      | 3     | 4      |
