Feature: Docs as first-class citizen
  As a contributor to specht
  I want documentation generated and checked inside the build
  So that the public surface and each command's usage doc stay in step with the code, with no separate step and no hosted site

  # Mechanism only: doc-comment enforcement on the public surface, a
  # generated API reference as a build artifact, and a co-location
  # convention for usage docs. The site is 0002-F2, deferred.

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
  Scenario: A command's usage doc sits beside the command
    Given a command folder under the tool's features
    Then its usage doc is in that folder, at the location the convention names
    And it is not the command's specification

  @B-004 @boundary
  Scenario: Documentation generation publishes nothing
    When the documentation target runs
    Then nothing is published
    And no site framework is restored or invoked

  @B-005
  Scenario: The default build runs documentation enforcement and generation
    When the default build runs
    Then the documentation target runs as part of it
