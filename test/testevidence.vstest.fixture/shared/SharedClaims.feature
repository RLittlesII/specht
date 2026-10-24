Feature: Shared claims

  @visual
  Scenario: Visual claim
    Given the claim holds

  Scenario Outline: Nonvisual claim
    Given the claim holds for <row>

    Examples:
      | row |
      | one |
      | two |
