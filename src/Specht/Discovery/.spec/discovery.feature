Feature: Discovery
  As the maintainer of a repository on the specification model
  I want the checker to find my specifications where my manifest says they are, without opening what it will ignore
  So that a large tree is checked quickly and a layout of my own needs no fork

  Background:
    Given a repository root holding a manifest and the three frontmatter schemas
    And the manifest is the default one this repository checks itself with

  @B-001
  Scenario: A layout is discovered from the manifest
    Given the manifest declares a third layout whose specification file is a differently named markdown file under a documentation folder
    And the root holds a specification at that place
    When the check runs
    Then that specification is checked by every rule
    And the summary counts it under the third layout's name

  @B-002
  Scenario: A directory name is excluded at any depth
    Given the manifest excludes the directory name "vendor"
    And the root holds a co-located specification three levels below a "vendor" directory
    When the check runs
    Then that specification is not discovered

  @B-002
  Scenario: A root-relative path is excluded only at that place
    Given the manifest excludes the root-relative path ".spec"
    And the root holds a README in its root ".spec" folder
    And the root holds a co-located specification under "src/Thing/.spec"
    When the check runs
    Then the root README is not discovered
    And the specification under "src/Thing/.spec" is

  @B-003
  Scenario: Items, epics and companions are discovered from the manifest
    Given the manifest declares an item file shape, an epic file glob and a companion glob
    And the root holds one item, one epic and one companion matching them beside a specification
    When the check runs
    Then the summary counts one item
    And the epic's frontmatter is checked
    And the companion's tags are resolved against the specification

  @B-004
  Scenario: Path identity applies only to a layout that declares it
    Given the manifest's legacy layout declares which path segments carry the epic and the Feature id
    And the manifest's co-located layout declares none
    And the root holds a legacy specification whose epic directory does not match its frontmatter
    And the root holds a co-located specification under a folder named nothing like its identity
    When the check runs
    Then the legacy specification is reported for the mismatch
    And the co-located specification is not

  @B-005
  Scenario: Discovery in a git work tree never opens an ignored directory
    Given the root is a git work tree whose ignore file lists "node_modules"
    And "node_modules" holds a co-located specification three levels deep
    When the check runs
    Then that specification is not discovered
    And no directory under "node_modules" is opened

  @B-005
  Scenario: An untracked specification is discovered in a git work tree
    Given the root is a git work tree
    And the root holds a co-located specification that is not yet tracked and not ignored
    When the check runs
    Then that specification is discovered

  @B-006
  Scenario: Discovery outside git never enters an excluded directory
    Given the root is not inside a git work tree
    And "obj" holds a co-located specification three levels deep
    When the check runs
    Then that specification is not discovered
    And no directory under "obj" is opened

  @B-006
  Scenario: Discovery runs without git on the path
    Given the root is a git work tree
    And no git executable is on the path
    When the check runs
    Then discovery completes by walking the tree
    And the exit code reflects the violations alone

  @B-007
  Scenario: Both discovery modes agree
    Given this repository's tree at one commit
    When specifications, items, epics and companions are discovered through git
    And they are discovered by walking the tree
    Then the two sets are equal

  @B-008
  Scenario: Discovered files are in one order everywhere
    Given the root holds specifications under "src/b", "src/a" and "src/A"
    When they are discovered through git
    And they are discovered by walking the tree
    Then both list them in the same order
    And the order is by root-relative path, ordinally

  @B-009
  Scenario: A layout is counted by its manifest name
    Given the manifest names its layouts "legacy" and "co-located"
    And the root holds two legacy specifications and one co-located specification
    When the check runs
    Then the summary counts two under "legacy" and one under "co-located"
