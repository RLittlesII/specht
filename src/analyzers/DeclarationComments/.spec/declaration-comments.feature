@0201-F8
Feature: Declaration comments

  @B-001
  Scenario Outline: Class surface comments
    Given a "<accessibility>" class declaration with an ordinary line comment on its surface
    When declaration comments are analyzed
    Then the ordinary line comment is reported at its source span

    Examples:
      | accessibility  |
      | public         |
      | internal       |
      | private nested |
      | partial        |

  @B-002
  Scenario Outline: Method surface comments
    Given a method declaration in a "<owner>" with an ordinary line comment on its surface
    When declaration comments are analyzed
    Then the ordinary line comment is reported at its source span

    Examples:
      | owner     |
      | class     |
      | struct    |
      | interface |
      | record    |

  @B-003 @boundary
  Scenario Outline: XML documentation remains allowed
    Given a class and method carrying only "<documentation>" on their surfaces
    When declaration comments are analyzed
    Then no declaration-comment diagnostic is reported

    Examples:
      | documentation               |
      | XML summaries               |
      | a traceability-only XML tag |
      | no comments                 |

  @B-004
  Scenario Outline: Comments throughout the declaration header
    Given a class and method with an ordinary line comment "<position>"
    When declaration comments are analyzed
    Then the ordinary line comments are reported at their source spans

    Examples:
      | position                         |
      | before attributes                |
      | between attributes               |
      | between attributes and modifiers |
      | between modifiers                |
      | within the signature             |
      | before the body delimiter        |

  @B-005 @boundary
  Scenario Outline: XML documentation does not hide ordinary comments
    Given a class and method with ordinary line comments "<position>" their XML documentation
    When declaration comments are analyzed
    Then every ordinary surface line comment is reported once

    Examples:
      | position |
      | before   |
      | after    |
      | between  |

  @B-006 @boundary
  Scenario Outline: Other comment locations remain outside the rule
    Given ordinary comments only in "<location>"
    When declaration comments are analyzed
    Then no declaration-comment diagnostic is reported

    Examples:
      | location                            |
      | a method body                       |
      | an expression body                  |
      | after a completed declaration       |
      | a constructor surface               |
      | a property surface                  |
      | a local function surface            |
      | a record declaration surface        |
      | a struct declaration surface        |
      | a suppression directive explanation |

  @B-007 @boundary
  Scenario: Separate file header
    Given a file beginning with an ordinary comment block separated from its first code or documentation by an empty line
    And a class with an ordinary surface comment after that file header
    When declaration comments are analyzed
    Then only the class surface comment is reported

  @B-007 @boundary
  Scenario: A blank line does not exempt a method comment
    Given a method with an ordinary surface comment separated from its signature by an empty line
    When declaration comments are analyzed
    Then the ordinary line comment is reported at its source span

  @B-008 @boundary
  Scenario Outline: Generated source is excluded
    Given generated source identified by "<marker>" carrying ordinary class and method surface comments
    When declaration comments are analyzed
    Then no declaration-comment diagnostic is reported

    Examples:
      | marker                    |
      | a .g.cs file name         |
      | a .generated.cs file name |
      | an auto-generated header  |

  @B-009 @boundary
  Scenario: No specification inputs required
    Given source with an ordinary class surface comment and no manifest or specification inputs
    When declaration comments are analyzed
    Then the ordinary line comment is reported at its source span

  @B-010
  Scenario: Default warning
    Given source with an ordinary method surface comment and no reporting override
    When declaration comments are analyzed
    Then its declaration-comment diagnostic is an enabled warning

  @B-011 @boundary
  Scenario Outline: Standard reporting controls
    Given source with an ordinary method surface comment
    And "<control>" applied to the declaration-comment diagnostic
    When declaration comments are analyzed
    Then the declaration-comment diagnostic follows that reporting control

    Examples:
      | control                      |
      | an error severity override   |
      | a disabled severity override |
      | a warning suppression pragma |
