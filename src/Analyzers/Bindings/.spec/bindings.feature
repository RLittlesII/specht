@0201-F1
Feature: Binding identity

  @B-001
  Scenario: Consumer grammars
    Given a manifest defining custom Feature and claim grammars and records using them
    When the declared analysis is requested
    Then references are validated and resolved using those configured grammars

  @B-002
  Scenario: Owning scope
    Given two Features declaring the same local claim ID
    When the declared analysis is requested
    Then a qualified claim reference resolves only in its declared owning Feature

  @B-003
  Scenario: No epic tier
    Given a consumer whose configured model has no epic tier
    When the declared analysis is requested
    Then a Feature and claim reference resolves without inventing an epic

  @B-004
  Scenario: ADR ownership
    Given a repository ADR and Feature ADRs sharing a local number
    When the declared analysis is requested
    Then an ADR reference resolves only in its declared repository or Feature scope

  @B-005
  Scenario: Framework independence
    Given the same specification inputs and declared references with no Reqnroll or test-framework assemblies
    When the declared analysis is requested
    Then reference resolution yields the same identities and findings
