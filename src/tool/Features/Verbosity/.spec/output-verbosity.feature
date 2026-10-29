Feature: Output verbosity
  As a consumer who reads the check's output only when it fails
  I want to ask a command for less on its standard output
  So that I see less without the verdict, the report or the gate changing

  Background:
    Given a repository root with a valid Specht manifest

  @B-001 @boundary
  Scenario: A run without a level prints what it does today
    Given a command that prints lines on its standard output
    When the command runs without a verbosity request
    Then its standard output is what that command's own Feature defines

  @B-002 @boundary
  Scenario: A quiet request keeps the exit code
    Given a tree whose check reports an error
    When the check runs with a quiet request
    Then it exits with the same code as the same check without the request

  @B-003 @boundary
  Scenario: A quiet request keeps the JSON document
    Given a tree whose check reports violations
    When the check runs with a quiet request and asks for JSON
    Then the JSON document is the one the same check writes without the request

  @B-004 @boundary
  Scenario: A quiet request keeps the report file
    Given a tree whose check reports violations
    When the check runs with a quiet request and names a report file
    Then the report file is the one the same check writes without the request

  @B-005 @boundary
  Scenario: A quiet request keeps stderr
    Given a root with no manifest
    When the check runs with a quiet request
    Then the message about the missing manifest is the one written without the request

  @B-006 @boundary
  Scenario: A quiet request writes the same files
    Given a root where init would write some files and skip others
    When init runs with a quiet request
    Then the files under the root are the ones init writes without the request

  @B-007
  Scenario: A quiet check prints no summary
    Given a tree whose check reports violations
    When the check runs with a quiet request
    Then no summary line is printed
