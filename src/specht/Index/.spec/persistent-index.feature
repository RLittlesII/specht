Feature: Persistent specification index
  A local MCP client follows stable specification keys through a rebuildable
  database view of the repository's authoritative Markdown tree.

  @B-001
  Scenario: Build a durable index from Markdown
    Given a repository with specification files and a pinned schema version
    When the specification index is built
    Then its files, facts, relationships, and source locations are available from durable storage

  @B-002
  Scenario: Follow keys to related source locations
    Given an index containing a claim linked to its Feature and coverage row
    When a client follows the claim key
    Then it receives the related keys and root-relative source locations without parsing Markdown

  @B-003
  Scenario: Reuse an unchanged index
    Given a validated index of an unchanged specification tree
    When a client looks up several keys
    Then the lookups do not reparse specification Markdown

  @B-004
  Scenario: Bring a stale index current
    Given an index that predates a direct Markdown edit or branch switch
    When a client asks for an indexed fact
    Then the answer reflects the current authoritative files

  @B-005
  Scenario: Remove stale keys after rebuild
    Given a file or key that existed in an earlier indexed tree
    When the index is rebuilt after the file or key is removed
    Then the removed fact and its relationships no longer resolve

  @B-006
  Scenario: Match the command-line answer
    Given an indexed repository tree
    When MCP and the command line answer the same fact request for that tree
    Then their facts and root-relative source locations agree

  @boundary
  Scenario: The index does not author a specification
    Given a repository whose Markdown has been indexed
    When a client follows a key through the index
    Then no repository document is changed
