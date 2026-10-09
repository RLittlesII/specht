Feature: Engine stage benchmarks
  As the developer changing the engine
  I want each stage the engine is built from measured on its own
  So that a cost is traced to the stage that incurs it

  Background:
    Given generated trees of 1, 10, 100 and 1000 specifications

  @B-001
  Scenario: Loading the manifest and schemas is measured
    When the stage benchmarks run
    Then loading a tree's manifest and schemas has a result of its own

  @B-002
  Scenario: Discovery is measured at every tree size
    When the stage benchmarks run
    Then finding the specifications has a result for each tree size

  @B-003
  Scenario: Reading one specification's frontmatter is measured
    When the stage benchmarks run
    Then reading one specification's frontmatter has a result of its own

  @B-004
  Scenario: Parsing one specification is measured
    When the stage benchmarks run
    Then parsing one specification's text has a result of its own

  @B-005
  Scenario: Building the model is measured at every tree size
    When the stage benchmarks run
    Then building the model of a tree has a result for each tree size

  @B-006
  Scenario: Every rule is measured at every tree size
    Given a model of each tree built before the measurement
    When the stage benchmarks run
    Then each rule the engine runs has a result for each tree size
    And each rule that reads a file reads it from the generated tree

  @B-007
  Scenario: Evaluating a model is measured at every tree size
    Given a model of each tree built before the measurement
    When the stage benchmarks run
    Then evaluating the model has a result for each tree size
    And the companion scenario files it reads come from the generated tree

  @B-009
  Scenario: A new rule is measured without a new benchmark
    Given a rule added to the engine
    When the stage benchmarks run with no benchmark changed
    Then the new rule has a result for each tree size
