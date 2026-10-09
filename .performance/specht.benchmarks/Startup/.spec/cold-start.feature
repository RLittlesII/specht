Feature: Tool cold start
  As the maintainer of four repositories
  I want the packed tool's start measured the way a consumer runs it
  So that I know what every hook run and every CI run pays before the engine starts work

  Background:
    Given the tool package the build packed
    And a generated tree of one specification

  @B-001
  Scenario: The packed tool is installed the way a consumer installs it
    When the cold-start benchmark prepares
    Then the tool is installed from the local package output into a temporary local tool manifest
    And no package feed is contacted

  @B-002
  Scenario: A check by the packed tool is measured start to exit
    When the cold-start benchmark runs
    Then one check of the tree has a result measured from the tool's start to its exit

  @B-003
  Scenario: Every measured invocation is a fresh process
    When the cold-start benchmark measures two invocations
    Then each invocation ran in a process of its own

  @B-004
  Scenario: A tool that fails is not timed
    Given a tree on which the check exits with violations
    When the cold-start benchmark runs on it
    Then the benchmark fails instead of reporting a time

  @B-005 @boundary
  Scenario: A run leaves no tool installed behind it
    When the cold-start benchmark completes
    Then no tool is installed globally, in the user's package cache or in this repository's tool manifest
