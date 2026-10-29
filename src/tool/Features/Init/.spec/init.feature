Feature: init
  As the maintainer of a repository adopting the specification model
  I want one command that writes the manifest, and the schemas and the templates only when I ask for them
  So that I start from the same bytes every other repository starts from, take no file I did not ask for, and keep what I already have

  @B-029
  Scenario: Init alone writes only the manifest
    Given a root directory with no configuration folder
    When init runs against it
    Then the only file written is the manifest

  @B-030
  Scenario: Init with schemas adds the frontmatter schemas
    Given a root directory with no configuration folder
    When init runs against it with schemas
    Then the files written are exactly the manifest and the Feature and item frontmatter schemas

  @B-031
  Scenario: Init with templates adds the templates
    Given a root directory with no configuration folder
    When init runs against it with templates
    Then the files written are exactly the manifest and the four templates

  @B-032
  Scenario: Init with every part writes what schemas and templates together write
    Given two root directories with no configuration folder
    When init runs against the first with every part
    And init runs against the second with schemas and templates
    Then the two roots hold the same files with the same content

  @B-018
  Scenario: Init with every part and epics writes the manifest, the three schemas and the four templates
    Given a root directory with no configuration folder
    When init runs against it with every part and epics
    Then the manifest is written
    And the frontmatter schema folder holds the Feature, item and epic frontmatter schemas
    And the templates folder holds the four templates

  @B-019
  Scenario: Init without epics writes no epic schema
    Given a root directory with no configuration folder
    When init runs against it with every part
    Then the manifest is written
    And the frontmatter schema folder holds the Feature and item frontmatter schemas
    And the frontmatter schema folder holds no epic frontmatter schema
    And the templates folder holds the four templates

  @B-020
  Scenario Outline: Init into a bare repository succeeds
    Given a root directory with no configuration folder
    When init runs against it with <options>
    Then the exit code is 0

    Examples:
      | options              |
      | no option            |
      | schemas              |
      | templates            |
      | every part           |
      | epics                |
      | every part and epics |

  @B-021
  Scenario: Init without epics writes a manifest with no epic grammar
    Given a root directory with no configuration folder
    When init runs against it
    Then the manifest written declares no epic grammar

  @B-022
  Scenario: Init without epics writes a task grammar with no epic part
    Given a root directory with no configuration folder
    When init runs against it
    Then the manifest written has a task grammar with no epic part

  @B-024
  Scenario: Init writes a level for every rule of the version it pins
    Given a root directory with no configuration folder
    When init runs against it
    Then the manifest written sets a level for every rule of the version it pins

  @B-025
  Scenario: Init writes each rule at its default level
    Given a root directory with no configuration folder
    When init runs against it
    Then each level the manifest written sets is that rule's own default severity

  @B-026 @boundary
  Scenario: Init writes no level for a rule outside the version it pins
    Given a root directory with no configuration folder
    When init runs against it
    Then the manifest written sets a level for no rule outside the version it pins

  @B-023 @boundary
  Scenario Outline: Init writes no epic schema beside a manifest declaring no epic grammar
    Given a root directory holding only a manifest pinning the newest version the tool ships and declaring no epic grammar
    When init runs against it with <options>
    Then the frontmatter schema folder holds no epic frontmatter schema

    Examples:
      | options           |
      | schemas           |
      | schemas and epics |

  @B-002 @boundary
  Scenario: Init with epics never adds the epic grammar to an existing manifest
    Given a root directory holding a manifest declaring no epic grammar
    When init runs against it with epics
    Then the manifest is byte-for-byte as it was

  @B-002 @boundary
  Scenario: Init never overwrites an existing file
    Given a root directory holding only a manifest with local edits
    When init runs against it with every part
    Then the manifest is byte-for-byte as it was

  @B-002 @boundary
  Scenario: Init leaves alone an existing file of a part it was not asked for
    Given a root directory holding only the feature template with local edits
    When init runs against it
    Then the feature template is byte-for-byte as it was

  @B-003
  Scenario: A written file equals the embedded copy
    Given a root directory with no configuration folder
    And no upstream schema source is recorded
    When init runs against it with every part
    Then each written file other than the manifest is byte-identical to the tool's embedded copy

  @B-004
  Scenario: The embedded copies equal this repository's live copy on what the tool owns
    Given the tool is built from this repository at one commit
    And this repository's manifest lowers one rule's severity
    When its embedded copies of the newest version are compared with this repository's live manifest, frontmatter schemas and templates
    Then each embedded frontmatter schema and template is byte-identical to its file
    And the embedded manifest equals the live manifest on every tool-owned key

  @B-005
  Scenario: A written schema's id names this repository and the version
    Given a root directory with no configuration folder
    And no upstream schema source is recorded
    When init runs against it with schemas
    Then each frontmatter schema written carries an id made of this repository's schema address, the version written, the frontmatter folder and its own file name

  @B-006
  Scenario: Init lists what it wrote and what it skipped
    Given a root directory holding only the feature template
    When init runs against it with every part
    Then the standard output lists the manifest, each frontmatter schema and each template relative to the root
    And marks the feature template as skipped and the rest as written

  @B-033 @boundary
  Scenario: Init lists nothing it was not asked to write
    Given a root directory holding only the feature template
    When init runs against it
    Then the standard output lists the manifest and no other file

  @B-007
  Scenario: A root that is not a directory is a missing-input failure
    Given a root path that does not exist
    When init runs against it
    Then the standard error names that path exactly as it was given
    And nothing is written
    And the exit code is 2

  @B-008 @boundary
  Scenario: Init writes nowhere else
    Given a root directory with no configuration folder
    And a snapshot of every file under the root
    When init runs against it with every part
    Then the only files created are the manifest and files under the frontmatter schema folder and the templates folder
    And no other file was modified

  @B-028 @boundary
  Scenario: Init leaves the files at the retired location as they were
    Given a root directory holding a schema set and templates at the retired location and no configuration folder
    And a snapshot of every file under the root
    When init runs against it with every part
    Then no file at the retired location was created, modified or deleted

  @B-009
  Scenario: Init defaults the root to the working directory
    Given a working directory with no configuration folder
    When init runs there without naming a root
    Then the manifest is written under the working directory

  @B-010
  Scenario: Init writes what is missing beside an existing file
    Given a root directory holding only a manifest pinning the newest version the tool ships and declaring the epic grammar
    When init runs against it with every part
    Then the three frontmatter schemas and the four templates are written

  @B-011
  Scenario: Init succeeds when some files already exist
    Given a root directory holding only a manifest pinning the newest version the tool ships and declaring the epic grammar
    When init runs against it with every part
    Then the exit code is 0

  @B-012
  Scenario: Init fills a partial tree at its pinned older version
    Given the tool ships schema versions 0.1.0 and 0.2.0
    And a root directory holding only a manifest pinning version 0.1.0 and declaring the epic grammar
    When init runs against it with every part
    Then the three frontmatter schemas and the four templates are written at version 0.1.0
    And the exit code is 0

  @B-002 @B-012 @boundary
  Scenario: Rerunning init restores deleted schemas at the pin and preserves local edits
    Given an initialized root pinned to version 0.1.0 with local edits to a schema and a template
    And the other frontmatter schemas have been deleted
    When init runs against it with every part
    Then the missing frontmatter schemas are restored at version 0.1.0
    And every file that existed before the rerun is byte-for-byte unchanged
    And the exit code is 0

  @B-013 @boundary
  Scenario: Init refuses a version the tool does not ship
    Given a root directory holding a manifest pinning version 7.0.0
    And a snapshot of every file under the root
    When init runs against it
    Then no file under the root has changed
    And the standard error names version 7.0.0 and the versions the tool ships
    And the exit code is 3

  @B-014
  Scenario: Init writes a fetched upstream schema that matches its hash
    Given a root whose manifest records an upstream schema source with its version and content hash
    And one frontmatter schema file is present under the root and the others are absent
    And the source serves content matching that hash
    When init runs against it with schemas
    Then the fetched content is written to each absent schema file
    And the present schema file is unchanged

  @B-015
  Scenario Outline: Init refuses a fetched upstream schema that does not match its hash
    Given a root whose manifest records an upstream schema source with its version and content hash
    And no frontmatter schema file under the root
    And the source serves content that does not match that hash
    And a snapshot of every file under the root
    When init runs against it with <options>
    Then no file under the root has changed
    And the standard error names the source
    And the exit code is 3

    Examples:
      | options    |
      | schemas    |
      | every part |

  @B-015 @boundary
  Scenario: Init writes nothing when one fetched schema matches and another does not
    Given a root whose manifest records an upstream schema source with its version and content hash
    And no frontmatter schema file under the root
    And the source serves one schema matching its recorded hash and another that does not
    And a snapshot of every file under the root
    When init runs against it with every part
    Then no file under the root has changed
    And the standard error names the source
    And the exit code is 3

  @B-016
  Scenario Outline: Init refuses an upstream source it cannot reach
    Given a root whose manifest records an upstream schema source with its version and content hash
    And no frontmatter schema file under the root
    And the source cannot be reached
    And a snapshot of every file under the root
    When init runs against it with <options>
    Then no file under the root has changed
    And the standard error names the source
    And the exit code is 3

    Examples:
      | options    |
      | schemas    |
      | every part |

  @B-016
  Scenario Outline: Init refuses an upstream source that does not serve the requested schema
    Given a root whose manifest records an upstream schema source with its version and content hash
    And no frontmatter schema file under the root
    And the source responds with status <status>
    And a snapshot of every file under the root
    When init runs against it with every part
    Then no file under the root has changed
    And the standard error names the source
    And the exit code is 3

    Examples:
      | status |
      | 401    |
      | 404    |

  @B-002 @B-014 @boundary
  Scenario: Init keeps existing upstream schemas without fetching
    Given a root whose manifest records an upstream schema source with its version and content hash
    And every selected frontmatter schema file is present under the root
    And the network is unavailable
    And a snapshot of every file under the root
    When init runs against it with schemas
    Then no network call was attempted
    And no file under the root has changed

  @B-034 @boundary
  Scenario Outline: Init without schemas fetches nothing from a recorded source
    Given a root whose manifest records an upstream schema source with its version and content hash
    And no frontmatter schema file under the root
    And the network is unavailable
    When init runs against it with <options>
    Then no network call was attempted

    Examples:
      | options   |
      | no option |
      | templates |

  @B-027 @boundary
  Scenario: Init into a bare root makes no network call
    Given a root directory with no configuration folder
    And the network is unavailable
    When init runs against it with every part
    Then no network call was attempted

  @B-027 @boundary
  Scenario: Init beside a manifest recording no upstream source makes no network call
    Given a root directory whose manifest records no upstream schema source
    And no frontmatter schema file under the root
    And the network is unavailable
    When init runs against it with schemas
    Then no network call was attempted

  @B-017
  Scenario Outline: Init beside a manifest that does not parse fails as the check does
    Given a root directory holding a manifest that is not well-formed JSON
    And a snapshot of every file under the root
    When init runs against it with <options>
    Then no file under the root has changed
    And the standard error names the reason
    And the exit code is 3
    And the rejected init has no standard output

    Examples:
      | options    |
      | no option  |
      | schemas    |
      | templates  |
      | every part |

  @B-017
  Scenario Outline: Init beside a rejected manifest fails as the check does
    Given a root directory holding a manifest carrying a key the tool does not know
    And a snapshot of every file under the root
    When init runs against it with <options>
    Then no file under the root has changed
    And the standard error names the reason
    And the exit code is 3
    And the rejected init has no standard output

    Examples:
      | options    |
      | no option  |
      | schemas    |
      | templates  |
      | every part |

  @B-017
  Scenario Outline: Init beside a contradictory schema source fails as the check does
    Given a root directory whose manifest records an upstream schema source and explicitly selects the embedded source
    And a snapshot of every file under the root
    When init runs against it with <options>
    Then no file under the root has changed
    And the standard error names the contradiction
    And the exit code is 3
    And the rejected init has no standard output

    Examples:
      | options    |
      | no option  |
      | schemas    |
      | templates  |
      | every part |

  @B-035 @B-040
  Scenario Outline: Init records a new source while fetching remains opt-in
    Given a root directory with no configuration folder
    And new upstream source options are "valid"
    And the network is unavailable
    When init runs against it with <options>
    Then the manifest records the supplied upstream source
    And no network call was attempted

    Examples:
      | options   |
      | no option |
      | templates |

  @B-036 @boundary
  Scenario: Init from a new source preserves a schema already present
    Given a root directory with no configuration folder
    And new upstream source options are "valid"
    And one frontmatter schema file is present under the root and the others are absent
    When init runs against it with schemas
    Then the manifest records the supplied upstream source
    And the missing new-source schemas contain the fetched bytes
    And the present schema file is unchanged
    And the present schema was not fetched

  @B-037 @boundary
  Scenario Outline: Init from a new source writes nothing after a failed fetch
    Given a root directory with no configuration folder
    And new upstream source options are "valid"
    And the source <failure>
    And a snapshot of every file under the root
    When init runs against it with every part
    Then no file under the root has changed
    And the standard error names the source
    And the exit code is 3

    Examples:
      | failure                                  |
      | cannot be reached                        |
      | serves content that does not match that hash |
      | serves one schema matching its recorded hash and another that does not |

  @B-038 @boundary
  Scenario Outline: Init rejects incomplete or invalid source options before fetching
    Given a root directory with no configuration folder
    And new upstream source options are "<fault>"
    And a snapshot of every file under the root
    When init runs against it with <options>
    Then no file under the root has changed
    And the source options were refused before fetching
    And the exit code is 3

    Examples:
      | fault           | options    |
      | missing source  | every part |
      | missing version | every part |
      | missing hash    | every part |
      | missing hash    | no option  |
      | missing hash    | templates  |
      | invalid source  | every part |
      | invalid version | every part |
      | invalid hash    | every part |
      | malformed pair  | every part |
      | duplicate kind  | every part |

  @B-039 @boundary
  Scenario: Init refuses source options beside an existing manifest
    Given a root directory whose manifest records no upstream schema source
    And new upstream source options are "valid"
    And a snapshot of every file under the root
    When init runs against it with every part
    Then no file under the root has changed
    And the source options were refused before fetching
    And the standard error names the existing manifest
    And the exit code is 3

  @B-041
  Scenario: Init writes a fetched template that matches its hash
    Given a root whose manifest records an upstream schema source whose record holds a content hash for the feature template
    And its manifest names no template as the consumer's
    And no template file under the root
    And the source serves a feature template matching that hash
    When init runs against it with templates
    Then the root's feature template is the one the source served

  @B-042
  Scenario: Init refuses a fetched template that does not match its hash
    Given a root whose manifest records an upstream schema source whose record holds a content hash for the feature template
    And no template file under the root
    And the source serves a feature template that does not match that hash
    And a snapshot of every file under the root
    When init runs against it with templates
    Then no file under the root has changed
    And the standard error names the source
    And the exit code is 3

  @B-043
  Scenario Outline: Init refuses a source that will not serve a template
    Given a root whose manifest records an upstream schema source whose record holds a content hash for the feature template
    And no template file under the root
    And the source <refusal>
    And a snapshot of every file under the root
    When init runs against it with templates
    Then no file under the root has changed
    And the standard error names the source
    And the exit code is 3

    Examples:
      | refusal                             |
      | cannot be reached                   |
      | asks for a credential               |
      | does not serve the feature template |

  @B-044 @boundary
  Scenario: Init keeps a present template without fetching it
    Given a root whose manifest records an upstream schema source whose record holds a content hash for the feature template
    And the feature template is present under the root
    And the network is unavailable
    When init runs against it with templates
    Then no network call was attempted
    And the feature template is unchanged
