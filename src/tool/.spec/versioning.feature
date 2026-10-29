Feature: Schema versioning
  As the maintainer of a repository on the specification model
  I want to pin one schema version, see from its number whether a move can fail my build, and move the pin when I choose
  So that lagging is deliberate, moving is one command each way, and only I change my documents

  Background:
    Given a repository root holding a manifest and the three frontmatter schemas

  @B-001
  Scenario: The pinned version is the one checked with
    Given the tool ships schema versions 0.1.0 and 1.0.0
    And the manifest pins version 0.1.0 and records no upstream schema source
    And the root holds a specification whose frontmatter version 0.1.0 accepts and version 1.0.0 rejects
    When the check runs
    Then no frontmatter violation is reported

  @B-002
  Scenario: A manifest without a version is version 0.1.0
    Given the manifest carries no schema version
    When the check runs with JSON output
    Then the document names schema version 0.1.0

  @B-003
  Scenario: A version the tool does not ship is invalid configuration
    Given the tool ships schema version 0.1.0
    And the manifest pins version 7.0.0
    When the check runs
    Then the standard error names version 7.0.0 and the versions the tool ships
    And the standard output is empty
    And the exit code is 3

  @B-004 @boundary
  Scenario: Every version published from 1.0.0 on is still shipped
    Given the schema versions 1.0.0 and higher published in packages so far
    When a later tool's embedded versions are enumerated
    Then every one of those versions is present

  @B-005
  Scenario Outline: Upgrade brings the schema set and templates to the pinned version
    Given the tool ships schema versions 0.1.0 and 0.2.0
    And the root's frontmatter schemas and templates are version <files>'s
    And the root is pinned to version <pin> with the embedded source and no upstream schema source
    And its manifest names no template as the consumer's
    And one of its templates has been edited by hand
    When upgrade runs against it
    Then the frontmatter schemas and the templates are version <pin>'s
    And the exit code is 0

    Examples:
      | files | pin   |
      | 0.1.0 | 0.2.0 |
      | 0.2.0 | 0.1.0 |

  @B-006
  Scenario: Upgrade on a root already at its pin changes nothing
    Given the tool ships schema versions 0.1.0 and 0.2.0
    And the root is pinned to version 0.1.0 and records no upstream schema source
    And its frontmatter schemas, templates and manifest keys are already version 0.1.0's
    When upgrade runs against it
    Then the standard output says the repository is current
    And no file was rewritten
    And the exit code is 0

  @B-006
  Scenario: A root that differs only in a template the consumer owns is current
    Given the tool ships schema versions 0.1.0 and 0.2.0
    And the root is pinned to version 0.1.0 and records no upstream schema source
    And its frontmatter schemas, templates and manifest keys are already version 0.1.0's
    And its manifest names the feature template as the consumer's
    And the consumer has rewritten the feature template
    When upgrade runs against it
    Then the standard output says the repository is current
    And no file was rewritten
    And the exit code is 0

  @B-007 @boundary
  Scenario: Upgrade never touches a document
    Given the tool ships schema versions 0.1.0 and 0.2.0
    And the root is pinned to version 0.2.0 and its frontmatter schemas and templates are version 0.1.0's
    And the root holds specifications, records and companions
    And a snapshot of every file under the root
    When upgrade runs against it
    Then the only files changed are the manifest and files under the frontmatter schema folder and the templates folder

  @B-009
  Scenario: The on-disk source is selected by configuration
    Given the root's on-disk Feature schema has been edited to require a key the embedded one does not
    And the manifest records no upstream schema source
    And the root holds a specification without that key
    When the check runs with the on-disk source selected
    Then the specification is reported for the missing key

  @B-010 @boundary
  Scenario: The embedded source ignores an on-disk edit
    Given the root's on-disk Feature schema has been edited to require a key the embedded one does not
    And the manifest records no upstream schema source
    And the root holds a specification without that key
    When the check runs without selecting a source
    Then no violation is reported for that key

  @B-011
  Scenario: The report names the selected source
    Given the on-disk source is selected
    When the check runs with JSON output
    Then the document says the schemas came from disk

  @B-012
  Scenario: Init pins the newest version
    Given the tool ships schema versions 0.9.0 and 0.10.0
    And a root directory with no manifest
    When init runs against it
    Then the written manifest pins version 0.10.0

  @B-013 @boundary
  Scenario: A shipped version never changes
    Given the embedded schemas, manifest, templates and rule pages for version 0.1.0 as the first published package shipped them
    And a later tool that ships version 0.1.0
    When that tool's embedded files for version 0.1.0 are read
    Then they are byte-identical

  @B-014
  Scenario: The pinned version's rules are the ones evaluated
    Given the tool ships schema versions 0.1.0 and 1.0.0
    And version 1.0.0 holds a rule version 0.1.0 does not
    And the manifest pins version 0.1.0
    And the root holds a specification that breaks only that rule
    When the check runs
    Then no violation is reported

  @B-015 @boundary
  Scenario: A shipped version's rules never change
    Given the rules version 0.1.0 held in the first published package
    And a later tool that ships version 0.1.0
    When that tool's rules for version 0.1.0 are enumerated
    Then they are the same rules

  @B-016
  Scenario: Upgrade keeps the consumer's manifest settings
    Given the tool ships schema versions 0.1.0 and 0.2.0
    And the root is pinned to version 0.2.0 and its frontmatter schemas and templates are version 0.1.0's
    And its manifest lowers one rule to a warning, disables another and renames the claims section
    And its manifest already carries a key version 0.2.0 defines, set to a value of the consumer's
    When upgrade runs against it
    Then the warning, the disabled rule and the renamed section are as the consumer set them
    And the key version 0.2.0 defines keeps the consumer's value
    And every other key version 0.2.0 defines is added

  @B-138 @boundary
  Scenario: Upgrade never opts a repository into epics
    Given the tool ships schema versions 0.1.0 and 0.2.0
    And the root is pinned to version 0.2.0 and its frontmatter schemas and templates are version 0.1.0's
    And its manifest declares no epic grammar
    When upgrade runs against it
    Then the manifest declares no epic grammar

  @B-017
  Scenario: Upgrade prints what it changed
    Given the tool ships schema versions 0.1.0 and 0.2.0
    And the root is pinned to version 0.2.0 and its frontmatter schemas and templates are version 0.1.0's
    When upgrade runs against it
    Then the standard output names version 0.2.0 and each file rewritten
    And the standard output carries no diff of any file

  @B-017
  Scenario: Upgrade that only adds a manifest key names the manifest
    Given the tool ships schema versions 0.1.0 and 0.2.0
    And the root is pinned to version 0.2.0 and its frontmatter schemas and templates are already version 0.2.0's
    And its manifest lacks a key version 0.2.0 defines
    When upgrade runs against it
    Then the standard output names version 0.2.0 and the manifest

  @B-018
  Scenario: Upgrade under a missing root is a missing-input failure
    When upgrade runs against a path that is not a directory
    Then nothing is written
    And the exit code is 2

  @B-019
  Scenario: Upgrade without a manifest is a missing-input failure
    Given the manifest has been removed from the root
    When upgrade runs against it
    Then nothing is written
    And the exit code is 2

  @B-020
  Scenario: Upgrade with an invalid manifest is invalid configuration
    Given the root's manifest is not valid
    When upgrade runs against it
    Then nothing is written
    And the exit code is 3

  @B-020
  Scenario: Upgrade beside a contradictory schema source is invalid configuration
    Given the root's manifest records an upstream schema source and explicitly selects the embedded source
    When upgrade runs against it
    Then nothing is written
    And the exit code is 3

  @B-021 @boundary
  Scenario: Upgrade under a version the tool does not ship rewrites nothing
    Given the tool ships schema version 0.1.0
    And the manifest pins version 7.0.0
    And a snapshot of every file under the root
    When upgrade runs against it
    Then no file under the root has changed
    And the standard error names version 7.0.0 and the versions the tool ships, as the check's does
    And the exit code is 3

  @B-021 @boundary
  Scenario: Upgrade under a malformed schema version rewrites nothing
    Given the manifest's schema version is 1
    And a snapshot of every file under the root
    When upgrade runs against it
    Then no file under the root has changed
    And the standard error names 1, as the check's does
    And the exit code is 3

  @B-022
  Scenario: Version 0.1.0 accepts a title and a description on an epic
    Given the manifest pins version 0.1.0
    And the root holds an epic whose frontmatter carries a title and a description
    When the check runs
    Then no frontmatter violation is reported for the epic

  @B-023
  Scenario Outline: Version 0.1.0 rejects an empty <key> on an epic
    Given the manifest pins version 0.1.0
    And the root holds an epic whose frontmatter carries an empty <key>
    When the check runs
    Then the epic is reported for a frontmatter violation of its <key>

    Examples:
      | key         |
      | title       |
      | description |

  @B-024
  Scenario: The check reads an upstream schema from the local copy
    Given the manifest records an upstream schema source and selects no source
    And the root holds the copy of that source under its schema folder
    When the check runs
    Then the frontmatter is validated against the local copy

  @B-025 @boundary
  Scenario: The check makes no network call
    Given the manifest records an upstream schema source
    And the network is unavailable
    When the check runs
    Then the check completes
    And no network call was attempted

  @B-026
  Scenario: Upgrade writes a fetched schema that matches its hash
    Given the manifest records an upstream schema source with its version and content hash
    And the root's local copy does not match that hash
    And the source serves content matching that hash
    When upgrade runs
    Then the fetched schemas are written under the root's schema folder

  @B-027
  Scenario: Upgrade refuses a fetched schema that does not match its hash
    Given the manifest records an upstream schema source with its version and content hash
    And the root's local copy does not match that hash
    And the source serves content that does not match that hash
    And a snapshot of every file under the root
    When upgrade runs
    Then no file under the root has changed
    And the standard error names the source
    And the exit code is 3

  @B-028
  Scenario: An upstream schema does not change the rule vocabulary
    Given the manifest pins version 0.1.0 and records an upstream schema source
    When the check runs
    Then exactly version 0.1.0's rule ids are evaluated

  @B-029 @boundary
  Scenario: Upgrade never writes the embedded schemas over the on-disk source
    Given the tool ships schema versions 0.1.0 and 0.2.0
    And the root is pinned to version 0.2.0 with the on-disk source selected
    When upgrade runs against it
    Then no frontmatter schema under the root is version 0.2.0's embedded schema

  @B-030
  Scenario: Upgrade says which schemas it skipped and why
    Given the tool ships schema versions 0.1.0 and 0.2.0
    And the root is pinned to version 0.2.0 with the on-disk source selected
    When upgrade runs against it
    Then the output names each frontmatter schema it skipped
    And the reason it skipped them

  @B-031
  Scenario: Upgrade still rewrites the templates under the on-disk source
    Given the tool ships schema versions 0.1.0 and 0.2.0
    And the root is pinned to version 0.2.0 with the on-disk source selected
    And the root's templates are version 0.1.0's
    And its manifest names no template as the consumer's
    When upgrade runs against it
    Then the templates are version 0.2.0's
    And the output names each template it rewrote

  @B-032
  Scenario: Recording an upstream source selects the on-disk source
    Given the manifest records an upstream schema source and selects no source
    When the check runs with JSON output
    Then the document says the schemas came from the upstream copy

  @B-033 @boundary
  Scenario: Upgrade does not fetch a local copy that matches its hash
    Given the manifest records an upstream schema source with its version and content hash
    And the root's local copy matches that hash
    And the network is unavailable
    When upgrade runs
    Then no network call was attempted

  @B-067 @boundary
  Scenario: Upgrade makes no network call when no upstream source is recorded
    Given the manifest records no upstream schema source
    And the network is unavailable
    When upgrade runs
    Then no network call was attempted

  @B-034
  Scenario: Upgrade leaves the recorded upstream version alone
    Given the tool ships schema versions 0.1.0 and 0.2.0
    And the root is pinned to version 0.2.0 and records an upstream schema source at a version of its own
    When upgrade runs against it
    Then the manifest still records the upstream source at that version with the same content hash

  @B-035
  Scenario: Upgrade refuses an upstream source it cannot reach
    Given the manifest records an upstream schema source with its version and content hash
    And the root's local copy does not match that hash
    And the source cannot be reached
    And a snapshot of every file under the root
    When upgrade runs
    Then no file under the root has changed
    And the standard error names the source
    And the exit code is 3

  @B-036
  Scenario: An upstream source with the embedded source selected is rejected
    Given the manifest records an upstream schema source
    And the manifest explicitly selects the embedded source
    When the check runs
    Then the standard error names the contradiction
    And the standard output is empty
    And the exit code is 3

  @B-068
  Scenario: A manifest records an upstream schema source
    Given the manifest records an upstream schema source with its address, its version and a content hash for each schema it fetches
    And the root holds the copy of that source under its schema folder
    When the check runs with JSON output
    Then the document says the schemas came from the upstream copy

  @B-069
  Scenario Outline: An upstream schema source missing a part is invalid
    Given the manifest records an upstream schema source without its <part>
    When the check runs
    Then the manifest is rejected
    And the rejection names the <part>

    Examples:
      | part           |
      | address        |
      | version        |
      | content hashes |

  @B-070
  Scenario Outline: An upstream schema source that is not at a secure web address is invalid
    Given the manifest records an upstream schema source at <address>
    When the check runs
    Then the manifest is rejected
    And the rejection names the address

    Examples:
      | address                        |
      | a web address with no security |
      | a folder on the machine        |
      | a package name                 |

  @B-071
  Scenario: An upstream schema source with an empty version is invalid
    Given the manifest records an upstream schema source with an empty version
    When the check runs
    Then the manifest is rejected
    And the rejection names the version

  @B-072
  Scenario: A fetched schema with no content hash is invalid
    Given the manifest records an upstream schema source with no content hash for the item schema
    When the check runs
    Then the manifest is rejected
    And the rejection names the item schema

  @B-073
  Scenario Outline: A content hash in another form is invalid
    Given the manifest records an upstream schema source whose content hash for the item schema is <form>
    When the check runs
    Then the manifest is rejected
    And the rejection names the item schema

    Examples:
      | form                          |
      | written without its algorithm |
      | written in capital digits     |
      | shorter than the algorithm's  |
      | another algorithm's           |

  @B-074
  Scenario: Each schema is fetched from the source's address and its own file name
    Given the manifest records an upstream schema source with its version and content hash
    And the manifest names the file of each frontmatter schema
    And the root's local copy does not match that hash
    And the source serves content matching that hash
    When upgrade runs
    Then each schema was asked for at the source's address followed by that schema's file name

  @B-074 @boundary
  Scenario: Nothing but the frontmatter schemas is fetched
    Given the manifest records an upstream schema source with its version and content hash
    And the root's local copy does not match that hash
    And the source serves content matching that hash
    When upgrade runs
    Then neither the manifest nor a template was asked for at the source

  @B-075 @boundary
  Scenario: Without epics the epic schema is not fetched
    Given the manifest records an upstream schema source with content hashes for the Feature and item schemas only
    And the manifest declares no epic grammar
    And the root holds the copy of that source under its schema folder
    When the check runs
    Then the check completes

  @B-076
  Scenario: With epics the epic schema is fetched
    Given the manifest records an upstream schema source with content hashes for the Feature and item schemas only
    And the manifest declares the epic grammar
    When the check runs
    Then the manifest is rejected
    And the rejection names the epic schema

  @B-077
  Scenario: The check stops when a recorded source's schema file is absent
    Given the manifest records an upstream schema source
    And the root's schema folder does not hold the item schema
    When the check runs
    Then the exit code is 3

  @B-078
  Scenario: The check names the absent schema file
    Given the manifest records an upstream schema source
    And the root's schema folder does not hold the item schema
    When the check runs
    Then the standard error names the item schema's file

  @B-079
  Scenario: The check names the command that fetches an absent schema file
    Given the manifest records an upstream schema source
    And the root's schema folder does not hold the item schema
    When the check runs
    Then the standard error names the command that fetches it

  @B-080
  Scenario: The check stops when the local copy does not match its hash
    Given the manifest records an upstream schema source with its version and content hash
    And the root's copy of the item schema has been edited since it was fetched
    When the check runs
    Then the exit code is 3

  @B-081
  Scenario: The check names the schema file that does not match its hash
    Given the manifest records an upstream schema source with its version and content hash
    And the root's copy of the item schema has been edited since it was fetched
    When the check runs
    Then the standard error names the item schema's file

  @B-082
  Scenario: Upgrade writes each fetched schema under its own file name
    Given the manifest records an upstream schema source with its version and content hash
    And the manifest names the file of each frontmatter schema
    And the root's local copy does not match that hash
    And the source serves content matching that hash
    When upgrade runs
    Then each fetched schema is in the root's schema folder under the file name the manifest gives it

  @B-083 @boundary
  Scenario: Upgrade sends no credential to the source
    Given the manifest records an upstream schema source with its version and content hash
    And the root's local copy does not match that hash
    And the source serves content matching that hash
    And the machine holds a token the source would accept
    When upgrade runs
    Then nothing the tool asked the source for carried a credential

  @B-037
  Scenario: Version 0.1.0 accepts a specification with no epic when there are no epics
    Given the manifest pins version 0.1.0
    And the manifest declares no epic grammar
    And the root holds a specification whose frontmatter carries no epic
    When the check runs
    Then no frontmatter violation is reported for the specification

  @B-038
  Scenario: Version 0.1.0 accepts a bare parent when there are no epics
    Given the manifest pins version 0.1.0
    And the manifest declares no epic grammar
    And the root holds an item whose parent is "F2"
    When the check runs
    Then no frontmatter violation is reported for the item

  @B-039
  Scenario Outline: A schema version that is not major.minor.patch is invalid configuration
    Given the manifest's schema version is <value>
    When the check runs
    Then the standard error names <value>
    And the standard output is empty
    And the exit code is 3

    Examples:
      | value  |
      | 1      |
      | 0.1    |
      | v0.1.0 |

  @B-040
  Scenario: From 1.0.0 on a new rule is a new major
    Given two schema versions the tool ships, one higher than the other by version precedence
    And the lower one is version 1.0.0 or higher
    And the higher one holds a rule the lower one does not
    When their numbers are compared
    Then the higher one's major is greater than the lower one's

  @B-066
  Scenario: Before 1.0.0 a new rule is at least a new minor
    Given two schema versions the tool ships, one higher than the other by version precedence
    And both are below version 1.0.0
    And the higher one holds a rule the lower one does not
    When their numbers are compared
    Then the higher one's minor is greater than the lower one's

  @B-041
  Scenario: Listing names every shipped version
    Given the tool ships schema versions 0.1.0 and 0.2.0
    And the manifest has been removed from the root
    When the shipped versions are listed
    Then the standard output is 0.1.0 and 0.2.0, one to a line
    And the exit code is 0

  @B-042
  Scenario: Listing orders versions by precedence
    Given the tool ships schema versions 0.9.0 and 0.10.0
    And the manifest has been removed from the root
    When the shipped versions are listed
    Then 0.9.0 is printed before 0.10.0

  @B-084
  Scenario: Listing marks the version the root pins
    Given the tool ships schema versions 0.1.0 and 0.2.0
    And the manifest pins version 0.1.0
    When the shipped versions are listed
    Then version 0.1.0 is marked as the pinned one
    And version 0.2.0 is not marked

  @B-085 @boundary
  Scenario: Listing without a manifest marks nothing
    Given the tool ships schema versions 0.1.0 and 0.2.0
    And the manifest has been removed from the root
    When the shipped versions are listed
    Then no version is marked

  @B-043
  Scenario: Pinning a newer version moves the pin up
    Given the tool ships schema versions 0.1.0 and 0.2.0
    And the manifest pins version 0.1.0
    When specht schema pin 0.2.0 runs
    Then the manifest pins version 0.2.0
    And the exit code is 0

  @B-044
  Scenario: Pinning an older version moves the pin down
    Given the tool ships schema versions 0.1.0 and 0.2.0
    And the manifest pins version 0.2.0
    When specht schema pin 0.1.0 runs
    Then the manifest pins version 0.1.0
    And the exit code is 0

  @B-045
  Scenario Outline: Pinning changes only the version in the manifest
    Given the tool ships schema versions 0.1.0 and 0.2.0
    And the manifest pins version 0.1.0
    And its manifest lowers one rule to a warning, disables another and renames the claims section
    And the manifest uses encoding <encoding>
    And a snapshot of the manifest
    When specht schema pin 0.2.0 runs
    Then the manifest differs from the snapshot only in the version it pins

    Examples:
      | encoding |
      | utf-8    |
      | utf-16   |
      | utf-16BE |
      | utf-32   |
      | utf-32BE |

  @B-046 @boundary
  Scenario: Pinning touches no other file
    Given the tool ships schema versions 0.1.0 and 0.2.0
    And the root is pinned to version 0.1.0 and its frontmatter schemas and templates are version 0.1.0's
    And the root holds specifications, records and companions
    And a snapshot of every file under the root
    When specht schema pin 0.2.0 runs
    Then the only file changed is the manifest
    And the frontmatter schemas and the templates are still version 0.1.0's

  @B-047
  Scenario: Pinning adds a version to a manifest that has none
    Given the tool ships schema versions 0.1.0 and 0.2.0
    And the manifest carries no schema version
    When specht schema pin 0.2.0 runs
    Then the manifest pins version 0.2.0
    And every other manifest key keeps its value

  @B-048
  Scenario Outline: Pinning a version the tool does not ship is a not-found failure
    Given the tool ships schema version 0.1.0
    And a snapshot of every file under the root
    When specht schema pin <value> runs
    Then the standard error names <value> and the versions the tool ships
    And no file under the root has changed
    And the exit code is 4

    Examples:
      | value |
      | 7.0.0 |
      | 0.1   |

  @B-049
  Scenario: Pinning under a missing root is a missing-input failure
    When specht schema pin 0.1.0 runs against a path that is not a directory
    Then nothing is written
    And the exit code is 2

  @B-050
  Scenario: Pinning without a manifest is a missing-input failure
    Given the manifest has been removed from the root
    When specht schema pin 0.1.0 runs
    Then nothing is written
    And the exit code is 2

  @B-051
  Scenario: Pinning in an invalid manifest is invalid configuration
    Given the root's manifest is not valid for a reason other than its schema version
    When specht schema pin 0.1.0 runs
    Then nothing is written
    And the exit code is 3

  @B-052 @boundary
  Scenario: Upgrade never moves the pin
    Given the tool ships schema versions 0.1.0 and 0.2.0
    And the root is pinned to version 0.1.0 and its frontmatter schemas and templates are version 0.2.0's
    When upgrade runs against it
    Then the manifest pins version 0.1.0

  @B-053
  Scenario: A latest-patch policy floats to the newest patch
    Given the tool ships schema versions 0.2.0, 0.2.1 and 0.3.0
    And the manifest pins version 0.2.0 with the pin policy latest-patch
    When the check runs with JSON output
    Then the document names schema version 0.2.1

  @B-054
  Scenario: A latest-minor policy floats to the newest minor
    Given the tool ships schema versions 0.1.0, 0.2.0 and 1.0.0
    And the manifest pins version 0.1.0 with the pin policy latest-minor
    When the check runs with JSON output
    Then the document names schema version 0.2.0

  @B-055 @boundary
  Scenario: A pin is exact unless a policy floats it
    Given the tool ships schema versions 0.1.0, 0.2.0 and 1.0.0
    And the manifest pins version 0.1.0 with no pin policy
    When the check runs with JSON output
    Then the document names schema version 0.1.0

  @B-056 @boundary
  Scenario Outline: A pin policy the tool does not define is invalid configuration
    Given the manifest pins version 0.1.0 with the pin policy <policy>
    When the check runs
    Then the standard error names <policy>
    And the standard output is empty
    And the exit code is 3

    Examples:
      | policy       |
      | latest-major |
      | latest       |

  @B-057
  Scenario: Pinning without a version offers the shipped versions
    Given the tool ships schema versions 0.1.0 and 0.2.0
    And an interactive terminal
    When specht schema pin runs without a version
    Then versions 0.1.0 and 0.2.0 are offered as a single-choice list navigated with the arrow keys

  @B-058
  Scenario: The version chosen from the list is pinned
    Given the tool ships schema versions 0.1.0 and 0.2.0
    And the manifest pins version 0.1.0
    And an interactive terminal
    When specht schema pin runs without a version and 0.2.0 is confirmed with Enter
    Then the manifest pins version 0.2.0

  @B-046 @boundary
  Scenario: Interactive pinning leaves schema files and templates alone
    Given the tool ships schema versions 0.1.0 and 0.2.0
    And the root is pinned to version 0.1.0 and its frontmatter schemas and templates are version 0.1.0's
    And an interactive terminal
    And a snapshot of every file under the root
    When specht schema pin runs without a version and 0.2.0 is confirmed with Enter
    Then the only file changed is the manifest
    And the frontmatter schemas and the templates are still version 0.1.0's

  @B-059
  Scenario: Upgrade that only adds a manifest key succeeds
    Given the tool ships schema versions 0.1.0 and 0.2.0
    And the root is pinned to version 0.2.0 and its frontmatter schemas and templates are already version 0.2.0's
    And its manifest lacks a key version 0.2.0 defines
    When upgrade runs against it
    Then the exit code is 0

  @B-060 @boundary
  Scenario: Upgrade never writes a template the consumer owns
    Given the tool ships schema versions 0.1.0 and 0.2.0
    And the root is pinned to version 0.2.0 and its frontmatter schemas and templates are version 0.1.0's
    And its manifest names the feature template as the consumer's
    And the consumer has rewritten the feature template
    When upgrade runs against it
    Then the feature template is as the consumer wrote it

  @B-061
  Scenario: Upgrade says which templates it skipped and why
    Given the tool ships schema versions 0.1.0 and 0.2.0
    And the root is pinned to version 0.2.0 and its frontmatter schemas and templates are version 0.1.0's
    And its manifest names the feature template as the consumer's
    When upgrade runs against it
    Then the output names the feature template as skipped
    And the reason it skipped it

  @B-062
  Scenario: Upgrade still rewrites the templates the consumer does not own
    Given the tool ships schema versions 0.1.0 and 0.2.0
    And the root is pinned to version 0.2.0 and its templates are version 0.1.0's, which differ from version 0.2.0's
    And its manifest names the feature template as the consumer's
    When upgrade runs against it
    Then every template but the feature template is version 0.2.0's

  @B-063 @boundary
  Scenario Outline: Upgrade leaves alone a template file the tool does not ship
    Given the tool ships schema versions 0.1.0 and 0.2.0
    And the root is pinned to version 0.2.0 and its frontmatter schemas and templates are version 0.1.0's
    And its templates folder holds a runbook template that version 0.2.0 does not ship
    And its manifest <naming> the runbook template as the consumer's
    When upgrade runs against it
    Then the runbook template is still there, as the consumer wrote it
    And the templates folder holds no new file that version 0.2.0 does not ship

    Examples:
      | naming        |
      | names         |
      | does not name |

  @B-005 @boundary
  Scenario: Owning a template leaves the frontmatter schemas the tool's
    Given the tool ships schema versions 0.1.0 and 0.2.0
    And the root's frontmatter schemas and templates are version 0.1.0's
    And the root is pinned to version 0.2.0 with the embedded source and no upstream schema source
    And its manifest names the feature template as the consumer's
    When upgrade runs against it
    Then the frontmatter schemas are version 0.2.0's

  @B-065 @boundary
  Scenario: Owning a template changes nothing in the check
    Given the root holds specifications, records and companions
    And the report the check gives while the manifest names no template as the consumer's
    When the manifest names the feature template as the consumer's and the check runs again
    Then the report is the same

  @B-086
  Scenario: A shipped version's folder is named for its number
    Given the tool ships schema version 0.1.0
    When the folders holding its schemas, its templates and its rule pages are read
    Then each is named v0.1.0

  @B-087
  Scenario: A shipped schema's address names its version folder
    Given the tool ships schema version 0.1.0
    When the address each of its frontmatter schemas declares is read
    Then it is this repository's schema address, then v0.1.0, then the frontmatter folder and the file's name

  @B-088
  Scenario Outline: A change to a template or a rule page alone is a patch
    Given the tool ships schema version 0.1.0 and the next version above it
    And the two differ only in <what>, the version each states aside
    When the two version numbers are compared
    Then they differ in their patch alone

    Examples:
      | what                    |
      | a template              |
      | the text of a rule page |

  @B-089
  Scenario: A locked version still matches its digest
    Given the tool ships schema version 0.1.0 and the version lock records it
    And none of its schemas, templates or rule pages has changed since
    When the shipped versions are checked against the version lock
    Then version 0.1.0 matches the digest the lock records

  @B-089 @boundary
  Scenario Outline: A changed file under a locked version is caught
    Given the tool ships schema version 0.1.0 and the version lock records it
    And <file> of version 0.1.0 has been edited since
    When the shipped versions are checked against the version lock
    Then version 0.1.0 does not match the digest the lock records

    Examples:
      | file                 |
      | a frontmatter schema |
      | the manifest         |
      | a template           |
      | a rule page          |

  @B-090
  Scenario: Only one version is open to change
    Given the tool ships schema versions 0.1.0, 0.2.0 and 0.3.0
    And the version lock records version 0.1.0 only
    When the shipped versions are checked against the version lock
    Then the check fails because two versions are unlocked

  @B-091
  Scenario: The version open to change is the newest
    Given the tool ships schema versions 0.9.0 and 0.10.0
    And the version lock records version 0.10.0 only
    When the shipped versions are checked against the version lock
    Then the check fails because the unlocked version is not the newest

  @B-092
  Scenario: Locking records the newest version
    Given the tool ships schema versions 0.1.0 and 0.2.0
    And the version lock records version 0.1.0 only
    When the newest version is locked
    Then the version lock records version 0.2.0 with the digest of its files

  @B-093
  Scenario: Locking again changes nothing
    Given the tool ships schema versions 0.1.0 and 0.2.0 and the version lock records both
    When the newest version is locked
    Then the version lock is as it was

  @B-093 @boundary
  Scenario: Locking again does not accept a changed version
    Given the tool ships schema version 0.1.0 and the version lock records it
    And a template of version 0.1.0 has been edited since
    When the newest version is locked
    Then the version lock still records the digest version 0.1.0 was locked with

  @B-094
  Scenario: A cut is refused while a version is unlocked
    Given the tool ships schema version 0.2.0 and the version lock does not record it
    When version 0.3.0 is cut
    Then the cut fails
    And no file is created

  @B-095
  Scenario Outline: A cut is refused for a number that is not higher
    Given the tool ships schema versions 0.1.0 and 0.2.0 and the version lock records both
    When version <named> is cut
    Then the cut fails
    And no file is created

    Examples:
      | named |
      | 0.2.0 |
      | 0.1.5 |
      | 2     |

  @B-096
  Scenario: A cut copies the newest version to the new number
    Given the tool ships schema versions 0.1.0 and 0.2.0 and the version lock records both
    When version 0.3.0 is cut
    Then a version 0.3.0 folder stands beside each folder of version 0.2.0
    And each holds a file for every file of version 0.2.0's folder and no other

  @B-097
  Scenario: A cut schema's address names the new version
    Given the tool ships schema version 0.2.0 and the version lock records it
    When version 0.3.0 is cut
    Then each frontmatter schema of version 0.3.0 declares the address of version 0.3.0

  @B-098
  Scenario: A cut rule page gives the new version
    Given the tool ships schema version 0.2.0 and the version lock records it
    When version 0.3.0 is cut
    Then each rule page of version 0.3.0 gives schema version 0.3.0

  @B-099
  Scenario: A cut changes nothing but the stated version
    Given the tool ships schema version 0.2.0 and the version lock records it
    When version 0.3.0 is cut
    Then each file of version 0.3.0 equals version 0.2.0's file of the same name, the stated version aside

  @B-099 @boundary
  Scenario: A cut template states no version
    Given the tool ships schema version 0.2.0 and the version lock records it
    When version 0.3.0 is cut
    Then each template of version 0.3.0 is byte-identical to version 0.2.0's

  @B-100
  Scenario: A cut touches nothing outside the new version
    Given the tool ships schema version 0.2.0 and the version lock records it
    When version 0.3.0 is cut
    Then no file outside version 0.3.0's folders is created, changed or deleted

  @B-100 @boundary
  Scenario: A cut does not lock the new version
    Given the tool ships schema version 0.2.0 and the version lock records it
    When version 0.3.0 is cut
    Then the version lock does not record version 0.3.0

  @B-009
  Scenario: The manifest selects the local frontmatter schemas
    Given the manifest pins schema version "0.2.0"
    And the manifest selects the disk schema source
    And the manifest records no upstream schema source
    And the local Feature schema requires a key absent from the specification
    When the check runs without a command-line schema source
    Then the specification is reported for the missing key

  @B-009 @boundary
  Scenario: Disk selection needs no epic schema without an epic grammar
    Given the manifest pins schema version "0.2.0"
    And the manifest declares no epic grammar
    And the root holds the Feature and task schemas and no epic schema
    And the manifest selects the disk schema source
    When the check runs
    Then the check completes using the Feature and task schemas

  @B-101
  Scenario Outline: A command-line source overrides a manifest source
    Given a valid manifest pinning schema version "0.2.0"
    And the manifest selects the "<manifest>" schema source
    And the local and embedded Feature schemas require different keys
    When the check runs with command-line schema source "<argument>"
    Then the verdict uses the "<argument>" Feature schema

    Examples:
      | manifest | argument |
      | embedded | disk     |
      | disk     | embedded |

  @B-102 @boundary
  Scenario Outline: Older pins reject the manifest source key
    Given the manifest pins schema version "<version>"
    And the manifest selects the disk schema source
    When the check runs
    Then the manifest is rejected for the source selection key

    Examples:
      | version |
      | 0.1.0   |

  @B-103
  Scenario: The command-line source works with an older pin
    Given the manifest pins schema version "0.1.0"
    And the manifest records no upstream schema source
    And the local Feature schema requires a key absent from the specification
    When the check runs with command-line schema source "disk"
    Then the specification is reported for the missing key
    And the pinned version remains "0.1.0"

  @B-009 @B-011 @B-024
  Scenario: Explicit disk selection retains the recorded source
    Given the manifest records an upstream schema source
    And the root holds a matching local copy of that source
    When the check runs with command-line schema source "disk" and JSON output
    Then the document says the schemas came from the upstream source

  @B-009 @B-080 @boundary
  Scenario: Explicit disk selection cannot bypass the recorded hash
    Given the manifest records an upstream schema source
    And a local schema differs from its recorded hash
    When the check runs with command-line schema source "disk"
    Then the standard output is empty
    And the exit code is 3

  @B-036 @boundary
  Scenario: A command-line embedded selection contradicts a recorded source
    Given the manifest records an upstream schema source
    When the check runs with command-line schema source "embedded"
    Then the standard error names the contradiction
    And the standard output is empty
    And the exit code is 3

  @B-036 @boundary
  Scenario: A command-line selection cannot repair an invalid manifest
    Given the manifest pins schema version "0.2.0"
    And the manifest records an upstream schema source
    And the manifest explicitly selects the embedded source
    When the check runs with command-line schema source "disk"
    Then the standard error names the contradiction
    And the standard output is empty
    And the exit code is 3

  @B-104 @B-106 @B-107 @boundary
  Scenario: A missing disk schema is an input failure
    Given the manifest records no upstream schema source
    And a required named local schema file is absent
    When the check runs with command-line schema source "disk"
    Then the standard error names that schema by its root-relative path
    And the standard output is empty
    And the exit code is 3

  @B-105 @B-106 @B-107 @boundary
  Scenario Outline: An invalid disk schema is an input failure
    Given the manifest records no upstream schema source
    And a required named local schema has "<fault>"
    When the check runs with command-line schema source "disk"
    Then the standard error names that schema by its root-relative path
    And the standard output is empty
    And the exit code is 3

    Examples:
      | fault                         |
      | malformed JSON                |
      | an invalid keyword type       |
      | an unresolved local reference |

  @B-108
  Scenario: Listing requires an existing root
    When schema versions are listed against a path that is not a directory
    Then the standard output is empty
    And the exit code is 2

  @B-109
  Scenario: Listing an invalid pin provides recovery information
    Given the manifest pins version 7.0.0
    When the shipped versions are listed
    Then the standard output is empty
    And the standard error names version 7.0.0 and the versions the tool ships
    And the standard error suggests pinning a shipped version
    And the exit code is 3

  @B-110
  Scenario Outline: Pinning repairs an unusable current pin
    Given the manifest's schema version is <value>
    When specht schema pin 0.2.0 runs
    Then the manifest pins version 0.2.0
    And the exit code is 0

    Examples:
      | value |
      | 7.0.0 |
      | wrong |

  @B-111
  Scenario: Pinning validates the root before the target
    When specht schema pin 7.0.0 runs against a path that is not a directory
    Then nothing is written
    And the exit code is 2

  @B-112 @boundary
  Scenario: Downgrading preserves newer manifest settings
    Given the manifest pins version 0.2.0
    And its manifest configures claim tags
    And a snapshot of the manifest
    When specht schema pin 0.1.0 runs
    Then the manifest differs from the snapshot only in the version it pins
    And the manifest pins version 0.1.0

  @B-113 @boundary
  Scenario: Pinning without a terminal requires an explicit version
    Given there is no interactive terminal
    And a snapshot of every file under the root
    When specht schema pin runs without a version
    Then no file under the root has changed
    And the standard error suggests supplying a version
    And the exit code is 2

  @B-114
  Scenario: Pinning confirms the selected version and manifest
    When specht schema pin 0.2.0 runs
    Then the output confirms version 0.2.0 and the manifest

  @B-115
  Scenario: Schema 0.2.0 adds nine rules to the 0.1.0 vocabulary
    Given the tool ships schema versions 0.1.0 and 0.2.0
    When the vocabulary of each version is read
    Then version 0.1.0 holds its 21 rule ids
    And version 0.2.0 holds those 21 and SPEC014, SPEC015, SPEC022, SPEC023, SPEC024, SPEC032, SPEC033, SPEC034 and SPEC035

  @B-116
  Scenario: Upgrade never rewrites the consumer's matrix, test, owner and required-specification settings
    Given the tool ships schema versions 0.1.0 and 0.2.0
    And the root is pinned to version 0.2.0
    And its manifest sets section owners, test files, scenario title matching and required specification directories of the consumer's
    When upgrade runs against it
    Then the section owners, the test files, the title matching and the required directories are as the consumer set them

  @B-117
  Scenario: The 0.2.0 template says the missing-coverage rule reports an approved specification
    Given the tool ships schema versions 0.1.0 and 0.2.0
    When the feature template of each version is read
    Then version 0.2.0's traceability guidance says the missing-coverage rule reports a missing row once the specification is approved, unless the manifest lowers the rule
    And version 0.1.0's says a missing row does not block the specification reaching approved

  @B-118
  Scenario: A done Feature's missing row is reported
    Given the manifest pins schema version 0.3.0
    And the root holds a specification whose delivery status is done and whose traceability table has a missing test
    When the check runs
    Then the missing test is reported by the missing-coverage rule

  @B-118
  Scenario: A done Feature's missing row is reported whatever its agreement status
    Given the manifest pins schema version 0.3.0
    And the root holds a draft specification whose delivery status is done and whose traceability table has a missing test
    When the check runs
    Then the missing test is reported by the missing-coverage rule

  @B-119 @boundary
  Scenario: An approved Feature that is not done is not reported for a missing row
    Given the manifest pins schema version 0.3.0
    And the root holds an approved specification whose delivery status is in progress and whose traceability table has a missing test
    When the check runs
    Then the missing-coverage rule reports nothing

  @B-120
  Scenario Outline: Older versions report an approved Feature's missing row
    Given the manifest pins schema version <version>
    And the root holds an approved specification whose delivery status is in progress and whose traceability table has a missing test
    When the check runs
    Then the missing test is reported by the missing-coverage rule

    Examples:
      | version |
      | 0.1.0   |
      | 0.2.0   |

  @B-121
  Scenario: The missing-coverage message names the done status
    Given the manifest pins schema version 0.3.0
    And the root holds a specification whose delivery status is done and whose traceability table has a missing test
    When the check runs
    Then the missing-coverage message is "status is 'done' while § 9 still carries a 'Missing' cell"

  @B-122 @boundary
  Scenario: The sign-off rule still follows approval
    Given the manifest pins schema version 0.3.0
    And the root holds a specification in review whose delivery status is done and whose sign-off has a row awaiting review
    When the check runs
    Then the sign-off rule reports nothing

  @B-123
  Scenario: The missing-coverage rule keeps its id and its severity
    Given the tool ships schema version 0.3.0
    When its vocabulary is read
    Then the missing-coverage rule is SPEC060 at error severity
    And no other rule id reports missing coverage

  @B-124
  Scenario: The 0.3.0 template says the missing-coverage rule reports a done Feature
    Given the tool ships schema version 0.3.0
    When its feature template is read
    Then its traceability guidance says a missing row does not block the specification reaching approved
    And that the missing-coverage rule reports it once the delivery status is done, unless the manifest lowers the rule

  @B-125
  Scenario: A recorded source supplies the templates it hashes
    Given the manifest records an upstream schema source with its version and content hash
    And the source's record holds a content hash for the feature template
    When the check runs
    Then the manifest is accepted
    And the feature template is one the source supplies

  @B-126
  Scenario Outline: A template hash in another form is invalid
    Given the manifest records an upstream schema source whose content hash for the feature template is <form>
    When the check runs
    Then the manifest is rejected
    And the rejection names the feature template

    Examples:
      | form                          |
      | written without its algorithm |
      | written in capital digits     |
      | shorter than the algorithm's  |
      | another algorithm's           |

  @B-127
  Scenario: A hash for a template the version does not ship is invalid
    Given the manifest records an upstream schema source with a content hash for a runbook template the pinned version does not ship
    When the check runs
    Then the manifest is rejected
    And the rejection names the runbook template

  @B-130
  Scenario: Upgrade writes a fetched template that matches its hash
    Given the manifest records an upstream schema source whose record holds a content hash for the feature template
    And its manifest names no template as the consumer's
    And the root's local copy of the feature template does not match that hash
    And the source serves a feature template matching that hash
    When upgrade runs
    Then the root's feature template is the one the source served

  @B-131
  Scenario: Each template is fetched from the source's address and its own file name
    Given the manifest records an upstream schema source whose record holds a content hash for the feature template
    And the root's local copy of the feature template does not match that hash
    And the source serves a feature template matching that hash
    When upgrade runs
    Then the feature template was asked for at the source's address followed by its file name

  @B-132
  Scenario: Upgrade refuses a fetched template that does not match its hash
    Given the manifest records an upstream schema source whose record holds a content hash for the feature template
    And the root's local copy of the feature template does not match that hash
    And the source serves a feature template that does not match that hash
    And a snapshot of every file under the root
    When upgrade runs
    Then no file under the root has changed
    And the standard error names the source
    And the exit code is 3

  @B-133
  Scenario Outline: Upgrade refuses a source that will not serve a template
    Given the manifest records an upstream schema source whose record holds a content hash for the feature template
    And the root's local copy of the feature template does not match that hash
    And the source <refusal>
    And a snapshot of every file under the root
    When upgrade runs
    Then no file under the root has changed
    And the standard error names the source
    And the exit code is 3

    Examples:
      | refusal                             |
      | cannot be reached                   |
      | asks for a credential               |
      | does not serve the feature template |

  @B-134 @boundary
  Scenario: Upgrade does not fetch templates that match their hashes
    Given the manifest records an upstream schema source whose record holds a content hash for the feature template
    And the root's local copy of each schema and of the feature template matches its hash
    And the network is unavailable
    When upgrade runs
    Then no network call was attempted

  @B-135 @boundary
  Scenario: An upstream source never writes a template the consumer owns
    Given the manifest records an upstream schema source whose record holds a content hash for the feature template
    And the source serves a feature template matching that hash
    And its manifest names the feature template as the consumer's
    And the consumer has rewritten the feature template
    When upgrade runs
    Then the feature template is as the consumer wrote it

  @B-136 @boundary
  Scenario: Upgrade keeps a fetched template over the pinned version's
    Given the manifest records an upstream schema source whose record holds a content hash for the feature template
    And its manifest names no template as the consumer's
    And the root's feature template matches that hash and differs from the pinned version's
    When upgrade runs
    Then the root's feature template is unchanged

  @B-137 @boundary
  Scenario: Upgrade sends no credential when it fetches a template
    Given the manifest records an upstream schema source whose record holds a content hash for the feature template
    And the root's local copy of the feature template does not match that hash
    And the source serves a feature template matching that hash
    And the machine holds a token the source would accept
    When upgrade runs
    Then nothing the tool asked the source for carried a credential
