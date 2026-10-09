Feature: Benchmark harness
  As the maintainer of four repositories
  I want the tool's costs measured by one command, locally and in CI, with memory on every measurement
  So that a cost a specification names has a number, and no number ever fails a build

  @B-001
  Scenario: The benchmarks are compiled with the solution
    When the solution is compiled
    Then the benchmark project is compiled with it

  @B-002 @boundary
  Scenario: The default build runs no benchmark
    When the default build runs
    Then no benchmark runs

  @B-003
  Scenario: Every benchmark runs when none is named
    When the benchmark target runs with no filter
    Then every benchmark in the project runs

  @B-004
  Scenario: A filter runs only the benchmarks it names
    Given benchmarks for discovery and for a full check
    When the benchmark target runs with a filter naming discovery
    Then only the discovery benchmarks run

  @B-005
  Scenario: A named job is the job every benchmark runs under
    When the benchmark target runs with the short job named
    Then every selected benchmark runs under the short job

  @B-006
  Scenario: With no job named, the default job runs
    When the benchmark target runs with no job named
    Then every selected benchmark runs under the library's default job

  @B-007
  Scenario: Every benchmark reports the memory it allocates
    When any benchmark runs
    Then its result reports the managed memory allocated per operation

  @B-008
  Scenario: Results land in the benchmark results folder
    When the benchmark target runs
    Then its results are written to the benchmark results folder under the repository's artifacts folder

  @B-009
  Scenario: A person can read each class's results
    When the benchmark target runs two benchmark classes
    Then a human-readable summary is written for each of them

  @B-010
  Scenario: A machine can read each class's results
    When the benchmark target runs two benchmark classes
    Then a machine-readable result is written for each of them

  @B-011 @boundary
  Scenario: Whatever a benchmark measures, the build does not fail
    Given a benchmark, whatever time and allocation it measures
    When the benchmark target runs
    Then the benchmark target succeeds

  @B-012
  Scenario: A benchmark that breaks fails the target
    Given a benchmark that throws
    When the benchmark target runs
    Then the benchmark target fails

  @B-013
  Scenario: The tool is packed before the benchmarks run
    When the benchmark target runs with the cold-start benchmark selected
    Then the tool package is packed before the first benchmark starts

  @B-014
  Scenario: A run leaves the repository as it found it
    Given a clean working tree
    When the benchmark target runs
    Then nothing in the working tree has changed outside the artifacts folder

  @B-015
  Scenario: A generated tree passes the check
    Given a generated tree of 1, 10, 100 and 1000 specifications
    When the check runs on it
    Then no violation is reported

  @B-016
  Scenario: The same size generates the same tree
    When a tree of one size is generated twice
    Then both trees hold the same files at the same relative paths with the same contents

  @B-017
  Scenario: CI runs the benchmarks on every operating system
    Given a pull request that changes code
    When integration runs
    Then the benchmarks run on Linux and on Windows

  @B-018
  Scenario: CI uses the short job
    When integration runs the benchmarks
    Then every benchmark runs under the short job

  @B-019
  Scenario: CI publishes the results for each operating system
    When integration's benchmarks complete on Linux and on Windows
    Then the run keeps a results artifact for each operating system, named for it

  @B-020
  Scenario: The benchmarks run after every gate
    When integration runs on one operating system
    Then the benchmarks run after the format, compile, test and self-check gates

  @B-021 @boundary
  Scenario: A run without the cold-start benchmark packs nothing
    When the benchmark target runs with a filter naming only the stage benchmarks
    Then the tool package is not packed
