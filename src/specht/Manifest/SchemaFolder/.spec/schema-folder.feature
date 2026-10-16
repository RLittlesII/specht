Feature: The schema folder
  As the maintainer of a repository that keeps its schemas where its tree wants them
  I want the manifest to name the folder the frontmatter schemas are read from and written to
  So that the tool reads and writes them where the manifest says, and a manifest that names no folder changes nothing

  Background:
    Given a repository root holding a manifest pinned to the schema version that first knows a schema folder
    And the manifest names the Feature and item frontmatter schema files by their default names

  @B-001
  Scenario: The manifest names the frontmatter schema folder
    Given the manifest names the schema folder "schemas/frontmatter"
    When the frontmatter schema folder is read from the manifest
    Then it is "schemas/frontmatter" under the root

  @B-002
  Scenario: A manifest naming no folder reads the default folder
    Given the manifest names no schema folder
    When the frontmatter schema folder is read from the manifest
    Then it is the default folder, where the tool reads today

  @B-002
  Scenario: The default manifest reproduces the baseline verdicts
    Given the default manifest and the synthetic baseline tree
    When the check runs
    Then every violation equals the golden report's, field for field, in order

  @B-003
  Scenario: The shipped manifest names no folder
    When the shipped manifest of the version that first knows a schema folder is read
    Then it names no schema folder

  @B-004
  Scenario: The on-disk source reads the named folder
    Given the manifest names the schema folder "schemas/frontmatter"
    And that folder holds a Feature frontmatter schema edited to require a key the embedded one does not
    And the default folder holds the unedited Feature frontmatter schema
    And the manifest records no upstream schema source
    When the check runs with the on-disk source selected
    Then a Feature specification lacking that key is reported
    And nothing under the default folder is read

  @B-005
  Scenario: An upstream copy is read and verified in the named folder
    Given the manifest names the schema folder "schemas/frontmatter"
    And the manifest records an upstream schema source with its version and content hashes
    And that folder holds the copy of that source and its content matches the recorded hashes
    When the check runs
    Then frontmatter is validated with the schemas read from that folder

  @B-005
  Scenario: An upstream copy missing from the named folder stops the check
    Given the manifest names the schema folder "schemas/frontmatter"
    And the manifest records an upstream schema source with its version and content hashes
    And the Feature frontmatter schema is absent from that folder and present under the default folder
    When the check runs
    Then the exit code is 3

  @B-006
  Scenario: The embedded source does not read the named folder
    Given the manifest names the schema folder "schemas/frontmatter"
    And that folder holds a Feature frontmatter schema edited to require a key the embedded one does not
    And the manifest records no upstream schema source and selects no source
    When the check runs
    Then the verdict equals the one the same root gives when the manifest names no schema folder

  @B-007
  Scenario: Init writes the frontmatter schemas into the named folder
    Given the manifest names the schema folder "schemas/frontmatter"
    And that folder does not exist
    When init runs against the root with schemas
    Then the Feature and item frontmatter schemas are written under "schemas/frontmatter"
    And nothing is written under the default folder

  @B-007
  Scenario: Init leaves a schema present in the named folder as it was
    Given the manifest names the schema folder "schemas/frontmatter"
    And that folder holds a Feature frontmatter schema of its own
    When init runs against the root with schemas
    Then that file is byte for byte as it was
    And the item frontmatter schema is written beside it

  @B-008
  Scenario: Init into a bare root writes the default layout
    Given a root directory with no configuration folder
    When init runs against it with schemas
    Then the manifest written names no schema folder
    And the frontmatter schemas are written under the default folder

  @B-009
  Scenario: Upgrade rewrites the frontmatter schemas in the named folder
    Given the manifest names the schema folder "schemas/frontmatter"
    And that folder holds the frontmatter schemas of an older shipped version
    And the manifest's pin has been moved to the newest shipped version
    When upgrade runs against the root
    Then the frontmatter schemas under "schemas/frontmatter" are the pinned version's
    And nothing under the default folder is created, modified or deleted

  @B-010
  Scenario: An absolute folder is rejected
    Given the manifest names an absolute path as the schema folder
    When the check runs
    Then the manifest is rejected before any rule runs
    And the exit code is 3
    And the message names the schema folder key and does not repeat its value

  @B-011
  Scenario Outline: A folder that climbs out of the root or uses a backslash is rejected
    Given the manifest names the schema folder "<value>"
    When the check runs
    Then the manifest is rejected before any rule runs
    And the exit code is 3
    And the message names the schema folder key

    Examples:
      | value                     |
      | ../shared/schemas         |
      | schemas/../../elsewhere   |
      | schemas\\frontmatter      |

  @B-012
  Scenario Outline: An empty folder or a folder naming the root itself is rejected
    Given the manifest names the schema folder "<value>"
    When the check runs
    Then the manifest is rejected before any rule runs
    And the exit code is 3
    And the message names the schema folder key

    Examples:
      | value     |
      |           |
      | .         |
      | ./schemas |

  @B-013
  Scenario: An older pin rejects the folder key
    Given the manifest pins the shipped schema version before the one that first knows a schema folder
    And the manifest names the schema folder "schemas/frontmatter"
    When the check runs
    Then the manifest is rejected as naming a key that version does not know
    And the exit code is 3

  @B-014
  Scenario Outline: A missing or invalid schema is named under the named folder
    Given the manifest names the schema folder "schemas/frontmatter"
    And the manifest records no upstream schema source
    And the Feature frontmatter schema in that folder is <state>
    When the check runs with the on-disk source selected
    Then the exit code is 3
    And the error names "schemas/frontmatter/feature.schema.json" relative to the root
    And nothing is written on the standard output

    Examples:
      | state                       |
      | absent                      |
      | not valid JSON              |
      | not a compilable schema     |

  @B-015
  Scenario: Without epics no epic schema is needed in the named folder
    Given the manifest names the schema folder "schemas/frontmatter"
    And the manifest declares no epic grammar
    And that folder holds the Feature and item frontmatter schemas and no epic frontmatter schema
    When the check runs with the on-disk source selected
    Then the root is not rejected

  @B-016
  Scenario: Schema file names stay plain file names
    Given the manifest names the schema folder "schemas/frontmatter"
    And the manifest names the Feature frontmatter schema file "custom/feature.schema.json"
    When the check runs
    Then the manifest is rejected naming the Feature kind and that value
    And the exit code is 3

  @B-016
  Scenario: A plain file name composes with the named folder
    Given the manifest names the schema folder "schemas/frontmatter"
    And the manifest names the Feature frontmatter schema file "feature.custom.json"
    And that folder holds "feature.custom.json" and not the default name
    When the check runs with the on-disk source selected
    Then the Feature frontmatter schema read is "schemas/frontmatter/feature.custom.json"

  @B-017 @boundary
  Scenario: The named folder changes nothing discovery finds
    Given the manifest names the schema folder "schemas/frontmatter"
    And a specification sits under a folder named "schemas"
    When discovery runs
    Then it finds the same specifications, items, epics and companion files the same root gives when the manifest names no schema folder

  @B-018 @boundary
  Scenario: The retired location is a folder like any other
    Given the manifest names the schema folder that was the retired location
    And that folder holds the Feature and item frontmatter schemas
    And the manifest records no upstream schema source
    When the check runs with the on-disk source selected
    Then frontmatter is validated with the schemas read from that folder

  @B-018 @boundary
  Scenario: A manifest left at the retired location is still not read
    Given the manifest names the schema folder that was the retired location
    And a second manifest sits at the retired manifest path
    When the check runs
    Then the standard output, the standard error and the exit code are the ones the same root gives without that second manifest

  @B-019
  Scenario: The folder key is a consumer setting
    Given this repository at one commit
    When the embedded manifest of the newest version is compared with the repository's live manifest on the tool-owned keys
    Then the schema folder key is not among the keys compared
    And neither manifest names a schema folder

  @B-020 @boundary
  Scenario: The manifest stays in the configuration folder
    Given the manifest names the schema folder "schemas/frontmatter"
    And a second manifest sits under "schemas/frontmatter"
    When the check runs
    Then the manifest read is the one in the configuration folder
    And the second manifest is not read

  @B-021 @boundary
  Scenario: The templates stay in the configuration folder
    Given the manifest names the schema folder "schemas/frontmatter"
    When init runs against the root with templates
    Then the four templates are written under the configuration folder's templates folder
    And nothing is written under "schemas/frontmatter"
