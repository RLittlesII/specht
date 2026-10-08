Feature: Package versioning
  As a consumer pinning the tool
  I want each package version to name exactly one commit
  So that a pin means one thing on every machine and a release cannot disagree with its tag

  @B-001
  Scenario: One commit has one version everywhere
    Given one commit checked out with its full history on two machines
    When each computes the package version
    Then both versions are the same

  @B-002
  Scenario: A later commit has a higher version
    Given two consecutive commits on the main branch with the version file unchanged
    When each computes the package version
    Then the later commit's version is higher

  @B-003
  Scenario: A main-branch build is a public version
    Given a commit on the main branch
    When its package version is computed
    Then the version carries no prerelease suffix

  @B-004
  Scenario: A branch build is a prerelease
    Given a commit on a feature branch
    When its package version is computed
    Then the version carries a prerelease suffix naming the commit

  @B-005
  Scenario: The package carries the computed version
    Given a commit
    When the tool is packed
    Then the package's version is the version computed for that commit

  @B-006 @boundary
  Scenario: A package version change leaves the schema version
    Given a commit that changes only the package's version
    When the tool is packed
    Then the schema versions the tool embeds are unchanged
    And the schema version a new repository is initialised with is unchanged

  @B-007
  Scenario: The release tag is made from the computed version
    Given a commit on the main branch
    When the maintainer creates its release tag with the versioning tool
    Then a version tag naming that commit's computed version points at it
