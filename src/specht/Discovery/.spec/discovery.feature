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
    Then that specification is discovered
    And it is checked by every rule

  @B-001
  Scenario: A declared layout list replaces the default layouts
    Given the manifest declares one layout only, the features layout
    And the root holds a specification where the default epics layout would find it
    And the root holds a specification where the features layout finds it
    When the check runs
    Then only the specification in the features layout is discovered
    And the summary names one layout

  @B-002
  Scenario: A directory name is excluded at any depth
    Given the manifest excludes the directory name "vendor"
    And the root holds a co-located specification three levels below a "vendor" directory
    When the check runs
    Then that specification is not discovered

  @B-002
  Scenario: A root-relative path is excluded only at that place
    Given the manifest excludes the root-relative path "/.spec"
    And the root holds a README in its root ".spec" folder
    And the root holds a co-located specification under "src/Thing/.spec"
    When the check runs
    Then the root README is not discovered
    And the specification under "src/Thing/.spec" is discovered

  @B-002
  Scenario: A bare directory name is not anchored to the root
    Given the manifest excludes the directory name "generated"
    And the root holds co-located specifications under "generated/One/.spec" and "src/generated/Two/.spec"
    When the check runs
    Then neither specification is discovered

  @B-003
  Scenario: Items, epics and companions are discovered from the manifest
    Given the manifest declares an item file shape, an epic file glob and a companion glob
    And the root holds one item and one companion matching them beside a specification
    And the root holds an epic file at "epics/0001-example/epic.md"
    When the check runs
    Then the summary counts one item
    And the epic's frontmatter is checked
    And the companion's tags are resolved against the specification

  @B-001
  Scenario: The default layouts are named epics and features
    Given the root holds one specification under the epics folder and one beside code
    When the check runs
    Then the summary and the report name the layouts "epics" and "features", in that order

  @B-012
  Scenario: A file matching any entry of a list is discovered
    Given the manifest declares two item file shapes, two epic file globs and two companion globs
    And the root holds, beside a specification, one item matching the first item shape and one matching the second
    And the root holds one epic file matching the first epic glob and one matching the second
    And the root holds, beside a second specification, one companion matching the second companion glob only
    When the check runs
    Then the summary counts two items
    And both epics' frontmatter is checked
    And that companion's tags are resolved against the second specification

  @B-004
  Scenario: Path identity applies only to a layout that declares it
    Given the manifest's epics layout declares which path segments carry the epic and the Feature id
    And the manifest's features layout declares none
    And the root holds a specification in the epics layout whose epic directory does not match its frontmatter
    And the root holds a specification in the features layout under a folder named nothing like its identity
    When the check runs
    Then the specification in the epics layout is reported for the mismatch
    And the specification in the features layout is not

  @B-005
  Scenario: Discovery in a git work tree never opens an ignored directory
    Given the root is a git work tree whose ignore file lists "dist"
    And the manifest does not exclude "dist"
    And "dist" holds a co-located specification three levels deep
    When the check runs
    Then that specification is not discovered
    And no directory under "dist" is opened

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

  @B-010
  Scenario: A manifest exclusion applies in a git work tree
    Given the root is a git work tree
    And the manifest excludes the root-relative path "/.spec" and the directory name "bin"
    And a README in the root ".spec" folder is tracked
    And a co-located specification under a "bin" directory is tracked
    And a second co-located specification under a "bin" directory is neither tracked nor ignored
    When the check runs
    Then none of the three files is discovered
    And git is never asked to list the root ".spec" folder or any "bin" directory

  @B-006
  Scenario: Discovery runs without git on the path
    Given the root is a git work tree
    And no git executable is on the path
    When the check runs
    Then discovery completes by walking the tree
    And the exit code reflects the violations alone

  @B-007
  Scenario: Both discovery modes agree
    Given a clean checkout of this repository at one commit
    When specifications, items, epics and companions are discovered through git
    And they are discovered by walking the tree
    Then the two sets are equal

  @B-008
  Scenario: Discovered files are in one order everywhere
    Given the root holds specifications under "src/c", "src/a" and "src/B"
    When they are discovered through git
    And they are discovered by walking the tree
    Then both list them in the same order
    And the order is by root-relative path, ordinally, so "src/B" comes before "src/a"

  @B-009
  Scenario: A layout is named by its manifest name
    Given the manifest names its layouts "old-tree" and "beside-code"
    And the root holds one specification in each layout
    When the check runs
    Then the summary and the report name the layouts "old-tree" and "beside-code"

  @B-011
  Scenario: A linked directory is not followed
    Given the root holds a symbolic link to a directory outside it
    And that directory holds a co-located specification
    When the specifications are discovered through git
    And they are discovered by walking the tree
    Then neither discovers that specification
