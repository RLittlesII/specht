Feature: Dependency updates
  As the maintainer of this repository
  I want routine updates proposed and merged once the build agrees, and breaking ones shown to me
  So that nothing ages silently and nothing breaks unseen

  @B-001
  Scenario: Package updates arrive together
    Given two packages each have a newer minor or patch release
    When updates are proposed
    Then one pull request carries both

  @B-002
  Scenario: Workflow action updates arrive together
    Given two workflow actions each have a newer minor or patch release
    When updates are proposed
    Then one pull request carries both

  @B-003
  Scenario: An SDK update arrives on its own
    Given a newer minor or patch release of the pinned SDK
    When updates are proposed
    Then one pull request carries it

  @B-004
  Scenario: Local tool updates arrive together
    Given two local tools each have a newer minor or patch release
    When updates are proposed
    Then one pull request carries both

  @B-005
  Scenario: A green routine update merges itself
    Given a routine update pull request
    When every required check passes
    Then it is merged without a person

  @B-006
  Scenario: A red routine update waits
    Given a routine update pull request
    When a required check fails
    Then it stays open and unmerged

  @B-007
  Scenario: A major update waits for a person
    Given a dependency has a new major release
    When updates are proposed
    Then a pull request carries it
    And it is never merged without a person

  @B-008 @boundary
  Scenario: The engine's dependencies are left alone
    Given the behaviour baseline has not been tagged
    And one of the engine's three dependencies has a newer release
    When updates are proposed
    Then no pull request updates it

  @B-009 @boundary
  Scenario: An action update leaves the generated workflow alone
    Given a workflow action has a newer minor release
    When updates are proposed
    Then the pull request changes the action's version where the build declares it
    And it changes no generated workflow file

  @B-010
  Scenario: Updates open no issue
    Given updates are available in every ecosystem
    When updates are proposed
    Then no issue is opened

  @B-011
  Scenario: Update pull requests are labelled
    When an update pull request is opened
    Then it carries the dependencies label

  @B-012
  Scenario: Each major update is reviewed alone
    Given two packages each have a new major release
    When updates are proposed
    Then each is carried by a pull request of its own
