Feature: Documentation site
  As a maintainer evaluating or adopting specht
  I want a published documentation site
  So that I read the commands, the rules and the API surface without cloning the repository

  # Deferred by the owner: no work starts until specht itself is built and
  # 0002-F1 has shipped. The scenarios record the intended shape only.
  # The framework is undecided (OQ-2); astro.build is a candidate.

  @B-001
  Scenario: The site is built from the generated documentation
    Given 0002-F1 has produced an API reference and the commands' usage docs
    When the site builds
    Then its content is that reference and those usage docs
    And no page is written by hand in their place

  @B-002
  Scenario: The site is published to a stable public URL
    Given the site builds in CI
    When the publish pipeline runs
    Then the site is reachable at a public URL that stays the same across releases

  @B-003 @boundary
  Scenario: Nothing starts before the deferral is lifted
    Given 0002-F1 has not shipped or the owner has not lifted the deferral
    Then the repository holds no site scaffold, no publish pipeline and no site framework dependency
