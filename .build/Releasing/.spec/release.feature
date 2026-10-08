Feature: Release
  As the maintainer of this repository
  I want a release to be one version tag that publishes only what passed every gate
  So that a consumer's pin always names a package that was checked

  @B-001
  Scenario: A version tag runs every gate
    When a version tag is pushed
    Then the format, compile, test, self-check and pack gates run on the tagged commit

  @B-002
  Scenario: A failed gate publishes nothing
    Given a tagged commit whose tests fail
    When its version tag is pushed
    Then no package is published
    And no release is created

  @B-003
  Scenario: A green tag publishes the package
    Given a tagged commit that passes every gate
    When its version tag is pushed
    Then the tool package is published to the repository's package feed

  @B-004
  Scenario: A published version gets a release
    Given the package for a version tag was published
    Then a release named for that tag exists on the repository

  @B-005 @boundary
  Scenario: Nothing but a version tag publishes
    When a branch is pushed, or a tag that is not a version tag
    Then no package is published
    And no release is created

  @B-006
  Scenario: A release carries generated notes
    Given two pull requests were merged since the previous version tag
    When a version tag is published
    Then its release notes list both pull requests

  @B-007
  Scenario: A release carries the package
    When a version tag is published
    Then its release has the published tool package attached

  @B-008 @boundary
  Scenario: A dry run stops before the push
    When a release dry run is started without a tag
    Then every gate and the pack run
    And no package is published
    And no release is created

  @B-009
  Scenario: A tag that disagrees with the version publishes nothing
    Given a version tag whose version differs from the version computed for its commit
    When the tag is pushed
    Then the run fails
    And no package is published
    And no release is created
