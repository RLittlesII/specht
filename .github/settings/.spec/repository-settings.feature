Feature: Repository settings checklist
  As the maintainer pushing this repository for the first time
  I want every setting that makes a check binding written down beside the code
  So that I apply them once, in one sitting, and a reviewer can see them change

  @B-001
  Scenario: Main accepts changes only through pull requests
    When the maintainer reads the settings checklist
    Then it requires a pull request before any change reaches the main branch

  @B-002
  Scenario: The required checks are the checks CI produces
    Given the settings checklist and the integration workflow
    Then the checks the checklist requires from integration are exactly the checks integration produces

  @B-003
  Scenario: The patch status is required and the total is not
    When the maintainer reads the settings checklist
    Then it requires the patch coverage status
    And it does not require the project coverage status

  @B-004
  Scenario: A squash keeps the commit messages
    When the maintainer reads the settings checklist
    Then it enables squash merging built from the commit messages

  @B-005
  Scenario: Rebase is allowed and merge commits are not
    When the maintainer reads the settings checklist
    Then it enables rebase merging
    And it disables merge commits

  @B-006
  Scenario: A green pull request can merge itself
    When the maintainer reads the settings checklist
    Then it enables merging a pull request automatically once its required checks pass

  @B-007
  Scenario: Every secret a workflow reads is listed
    Given the settings checklist and the committed workflows
    Then every secret a workflow reads is named in the checklist with the workflow that reads it

  @B-008
  Scenario: The service installations are listed
    When the maintainer reads the settings checklist
    Then it names the dependency-update and coverage service installations

  @B-009 @boundary
  Scenario: Outside reports can still be filed
    When the maintainer reads the settings checklist
    Then it keeps issues enabled for outside reports
    And nothing in it makes an issue a tracked work item
