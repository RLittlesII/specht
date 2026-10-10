Feature: init
  As the maintainer of a repository adopting the specification model
  I want one command that writes the schema set and the templates into my repository
  So that I start from the same bytes every other repository starts from, and keep what I already have

  @B-018
  Scenario: Init with epics writes the eight files into a bare repository
    Given a root directory with no schema folder and no templates folder
    When init runs against it with epics
    Then the schema folder holds the manifest and the three frontmatter schemas
    And the templates folder holds the four templates

  @B-019
  Scenario: Init without epics writes no epic schema
    Given a root directory with no schema folder and no templates folder
    When init runs against it
    Then the schema folder holds the manifest and the Feature and item frontmatter schemas
    And the schema folder holds no epic frontmatter schema
    And the templates folder holds the four templates

  @B-020
  Scenario Outline: Init into a bare repository succeeds
    Given a root directory with no schema folder and no templates folder
    When init runs against it <with or without> epics
    Then the exit code is 0

    Examples:
      | with or without |
      | with            |
      | without         |

  @B-021
  Scenario: Init without epics writes a manifest with no epic grammar
    Given a root directory with no schema folder and no templates folder
    When init runs against it
    Then the manifest written declares no epic grammar

  @B-022
  Scenario: Init without epics writes a task grammar with no epic part
    Given a root directory with no schema folder and no templates folder
    When init runs against it
    Then the manifest written has a task grammar with no epic part

  @B-023 @boundary
  Scenario Outline: Init writes no epic schema beside a manifest declaring no epic grammar
    Given a root directory whose schema folder holds a manifest pinning the newest version the tool ships and declaring no epic grammar
    And no other schema or template file beside it
    When init runs against it <with or without> epics
    Then the schema folder holds no epic frontmatter schema

    Examples:
      | with or without |
      | with            |
      | without         |

  @B-002 @boundary
  Scenario: Init with epics never adds the epic grammar to an existing manifest
    Given a root directory whose schema folder holds a manifest declaring no epic grammar
    When init runs against it with epics
    Then the manifest is byte-for-byte as it was

  @B-002 @boundary
  Scenario: Init never overwrites an existing file
    Given a root directory whose schema folder holds a manifest with local edits
    And no other schema or template file beside it
    When init runs against it
    Then the manifest is byte-for-byte as it was

  @B-003
  Scenario: A written file equals the embedded copy
    Given a root directory with no schema folder and no templates folder
    And no upstream schema source is recorded
    When init runs against it
    Then each written file other than the manifest is byte-identical to the tool's embedded copy

  @B-004
  Scenario: The embedded copies equal this repository's live copy on what the tool owns
    Given the tool is built from this repository at one commit
    And this repository's manifest lowers one rule's severity
    When its embedded copies of the newest version are compared with the files under this repository's schema and templates folders
    Then each embedded frontmatter schema and template is byte-identical to its file
    And the embedded manifest equals the live manifest on every tool-owned key

  @B-005
  Scenario: A written schema's id names this repository and the version
    Given a root directory with no schema folder
    And no upstream schema source is recorded
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
    Then the standard error names that path exactly as it was given
    And nothing is written
    And the exit code is 2

  @B-008 @boundary
  Scenario: Init writes nowhere else
    Given a root directory with no schema folder and no templates folder
    And a snapshot of every file under the root
    When init runs against it
    Then the only files created are under the schema folder and the templates folder
    And no other file was modified

  @B-009
  Scenario: Init defaults the root to the working directory
    Given a working directory with no schema folder and no templates folder
    When init runs there without naming a root
    Then the schema folder and the templates folder are written under the working directory

  @B-010
  Scenario: Init writes what is missing beside an existing file
    Given a root directory whose schema folder holds a manifest pinning the newest version the tool ships and declaring the epic grammar
    And no other schema or template file beside it
    When init runs against it
    Then the seven other files are written

  @B-011
  Scenario: Init succeeds when some files already exist
    Given a root directory whose schema folder holds a manifest pinning the newest version the tool ships and declaring the epic grammar
    And no other schema or template file beside it
    When init runs against it
    Then the exit code is 0

  @B-012
  Scenario: Init fills a partial tree at its pinned older version
    Given the tool ships schema versions 1 and 2
    And a root directory whose schema folder holds a manifest pinning version 1
    And no other schema or template file beside it
    When init runs against it
    Then the seven other files are written at version 1
    And the exit code is 0

  @B-013 @boundary
  Scenario: Init refuses a version the tool does not ship
    Given the tool ships schema version 1 only
    And a root directory whose schema folder holds a manifest pinning version 7
    And a snapshot of every file under the root
    When init runs against it
    Then no file under the root has changed
    And the standard error names version 7 and the versions the tool ships
    And the exit code is 3

  @B-014
  Scenario: Init writes a fetched upstream schema that matches its hash
    Given a root whose manifest records an upstream schema source with its version and content hash
    And one frontmatter schema file is present under the root's schema folder and the others are absent
    And the source serves content matching that hash
    When init runs against it
    Then the fetched content is written to each absent schema file
    And the present schema file is unchanged

  @B-015
  Scenario: Init refuses a fetched upstream schema that does not match its hash
    Given a root whose manifest records an upstream schema source with its version and content hash
    And the source serves content that does not match that hash
    And a snapshot of every file under the root
    When init runs against it
    Then no file under the root has changed
    And the standard error names the source
    And the exit code is 3

  @B-016
  Scenario: Init refuses an upstream source it cannot reach
    Given a root whose manifest records an upstream schema source with its version and content hash
    And no frontmatter schema file under the root's schema folder
    And the source cannot be reached
    And a snapshot of every file under the root
    When init runs against it
    Then no file under the root has changed
    And the standard error names the source
    And the exit code is 3

  @B-017
  Scenario: Init beside a manifest that does not parse fails as the check does
    Given a root directory whose schema folder holds a manifest that is not well-formed JSON
    And a snapshot of every file under the root
    When init runs against it
    Then no file under the root has changed
    And the standard error names the reason
    And the exit code is 3

  @B-017
  Scenario: Init beside a rejected manifest fails as the check does
    Given a root directory whose schema folder holds a manifest carrying a key the tool does not know
    And a snapshot of every file under the root
    When init runs against it
    Then no file under the root has changed
    And the standard error names the reason
    And the exit code is 3

  @B-017
  Scenario: Init beside a contradictory schema source fails as the check does
    Given a root directory whose manifest records an upstream schema source and explicitly selects the embedded source
    And a snapshot of every file under the root
    When init runs against it
    Then no file under the root has changed
    And the standard error names the contradiction
    And the exit code is 3
