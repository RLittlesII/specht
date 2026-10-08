Feature: The build
  As the maintainer of this repository
  I want every gate a change must pass to be one named build target
  So that my shell, my commit and CI all ask the same question

  @B-001
  Scenario: The default build compiles and tests
    Given a fresh clone with the pinned SDK
    When the build runs with no target named
    Then the solution is compiled
    And the tests run after it
    And the build fails when either step fails

  @B-002
  Scenario: The Windows entry runs the same target
    Given a clone on Windows
    When a target is run through the Windows entry script
    Then the same target runs as through the Unix entry script

  @B-003
  Scenario: Unformatted code fails the format gate
    Given a source file whose formatting differs from the repository's rules
    When the format gate runs
    Then it fails
    And it names that file

  @B-004 @boundary
  Scenario: The format gate changes nothing
    Given a source file whose formatting differs from the repository's rules
    And a snapshot of every file in the clone
    When the format gate runs
    Then no file was created, modified or deleted

  @B-005
  Scenario: The test gate runs every tier
    Given a failing test in the integration tier only
    When the test gate runs
    Then the unit, integration and acceptance tiers each ran
    And the test gate fails

  @B-006
  Scenario: The unit tier runs alone
    When the unit tier is run
    Then only unit-tier tests run

  @B-007
  Scenario: The integration tier runs alone
    When the integration tier is run
    Then only integration-tier tests run

  @B-008
  Scenario: The acceptance tier runs the linked scenarios
    When the acceptance tier is run
    Then the scenarios of every linked specification companion run

  @B-009
  Scenario: Packing writes one tool package
    When the pack target runs
    Then exactly one tool package is written to the build's package output
    And its name carries the version computed for the commit

  @B-010
  Scenario: A fresh clone restores every tool the build needs
    Given a fresh clone on a machine with no tools installed
    When the local tools are restored
    Then every tool the build and the hook invoke is available
    And each at the version the committed tool manifest pins

  @B-011
  Scenario: An installed hook runs on commit
    Given the hooks were installed in the clone
    When a commit is made
    Then the pre-commit hook runs

  @B-012
  Scenario: The hook refuses unformatted staged code
    Given a staged source file whose formatting differs from the repository's rules
    When a commit is attempted
    Then the commit is refused
    And the hook names that file

  @B-013
  Scenario: The hook refuses a staged specification that fails
    Given a staged specification that breaks an error-severity rule
    When a commit is attempted
    Then the commit is refused

  @B-014
  Scenario: The hook lets an unrelated commit through
    Given a commit that stages no source code, no Markdown and no specification
    When the commit is attempted
    Then none of the hook's checks runs
    And the commit is made

  @B-016
  Scenario: The hook refuses unformatted staged Markdown
    Given a staged Markdown file whose formatting differs from the repository's rules
    When a commit is attempted
    Then the format gate checks the staged files
    And the commit is refused
    And the hook names that file

  @B-017
  Scenario: Unformatted Markdown fails the format gate
    Given a Markdown file whose formatting differs from the repository's rules
    When the format gate runs
    Then it fails
    And it names that file

  @B-015 @boundary
  Scenario: The hook changes nothing it checks
    Given a staged source file whose formatting differs from the repository's rules
    And a snapshot of the working tree and the staged changes
    When a commit is attempted
    Then the working tree and the staged changes are unchanged
