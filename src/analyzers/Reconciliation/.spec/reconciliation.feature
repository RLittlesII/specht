@0201-F5
Feature: Traceability reconciliation

  @B-001
  Scenario: Direct test chain
    Given a claim bound to a discovered test through the common runner-evidence contract and a matching result with no scenario policy
    When the evidence gate is requested for the declared scope
    Then reconciliation presents the claim-to-method-to-case-to-result chain without requiring Reqnroll

  @B-002
  Scenario: Incomplete scope
    Given a declared repository scope omitting a required test project or input
    When the evidence gate is requested for the declared scope
    Then the report marks coverage incomplete and identifies the missing input

  @B-003
  Scenario: Outcome distinction
    Given a resolved binding with a failed, skipped or absent test result
    When the evidence gate is requested for the declared scope
    Then the report keeps that outcome distinct from a passing result

  @B-004
  Scenario: Policy gaps
    Given an explicit policy requiring a link absent from a claim's evidence
    When the evidence gate is requested for the declared scope
    Then the report identifies that missing link instead of treating reference resolution as complete coverage

  @B-005
  Scenario: Optional scenarios
    Given scenario bridge evidence supplied for part of a declared scope
    When the evidence gate is requested for the declared scope
    Then those scenario links augment the direct chains without inventing links for other tests

  @B-006
  Scenario: Unbound claim
    Given a declared specification claim requiring test evidence with no test binding at all
    When the evidence gate is requested for the declared scope
    Then reconciliation reports the missing evidence starting from that claim even when no method names it

  @B-007 @boundary
  Scenario: Ineligible target evidence
    Given a resolving target classified as ineligible by the binding lifecycle policy
    And a binding to that target
    When evidence is reconciled for the current scope
    Then that binding does not satisfy a current evidence requirement
    And the historical link retains the target identity and lifecycle state

  @B-008
  Scenario: Default test requirement
    Given a non-exempt current claim in a selected Feature
    And no valid test binding to that claim
    When coverage requirements are evaluated
    Then the claim has missing required test evidence

  @B-009
  Scenario Outline: Optional link requirements
    Given a current claim with no "<kind>" link
    And the consumer "<policy>" that link requirement
    When coverage requirements are evaluated
    Then the missing link is "<outcome>"

    Examples:
      | kind           | policy                  | outcome                      |
      | implementation | has not enabled         | not a required-link gap      |
      | scenario       | has not enabled         | not a required-link gap      |
      | implementation | has explicitly enabled  | reported as a required gap   |
      | scenario       | has explicitly enabled  | reported as a required gap   |

  @B-010 @boundary
  Scenario: Visible coverage exemption
    Given an explicit coverage exemption with a reason
    When coverage is presented
    Then the exemption and reason remain visible
    And the exemption is classified as exempt rather than covered

  @B-010 @boundary
  Scenario: An exempt test link leaves the scenario requirement active
    Given an explicit coverage exemption with a reason
    And a required scenario link with explicit bridge evidence
    When coverage is presented
    Then the test link is exempt and the scenario link is satisfied

  @B-011
  Scenario Outline: Declared compilation completeness
    Given a declared set of project and target-framework combinations
    And "<availability>"
    When analysis completeness is evaluated
    Then the analysis is "<state>"

    Examples:
      | availability                                      | state      |
      | current required inputs for every combination     | complete   |
      | a project combination omitted                     | incomplete |
      | a target-framework combination omitted            | incomplete |
      | stale required inputs for one combination         | incomplete |

  @B-011 @boundary
  Scenario: Older export beside current export
    Given a stale export beside a current export for the same declared combination
    When analysis completeness is evaluated
    Then the analysis is "complete"
    And the stale export remains visible without displacing current evidence

  @B-011 @boundary
  Scenario: Selected record absent from the input index
    Given a declared compilation export omits the selected claim record from its input index
    When analysis completeness is evaluated
    Then the analysis is "incomplete"
    And the missing record is identified as a required input

  @B-012 @boundary
  Scenario Outline: Stale evidence state
    Given "<state>" discovery or result evidence
    When evidence is reconciled
    Then the evidence remains visibly "<state>"
    And it does not establish current discovery or execution

    Examples:
      | state      |
      | stale      |
      | mismatched |

  @B-013 @boundary
  Scenario Outline: Binding and execution independence
    Given a valid binding with "<state>" test results
    When evidence is reconciled
    Then binding validity is evaluated independently of the execution-evidence state
    And the results are not treated as passing

    Examples:
      | state      |
      | absent     |
      | stale      |
      | mismatched |

  @B-014
  Scenario Outline: Trace report surface
    Given supplied traceability evidence
    When specht evidence is requested with "<format>" output
    Then reconciliation is presented in that format

    Examples:
      | format         |
      | human summary  |
      | versioned JSON |

  @B-014
  Scenario: Caller-named JSON report
    Given supplied traceability evidence
    When specht evidence writes a caller-named report
    Then that report contains the same versioned JSON as stdout

  @B-014 @boundary
  Scenario: Unwritable caller-named report
    Given supplied traceability evidence
    When specht evidence cannot write the caller-named report
    Then the command reports the write failure without exposing an absolute path

  @B-014 @boundary
  Scenario: External evidence directory
    Given supplied traceability evidence
    When evidence is supplied from an external directory containing two exports
    Then both external exports have distinct root-safe labels

  @B-014 @boundary
  Scenario: Overlapping evidence selections
    Given supplied traceability evidence
    When the same export is selected by file and directory
    Then it is reconciled once

  @B-015 @boundary
  Scenario: Checker report isolation
    Given the existing checker report contract
    When the separate trace report is introduced
    Then the checker report contract is unchanged

  @B-016 @boundary
  Scenario: Consume supplied evidence
    Given discovery and result artifacts from a normal test workflow
    When reconciliation runs
    Then it consumes those artifacts without launching test execution

  @B-017
  Scenario Outline: Passing outcomes required by default
    Given complete current inputs and required links for a selected claim
    And its required test case has outcome "<outcome>"
    When the supplied evidence is reconciled by specht evidence
    Then the default trace gate "<gate>"
    And binding validity and execution outcome remain separate

    Examples:
      | outcome | gate      |
      | passed  | succeeds  |
      | failed  | fails     |
      | skipped | fails     |
      | notRun  | fails     |
      | aborted | fails     |
      | absent  | fails     |

  @B-018 @boundary
  Scenario: Planned rows
    Given a required claim whose outline has an active Examples block that passed and a planned Examples block that was skipped
    When the supplied evidence is reconciled by specht evidence
    Then the planned rows are reported as planned
    And the planned rows neither cover the claim nor fail the default trace gate

  @B-019
  Scenario Outline: Active row beside planned rows
    Given a required claim whose outline has a planned Examples block and an active row whose case was "<outcome>"
    When the supplied evidence is reconciled by specht evidence
    Then the default trace gate fails for that active row

    Examples:
      | outcome |
      | skipped |
      | absent  |

  @B-020 @boundary
  Scenario: Unselected case
    Given a required case discovered by a run that did not select it
    And no other run selected it
    When the supplied evidence is reconciled by specht evidence
    Then the case is not counted as passed
    And the default trace gate fails

  @B-021
  Scenario Outline: Partitioned runs
    Given two projects compile the same claim files
    And the first project's run selects "Category!=visual" and the second's selects "Category=visual"
    And the visual case selected by the second run "<outcome>"
    When the supplied evidence is reconciled by specht evidence
    Then the default trace gate "<gate>"

    Examples:
      | outcome          | gate     |
      | passed           | succeeds |
      | failed           | fails    |
      | has no outcome   | fails    |
