@0201-F2
Feature: CSharp binding authoring

  @B-001
  Scenario: Explicit evidence
    Given a supported declaration carrying a complete explicit Feature and claim binding
    When the declared analysis is requested
    Then checked traceability uses that declaration identity regardless of its containing type default

  @B-002 @boundary
  Scenario: Inherited binding
    Given a member carrying only a claim under a type default
    When the declared analysis is requested
    Then analysis reports that the member lacks explicit identity and does not count it as checked evidence

  @B-003
  Scenario: Materialize default
    Given a claim-only member with one unambiguous type default and an invoked repair
    When the declared analysis is requested
    Then the repair persists its complete target identity while preserving other XML documentation

  @B-004
  Scenario: Unsafe repair
    Given missing, conflicting or ambiguous default targets
    When the declared analysis is requested
    Then no repair silently selects a target

  @B-005
  Scenario: Invalid binding
    Given a malformed, unresolved or ambiguous reference on a supported declaration
    When the declared analysis is requested
    Then analysis reports a diagnostic at that declaration with its target reference

  @B-006
  Scenario Outline: Multiple targets
    Given a supported declaration with two repeated "<role>" elements
    And each element carries one complete distinct target identity
    When the declared analysis is requested
    Then both declared relationships are retained independently

    Examples:
      | role       |
      | verifies   |
      | implements |
      | follows    |

  @B-007
  Scenario: Missing member binding
    Given a selected test method with no binding documentation
    When the declared analysis is requested
    Then analysis reports the missing binding and an invoked fix offers existing claim choices without inventing the target

  @B-008
  Scenario: Type authoring defaults
    Given a type with an authoring-default binding
    When its binding role is interpreted
    Then the type defaults supply authoring context only

  @B-009
  Scenario: Test verification role
    Given a supported declaration with a test verification binding
    When its binding role is interpreted
    Then the declared relationship connects the test to its claim

  @B-010
  Scenario: Implementation role
    Given a supported declaration with an implementation binding
    When its binding role is interpreted
    Then the declared relationship connects implementation code to its claim

  @B-011
  Scenario: ADR relationship role
    Given a supported declaration with an architectural-decision binding
    When its binding role is interpreted
    Then the declared relationship connects the code to its ADR

  @B-012
  Scenario Outline: Supported declaration scope
    Given a "<role>" binding on a "<declaration>"
    When its declaration scope is checked
    Then that placement is supported

    Examples:
      | role       | declaration                       |
      | spec       | XML-documentable type             |
      | verifies   | recognized test method            |
      | implements | XML-documentable type             |
      | implements | XML-documentable member           |
      | follows    | XML-documentable type             |
      | follows    | XML-documentable member           |

  @B-013 @boundary
  Scenario Outline: Type binding isolation
    Given a type with an explicit "<role>" binding
    And a member of that type with no explicit binding
    When checked traceability is requested
    Then that binding describes only the type itself
    And the member has no implicit binding

    Examples:
      | role       |
      | implements |
      | follows    |

  @B-014
  Scenario Outline: Nearest nested default
    Given an outer type with an authoring default
    And a nested type with "<nested default>"
    And a member inside the nested type needing authoring context
    When a binding repair determines its default context
    Then the context comes from the "<selected type>"

    Examples:
      | nested default       | selected type |
      | no default           | outer type    |
      | its own default      | nested type   |

  @B-015 @boundary
  Scenario: Whole nested default
    Given an outer type with an epic-qualified authoring default
    And a nested type with its own default that omits the optional epic
    When a binding repair determines the nested member's default context
    Then the context contains only the nested type's default components
    And no epic component is inherited from the outer type

  @B-016
  Scenario Outline: Shared partial default
    Given a partial type with "<defaults>" across its parts
    And a member in a part that declares no default
    When a binding repair determines its default context
    Then the member has the same shared type default

    Examples:
      | defaults                    |
      | one default                 |
      | identical repeated defaults |

  @B-017 @boundary
  Scenario: Conflicting partial defaults
    Given a partial type with different authoring defaults on two parts
    When its authoring defaults are analyzed
    Then analysis reports the conflicting defaults
    And no repair automatically materializes a target from those defaults

  @B-018 @boundary
  Scenario: No base type default
    Given a base type with an authoring default
    And a derived type with no default and no enclosing type default
    When a binding repair determines the derived member's default context
    Then no default context is inherited from the base type

  @B-019 @boundary
  Scenario Outline: Scalar identity values
    Given a relationship element with "<value>" in an identity attribute
    When that attribute is interpreted under the consumer grammar
    Then it is treated as one identifier without splitting it into targets

    Examples:
      | value                     |
      | a delimiter-bearing ID    |
      | two IDs written as a list |

  @B-020
  Scenario Outline: Invalid reserved XML
    Given a reserved XML element with "<defect>"
    When binding documentation is analyzed
    Then analysis reports a validation diagnostic

    Examples:
      | defect                      |
      | a missing required attribute |
      | an unknown attribute         |
      | body text                    |
      | a nested element             |
      | malformed XML                |
      | an unsupported placement     |

  @B-021 @boundary
  Scenario Outline: Invalid relationship exclusion
    Given an invalid "<role>" element
    When checked traceability is requested
    Then that relationship supplies no checked evidence

    Examples:
      | role       |
      | verifies   |
      | implements |
      | follows    |

  @B-022 @boundary
  Scenario: Invalid default exclusion
    Given a type with an invalid authoring-default element
    When a binding repair determines its default context
    Then that invalid element supplies no authoring context

  @B-023
  Scenario Outline: Unrelated documentation preservation
    Given a declaration with unrelated XML documentation including another custom tag
    When "<operation>" is requested
    Then the unrelated documentation is preserved

    Examples:
      | operation               |
      | binding analysis        |
      | an invoked binding repair |

  @B-024
  Scenario Outline: Empty reserved elements
    Given an empty "<role>" element with its defined attributes on a supported declaration
    When the XML shape of the binding is validated
    Then that XML shape is accepted
    And target resolution and lifecycle eligibility remain separate checks

    Examples:
      | role       |
      | spec       |
      | verifies   |
      | implements |
      | follows    |

  @B-025
  Scenario Outline: Duplicate relationship identity
    Given valid relationship elements with the same code symbol, role and resolved target
    And those elements appear in "<location>"
    When relationships are collected
    Then the elements represent one relationship

    Examples:
      | location                       |
      | one declaration's documentation |
      | different parts of a partial type |

  @B-026
  Scenario Outline: Duplicate relationship diagnostic
    Given duplicate valid "<role>" elements on one code symbol
    When binding documentation is analyzed
    Then analysis reports a duplicate diagnostic

    Examples:
      | role       |
      | verifies   |
      | implements |
      | follows    |

  @B-027
  Scenario: Remove redundant relationship copies
    Given duplicate valid relationship elements on one code symbol
    When the duplicate-removal fix is invoked
    Then redundant copies are removed while one complete binding is preserved

  @B-028 @boundary
  Scenario: Distinct relationship roles
    Given one recognized test method with valid verification and implementation relationships to the same claim
    When relationships are collected
    Then the two roles remain distinct relationships

  @B-029 @boundary
  Scenario: Distinct code symbols
    Given two different test methods each with a valid verification relationship to the same claim
    When relationships are collected
    Then each test method retains its own verification relationship

  @B-030
  Scenario Outline: Explicit repair target selection
    Given a binding repair that needs a target choice or replacement
    And "<selection>"
    When the repair is invoked
    Then "<outcome>"

    Examples:
      | selection                       | outcome                                      |
      | an existing record is selected  | the binding uses the selected existing record |
      | no existing record is selected  | no target edit is made                       |

  @B-031 @boundary
  Scenario Outline: No guessed corrections
    Given "<defect>" in a binding element
    When available repairs are requested
    Then no replacement is guessed from spelling or similarity

    Examples:
      | defect               |
      | a misspelled ID      |
      | a misspelled attribute |

  @B-032 @boundary
  Scenario Outline: No destructive validation cleanup
    Given a reserved element with "<material>"
    When available repairs are requested
    Then no repair deletes that material merely to make the element valid

    Examples:
      | material           |
      | unknown attributes |
      | body content       |

  @B-033
  Scenario Outline: Deterministic Fix All
    Given multiple bindings needing "<repair>" with no target choice
    When Fix All is invoked for that supported repair
    Then the deterministic repairs are applied

    Examples:
      | repair                                |
      | unambiguous default materialization    |
      | removal of valid relationship duplicates |

  @B-034 @boundary
  Scenario: No bulk target propagation
    Given a target selected for one declaration's binding
    And unrelated declarations needing their own target choices
    When bulk repairs are requested
    Then that selected target is not propagated to those declarations

  @B-035
  Scenario: Analyzer diagnostic namespace
    Given an analyzer finding
    When its diagnostic is reported
    Then its ID has the SPCT prefix followed by four decimal digits

  @B-036
  Scenario: Stable published diagnostic IDs
    Given a published diagnostic and a later analyzer release
    When the same diagnostic is reported in that later release
    Then its published diagnostic ID is retained

  @B-037 @boundary
  Scenario: Retired diagnostic IDs
    Given a diagnostic ID retired by a previous analyzer release
    When another diagnostic is introduced
    Then the retired ID is not assigned to it

  @B-038 @boundary
  Scenario: Checker vocabulary isolation
    Given a repository pinned to a checker schema version
    When an analyzer release adds an analyzer-only rule
    Then the pinned checker's SPEC rule vocabulary remains unchanged
