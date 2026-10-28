Feature: Docs as first-class citizen
  As a contributor to specht
  I want documentation generated and checked inside the build
  So that the public surface and the command-line reference stay in step with the code, with no separate step and no hosted site

  # Mechanism only: doc-comment enforcement on the public surface, a
  # generated API reference as a build artifact, and one hand-written
  # command-line reference page held to the tool's own help. The site is
  # 0002-F2, deferred.

  @B-001
  Scenario: An undocumented public member fails the build
    Given a public member in the engine with no XML doc comment
    When the default build runs
    Then the build fails
    And the failure reports CS1591 as an error

  @B-001 @boundary
  Scenario: An undocumented internal member does not fail the build
    Given every public type and member carries an XML doc comment
    And an internal member with no XML doc comment
    When the default build runs
    Then the build reports no documentation error

  @B-002
  Scenario: The API reference is generated from the doc comments
    Given every public type and member carries an XML doc comment
    When the documentation target runs
    Then an API reference for the current public surface is written under the artifacts folder
    And no copy of that reference is tracked in the repository

  @B-003
  Scenario: The command-line reference is one page
    Given the repository's documentation
    Then the command-line reference is one page of it
    And it is not a command's specification

  @B-004 @boundary
  Scenario: Documentation generation publishes nothing
    When the documentation target runs
    Then nothing is published
    And no site framework is restored or invoked

  @B-005
  Scenario: The default build runs documentation enforcement and generation
    When the default build runs
    Then the documentation target runs as part of it

  @B-006
  Scenario: Every command has a section in the reference
    Given the commands the tool's help lists
    When the command-line reference is compared with them
    Then each command has a section of its own in the reference

  @B-007
  Scenario: Every option has an entry under its command
    Given the options a command's help lists
    When the command-line reference is compared with them
    Then each option has an entry in that command's section

  @B-008
  Scenario: The reference documents nothing the tool does not accept
    Given the command-line reference
    When it is compared with the tool's help
    Then every command with a section is one the help lists
    And every option with an entry is one its command's help lists

  @B-008 @boundary
  Scenario: An unsupported command section is rejected
    Given a copy of the command-line reference with a synthetic unsupported command section
    When the command-line reference is compared with the tool's help
    Then the comparison rejects that unsupported command section

  @B-009
  Scenario: The front page shows only what the tool accepts
    Given the repository's front page
    When it is compared with the tool's help
    Then every command and option it names is one the help lists

  @B-010
  Scenario: The package readme shows only what the tool accepts
    Given the readme the package carries
    When it is compared with the tool's help
    Then every command and option it names is one the help lists

  @B-011
  Scenario: The front page links to the reference
    Given the repository's front page
    Then it links to the command-line reference
