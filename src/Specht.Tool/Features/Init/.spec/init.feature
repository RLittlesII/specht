Feature: init
  As the maintainer of a repository adopting the specification model
  I want one command that writes the schema set and the templates into my repository
  So that I start from the same bytes every other repository starts from, and keep what I already have

  @B-001
  Scenario: Init writes the defaults into a bare repository
    Given a root directory with no schema folder and no templates folder
    When init runs against it
    Then the schema folder holds the manifest and the three frontmatter schemas
    And the templates folder holds the four templates
    And the exit code is 0

  @B-002 @boundary
  Scenario: Init never overwrites an existing file
    Given a root directory whose schema folder holds a manifest with local edits
    And no other schema or template file beside it
    When init runs against it
    Then the manifest is byte-for-byte as it was
    And the standard output reports the manifest as skipped
    And the seven other files are written
    And the exit code is 0

  @B-003
  Scenario: A written file equals the embedded copy
    Given a root directory with no schema folder and no templates folder
    When init runs against it
    Then each written file is byte-identical to the tool's embedded copy

  @B-004
  Scenario: The embedded copies equal this repository's live copy
    Given the tool is built from this repository at one commit
    When its embedded copies of the newest version are compared with the files under this repository's schema and templates folders
    Then each embedded copy is byte-identical to its file

  @B-005
  Scenario: A written schema's id names this repository and the version
    Given a root directory with no schema folder
    When init runs against it
    Then each frontmatter schema written carries an id under this repository's schema address for the version written

  @B-006
  Scenario: Init lists what it wrote and what it skipped
    Given a root directory whose templates folder already holds the feature template
    When init runs against it
    Then the standard output lists every one of the eight files relative to the root
    And marks the feature template as skipped and the rest as written

  @B-007
  Scenario: A root that is not a directory is a missing-input failure
    Given a root path that does not exist
    When init runs against it
    Then the standard error names that path
    And nothing is written
    And the exit code is 2

  @B-008 @boundary
  Scenario: Init writes nowhere else
    Given a root directory with no schema folder and no templates folder
    And a snapshot of every file under the root
    When init runs against it
    Then the only files created are under the schema folder and the templates folder
    And no other file was modified
