Feature: Consuming the package
  As the maintainer of a repository that wants the checker
  I want one document saying what to commit and what to supply to install the tool
  So that every consumer installs it the same way, with no credential committed

  @B-001
  Scenario: A new repository installs the tool from the feed
    Given an empty repository with no package source of its own
    And no local tool manifest exists
    And no credential for any package feed
    When the prerelease tool is installed locally
    Then the tool is added to the repository's local tool manifest

  @B-002
  Scenario: A clone restores the pinned tool
    Given a clone whose committed tool manifest names the tool at a version published to nuget.org
    And no credential for any package feed
    When the local tools are restored
    Then the tool is available at the pinned version

  @B-003
  Scenario: This repository installs its own published tool
    Given the first package has been published
    Then this repository's committed tool manifest names the tool at a published version

  @B-007
  Scenario: A contributor without a token can build
    Given a clone of this repository after the first publish
    And no token for the package feed
    When the solution is restored and built
    Then the build succeeds

  @B-010
  Scenario: This repository installs its own tool from nuget.org
    Given a version of the tool has been published to nuget.org
    Then this repository's committed tool manifest names the tool at a version published there

  @B-011 @boundary
  Scenario: This repository drops the authenticated feed
    Given this repository's committed tool manifest names a version published to nuget.org
    Then its committed package configuration names no GitHub Packages source
