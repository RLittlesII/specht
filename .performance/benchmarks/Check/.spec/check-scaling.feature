Feature: End-to-end check scaling
  As the maintainer of four repositories
  I want a whole check measured over trees that grow a decade at a time
  So that I know how its cost grows before a repository is that large

  Background:
    Given generated trees of 1, 10, 100 and 1000 specifications

  @B-001
  Scenario: A whole check is measured at every tree size
    When the scaling benchmark runs
    Then a whole check has a result for each tree size

  @B-002
  Scenario: The measured check covers the whole tree
    When the scaling benchmark runs on the tree of 100 specifications
    Then the report the measured check returns counts 100 specifications

  @B-003 @boundary
  Scenario: The measured tree is laid out co-located
    When the scaling benchmark runs
    Then every specification it reads sits beside the code it specifies
    And none sits in the legacy epics layout

  @B-004
  Scenario: Each result names its tree size
    When the scaling benchmark runs
    Then each result names the number of specifications it was measured over
