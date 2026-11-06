Feature: Documentation site
  As a maintainer evaluating or adopting specht
  I want a published documentation site
  So that I read the commands, the rules and the API surface without cloning the repository

  # DocFX builds the site from 0002-F1's generated API reference and the
  # docs/*.md pages, and CI publishes it to GitHub Pages (decision 0001).
  # B-003, the deferral, is withdrawn and has no scenario.

  @B-001
  Scenario: The site is built from the generated documentation
    Given 0002-F1 has produced an API reference
    And the documentation pages exist
    When the site builds
    Then its content is that reference and those pages
    And no page is written by hand in their place

  @B-002
  Scenario: The site is published to a stable public URL
    Given the site builds in CI
    When the publish pipeline runs
    Then the site is reachable at a public URL that stays the same across releases
