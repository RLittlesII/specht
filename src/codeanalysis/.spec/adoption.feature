@0201-F6
Feature: Consumer analysis adoption

  @B-001
  Scenario: Selected scope
    Given a consumer selecting a subset of projects and declarations for enforcement
    When the declared analysis is requested
    Then only that selected scope is required to carry bindings and the scope is visible in evidence

  @B-002
  Scenario: Editor build parity
    Given the same valid pinned configuration and specification inputs supplied in editor and build
    When the declared analysis is requested
    Then the same binding yields the same resolution verdict

  @B-003
  Scenario: Invalid inputs
    Given a selected analysis scope with missing or invalid manifest or specification inputs
    When the declared analysis is requested
    Then analysis reports the input failure rather than silently reporting clean coverage

  @B-004
  Scenario: Optional Reqnroll
    Given a consumer selecting xUnit and Playwright .NET analysis without the scenario bridge
    When the declared analysis is requested
    Then adoption requires no Reqnroll installation

  @B-005
  Scenario Outline: Default warning findings
    Given "<finding>" within the selected enforcement scope
    And no severity override for that rule
    When the diagnostic is reported
    Then its default severity is warning

    Examples:
      | finding                         |
      | an invalid binding              |
      | an incomplete binding           |
      | an unresolved target            |
      | an ambiguous target             |
      | an unsupported binding placement |
      | a lifecycle-ineligible target   |
      | conflicting type defaults       |
      | a missing required binding      |

  @B-006
  Scenario: Informational duplicate findings
    Given duplicate valid relationships
    And no severity override for the duplicate rule
    When the duplicate diagnostic is reported
    Then its default severity is information

  @B-007
  Scenario Outline: Per-rule severity override
    Given a consumer selecting "<severity>" for one rule
    When that rule's diagnostic is reported
    Then it uses the selected severity

    Examples:
      | severity    |
      | information |
      | warning     |
      | error       |

  @B-008
  Scenario Outline: Standard Roslyn suppression
    Given a suppressible analyzer diagnostic
    And a consumer suppression using "<mechanism>"
    When diagnostics are presented
    Then the standard Roslyn suppression is honored without a custom suppression protocol

    Examples:
      | mechanism           |
      | analyzer configuration |
      | pragma warning      |
      | SuppressMessage     |

  @B-009 @boundary
  Scenario: Suppressed invalid binding
    Given an invalid binding whose diagnostic is suppressed
    When evidence validity is evaluated
    Then the binding remains invalid

  @B-010 @boundary
  Scenario: Suppressed missing evidence
    Given a required evidence link that is missing
    And the diagnostic for that missing link is suppressed
    When required-link completeness is evaluated
    Then that link is still missing

  @B-011 @boundary
  Scenario: Suppression preserves scope
    Given an explicitly declared analysis scope
    And a suppressed diagnostic within that scope
    When analysis scope is presented with the evidence
    Then the declared scope remains unchanged

  @B-012
  Scenario: Explicit binding validation
    Given an analyzed project with an invalid explicit binding
    And the declaration is outside the scope requiring authored bindings
    When binding analysis runs
    Then the explicit binding is still validated

  @B-013 @boundary
  Scenario: Generated authoring boundary
    Given generated code with no authored binding
    When missing-binding requirements are evaluated
    Then no authored binding is required on that generated code

  @B-014 @boundary
  Scenario: Shared infrastructure boundary
    Given shared infrastructure outside explicitly selected authoring enforcement scope
    When missing-binding requirements are evaluated
    Then its methods are not required to carry bindings merely because they are shared infrastructure

  @B-015 @boundary
  Scenario: Reasoned exemptions
    Given a requested coverage exemption with no explicit reason
    When coverage policy is validated
    Then that exemption is rejected

  @B-016
  Scenario: Consumer record inputs
    Given enabled analysis with actual consumer specification and ADR record values
    And build integration supplies the selected manifest, records and configured schemas through AdditionalFiles
    When bindings are resolved
    Then analysis reads the consumer record contents rather than treating schemas as the record inventory

  @B-017
  Scenario Outline: Required input failure
    Given enabled analysis with "<defect>" required inputs
    And no severity override for the input-failure rule
    When analysis runs
    Then an error diagnostic is reported
    And analysis is unavailable or incomplete rather than clean

    Examples:
      | defect       |
      | missing      |
      | invalid      |
      | incompatible |

  @B-018
  Scenario Outline: Input change invalidation
    Given an existing analysis verdict
    When "<input>" changes
    Then the affected analysis verdict is invalidated

    Examples:
      | input         |
      | source        |
      | specification |
      | configuration |

  @B-019
  Scenario: Private package installation
    Given a consumer installing Specht.CodeAnalysis through a private PackageReference
    When the package is consumed
    Then analyzers, code fixes and build integration are available
    And no application runtime dependency is introduced

  @B-020
  Scenario: Aligned package release version
    Given a repository release
    When analysis and tool packages are produced
    Then both use the repository release version

  @B-021
  Scenario: Contract-based compatibility
    Given different installed package versions with compatible schema and evidence contracts
    When interoperability is evaluated
    Then exact package-version equality is not required

  @B-022
  Scenario: Supplied analysis policy
    Given existing consumer configuration, its schemas and explicit analysis mappings
    When analysis policy is read
    Then selected scopes, required links, exemptions and record discovery use that policy
    And existing consumer ID grammars remain authoritative

  @B-023
  Scenario: Configuration responsibility
    Given MSBuild root/input configuration and standard Roslyn reporting configuration
    When analysis is configured
    Then MSBuild determines the repository root and supplied inputs
    And severity and suppression use standard Roslyn controls

  @B-024 @boundary
  Scenario Outline: Consumer schema ownership
    Given a consumer supplying "<schema>" at its own version "<version>"
    When analysis consumes the supplied configuration and records
    Then the supplied consumer schema and version are used
    And no Specht schema adoption, schema upgrade or pin change is required

    Examples:
      | schema                | version |
      | an older Specht schema | 0.1.0  |
      | a known Specht schema  | 0.2.0  |
      | a consumer-owned schema | 0.1.0 |
      | a consumer-owned schema | 7.4.2 |
