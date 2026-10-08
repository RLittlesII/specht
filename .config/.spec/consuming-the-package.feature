Feature: Consuming the package
  As the maintainer of a repository that wants the checker
  I want one document saying what to commit and what to supply to install the tool
  So that every consumer installs it the same way, with no credential committed

  @B-001
  Scenario: A new repository installs the tool from the feed
    Given an empty repository with the documented package source
    And a token allowed to read packages
    When the documented install command runs
    Then the tool is added to the repository's local tool manifest

  @B-002
  Scenario: A clone restores the pinned tool
    Given a clone whose committed tool manifest names the tool
    And whose committed package source is the documented one
    And a token allowed to read packages
    When the local tools are restored
    Then the tool is available at the pinned version

  @B-003
  Scenario: This repository installs its own published tool
    Given the first package has been published
    Then this repository's committed tool manifest names the tool at a published version

  @B-004
  Scenario: The document names the feed and the source entry
    When a consumer reads the install document
    Then it names the feed's address
    And the package source entry to commit

  @B-005
  Scenario: The document names the token and how CI supplies it
    When a consumer reads the install document
    Then it names the permission the token needs
    And how a CI run supplies the token

  @B-006
  Scenario: The document names the install and restore commands
    When a consumer reads the install document
    Then it names the command that installs the tool locally
    And the command that restores it in a clone

  @B-007
  Scenario: A contributor without a token can build
    Given a clone of this repository after the first publish
    And no token for the package feed
    When the solution is restored and built
    Then the build succeeds
