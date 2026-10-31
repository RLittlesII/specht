Feature: Work-item mirror
  As a reader of this repository on GitHub
  I want every work item and epic shown as an issue that follows the tree on main
  So that I can see the planned, open and done work without a clone, while the files stay the record

  @B-001
  Scenario: Every work item has one issue
    Given a work item on the main branch
    When the mirror runs
    Then exactly one mirrored issue stands for that work item

  @B-002
  Scenario: Every epic has one issue
    Given an epic on the main branch
    When the mirror runs
    Then exactly one mirrored issue stands for that epic

  @B-003
  Scenario: An issue says which id it mirrors
    Given work item 0045, the only file that claims id 0045
    When the mirror runs
    Then its issue carries a hidden marker naming 0045 and no path

  @B-004
  Scenario: A mirrored issue is labelled as a mirror
    Given a work item on the main branch
    When the mirror runs
    Then its issue is labelled mirrored

  @B-005
  Scenario: A work item's issue is titled by its id and title
    Given work item 0045 titled "Bring the schema set and templates to the pin with upgrade"
    When the mirror runs
    Then its issue is titled "0045: Bring the schema set and templates to the pin with upgrade"

  @B-006
  Scenario: An epic's issue is titled as an epic
    Given epic 0055 titled "Build, versioning, release and consumption"
    When the mirror runs
    Then its issue is titled "Epic 0055: Build, versioning, release and consumption"

  @B-007
  Scenario: An issue is labelled with its item's type
    Given a work item of type bug
    When the mirror runs
    Then its issue is labelled type:bug

  @B-008
  Scenario: An open item's issue is labelled with its status
    Given a work item whose status is blocked
    And its issue is labelled status:ready
    When the mirror runs
    Then its issue is labelled status:blocked
    And it carries no other status label

  @B-009
  Scenario: A done item's issue carries no status label
    Given a work item whose status is done
    And its issue is labelled status:in-review
    When the mirror runs
    Then its issue carries no status label

  @B-010
  Scenario Outline: A goal item's issue is labelled goal
    Given a work item that <listing> in the product goal
    When the mirror runs
    Then its issue <outcome> the goal label

    Examples:
      | listing       | outcome        |
      | is listed     | carries        |
      | is not listed | does not carry |

  @B-011
  Scenario: A missing label is created first
    Given a work item of type spike
    And the repository has no type:spike label
    When the mirror runs
    Then the repository has a type:spike label
    And the item's issue carries it

  @B-012
  Scenario: A done item's issue is closed as completed
    Given a work item whose status is done
    And its issue is open
    When the mirror runs
    Then its issue is closed as completed

  @B-013
  Scenario: An open item's closed issue is reopened
    Given a work item whose status is ready
    And its issue was closed by hand
    When the mirror runs
    Then its issue is open

  @B-014
  Scenario: A child item's issue sits under its parent's issue
    Given a task whose parent is Feature item 0041
    When the mirror runs
    Then the task's issue is a sub-issue of item 0041's issue

  @B-015
  Scenario: A Feature item's issue sits under its epic's issue
    Given a Feature item whose parent is epic 0055
    When the mirror runs
    Then the Feature item's issue is a sub-issue of epic 0055's issue

  @B-016
  Scenario: An issue names the file it mirrors
    Given a work item on the main branch
    When the mirror runs
    Then its issue names the item file's path relative to the repository root

  @B-017
  Scenario: A done item names the pull request that set it done
    Given work item 0045 was first set done by the commit merged as pull request 270
    And the earlier pull request 264 is also titled as work on 0045
    When the mirror runs
    Then its issue says it was delivered by pull request 270

  @B-018
  Scenario: A done item falls back to the latest pull request titled with its id
    Given a done work item whose first done commit names no pull request
    And pull requests 120 and 130 are both titled as work on that item
    When the mirror runs
    Then its issue says it was delivered by pull request 130

  @B-019
  Scenario: A done item from before pull requests names its commit
    Given a done work item whose first done commit names no pull request
    And no commit title names the item
    When the mirror runs
    Then its issue says it was delivered by that commit

  @B-020
  Scenario: An issue shows the item's content
    Given a work item with a summary, acceptance criteria, decisions and out-of-scope entries
    When the mirror runs
    Then its issue shows each of them

  @B-021
  Scenario: An issue carries the item as written
    Given a work item whose issue fits within GitHub's limit on an issue's length
    When the mirror runs
    Then its issue carries the item file exactly as written

  @B-022
  Scenario: An issue says it is a read-only mirror
    Given a work item on the main branch
    When the mirror runs
    Then its issue ends by saying it is a read-only mirror whose comments are not read

  @B-023 @boundary
  Scenario: Two items sharing an id each name the other
    Given two work items both claim id 0220
    When the mirror runs
    Then each has its own issue
    And each issue names the other item's file

  @B-024
  Scenario: The delivering pull request is told which issue it closed
    Given pull request 270 is merged to main and sets work item 0045 done
    When the mirror runs
    Then pull request 270 carries a comment saying it delivers item 0045's issue

  @B-025
  Scenario: A done commit naming no pull request is skipped
    Given a push to main sets a work item done with a commit that names no pull request
    When the mirror runs
    Then no pull request is commented on
    And the run counts the item as skipped

  @B-026
  Scenario: The delivery comment is posted once
    Given pull request 270 already says it delivers item 0045's issue
    When the mirror runs again
    Then pull request 270 carries that comment once

  @B-027
  Scenario: Backfill links items already done
    Given work item 0045 was done before the mirror existed
    And pull request 270 that delivered it has no delivery comment
    When the maintainer starts the mirror by hand with backfill selected
    Then pull request 270 carries one comment saying it delivers item 0045's issue

  @B-028 @boundary
  Scenario: Pull-request bodies are left alone
    Given pull request 270 delivered work item 0045
    When the mirror runs
    Then the description of pull request 270 is unchanged

  @B-029
  Scenario: A run with nothing changed writes nothing
    Given every issue already matches its work item or epic
    And every milestone already matches the release plan
    When the mirror runs
    Then nothing is written to GitHub

  @B-030
  Scenario: A run writes only what differs
    Given a mirrored issue whose title was edited by hand
    And whose description and labels still match its work item
    When the mirror runs
    Then its issue's title is restored from the work item
    And nothing else about the issue is written

  @B-031
  Scenario: An interrupted run is finished by the next one
    Given a run stopped after mirroring half of the work items
    When the mirror runs again
    Then every work item and epic matches its issue

  @B-032 @boundary
  Scenario: A hand-filed issue is never touched
    Given an issue filed by hand that carries no mirror marker
    When the mirror runs
    Then that issue is unchanged
    And it carries no comment from the mirror

  @B-033 @boundary
  Scenario: A removed item's issue is never deleted
    Given a work item's file was removed from the main branch
    When the mirror runs
    Then its issue is not deleted

  @B-064
  Scenario: A removed item's open issue is closed as not planned
    Given a work item's file was removed from the main branch
    And its issue is open
    When the mirror runs
    Then its issue is closed with the reason not planned

  @B-065 @boundary
  Scenario: A removed item's closed issue keeps its state
    Given a work item's file was removed from the main branch
    And its issue was closed as completed
    When the mirror runs
    Then its issue stays closed with the reason completed

  @B-034
  Scenario: A removed item's issue is marked stale
    Given a work item's file was removed from the main branch
    When the mirror runs
    Then its issue is labelled stale

  @B-035
  Scenario: A removed item is reported
    Given a work item's file was removed from the main branch
    When the mirror runs
    Then the run's summary warns that its issue is stale

  @B-036
  Scenario Outline: A push that changes work items runs the mirror
    Given a push to main that changes <a file>
    Then the mirror runs

    Examples:
      | a file                         |
      | a work item                    |
      | an epic                        |
      | the mirror's script            |
      | the mirror's generated workflow |

  @B-037
  Scenario: The mirror can be started by hand
    When the maintainer starts the mirror by hand
    Then the mirror runs

  @B-038
  Scenario: A push that changes no work item does not run the mirror
    Given a push to main that changes only source code
    Then the mirror does not run

  @B-039 @boundary
  Scenario: Branches and pull requests are not mirrored
    Given a work item set in progress on a branch
    When the branch is pushed and a pull request is opened
    Then the mirror does not run
    And the item's issue does not show it in progress

  @B-040
  Scenario: Runs queue and none is cancelled
    Given a mirror run is in progress
    When later pushes to main start more runs
    Then the run in progress finishes uncancelled
    And the newest of the later runs starts after it

  @B-041
  Scenario: A run reports what it did
    When the mirror runs
    Then its summary counts the issues created, edited, closed, reopened, linked, commented, marked stale and skipped
    And it counts the milestones created, edited, closed or reopened

  @B-042 @boundary
  Scenario: Nothing on GitHub is read back into the tree
    Given a mirrored issue that someone commented on, relabelled and closed by hand
    When the mirror runs
    Then no file in the repository changes

  @B-043
  Scenario: An epic's issue is labelled as an epic
    Given an epic on the main branch
    When the mirror runs
    Then its issue is labelled type:epic

  @B-044
  Scenario: An open epic's issue is labelled with its status
    Given an epic whose status is ready-for-architecture
    When the mirror runs
    Then its issue is labelled status:ready-for-architecture
    And it carries no other status label

  @B-045
  Scenario: A done epic's issue is closed as completed
    Given an epic whose status is done
    And its issue is open
    When the mirror runs
    Then its issue is closed as completed

  @B-046
  Scenario: An open epic's closed issue is reopened
    Given an epic whose status is ready-for-architecture
    And its issue was closed by hand
    When the mirror runs
    Then its issue is open

  @B-047
  Scenario: An epic's issue shows its summary
    Given an epic with a Summary section
    When the mirror runs
    Then its issue shows that summary

  @B-048
  Scenario: An epic's issue carries the epic as written
    Given an epic whose issue fits within GitHub's limit on an issue's length
    When the mirror runs
    Then its issue carries the epic file exactly as written

  @B-049 @boundary
  Scenario: A duplicated id's marker names its file
    Given two work items both claim id 0220
    When the mirror runs
    Then each issue's hidden marker names 0220 and that item's file

  @B-050
  Scenario: A done epic's issue carries no status label
    Given an epic whose status is done
    And its issue is labelled status:in-progress
    When the mirror runs
    Then its issue carries no status label

  @B-051
  Scenario: An epic with no summary shows its description
    Given an epic with no Summary section
    When the mirror runs
    Then its issue shows the epic's description where the summary would be
    And then carries the epic file exactly as written

  @B-052
  Scenario: An oversize issue links its file instead of copying it
    Given an epic too long to copy into one issue within GitHub's limit
    When the mirror runs
    Then its issue links the epic file as it stands at the mirrored commit
    And it does not carry the epic file itself

  @B-053
  Scenario: An issue still too long also leaves out the sections after the summary
    Given a work item whose issue is over GitHub's limit even with its file replaced by a link
    When the mirror runs
    Then its issue shows the item's summary and the link to its file
    And it leaves out the item's acceptance criteria, decisions and out-of-scope entries

  @B-054
  Scenario: An issue created in a run sits under its parent's issue after that run
    Given a task whose parent is Feature item 0041, and neither has an issue yet
    When the mirror runs once through the gh command
    Then the task's issue is a sub-issue of item 0041's issue

  @B-055
  Scenario Outline: Each release in the plan has one open milestone
    Given the release plan names release 0.3.0
    And the repository holds <milestones>
    When the mirror runs
    Then the repository has exactly one milestone titled 0.3.0
    And it is open

    Examples:
      | milestones                              |
      | no milestone titled 0.3.0               |
      | a closed milestone titled 0.3.0         |

  @B-056
  Scenario: A milestone of the same title filed by hand is adopted
    Given the release plan names release 0.3.0
    And a milestone titled 0.3.0 was created by hand
    When the mirror runs
    Then that milestone is the one kept for release 0.3.0
    And its description carries the hidden mirror marker

  @B-057
  Scenario: Work needed by two releases takes the earlier one's milestone
    Given Feature item 0041 has an open task
    And release 0.3.0 lists item 0041
    And release 0.4.0 lists an item that waits on that task
    When the mirror runs
    Then the task's issue is in milestone 0.3.0

  @B-058
  Scenario: Work no release needs has no milestone
    Given an open work item that no release lists, and no listed item needs
    And its issue is in milestone 0.3.0
    When the mirror runs
    Then its issue is in no milestone

  @B-059 @boundary
  Scenario: Done work keeps its milestone
    Given a work item whose status is done
    And its issue is in milestone 0.2.0
    And the release plan no longer lists it
    When the mirror runs
    Then its issue is still in milestone 0.2.0

  @B-060 @boundary
  Scenario: A released milestone is closed, not deleted
    Given a milestone the mirror keeps for release 0.2.0
    And the release plan no longer names release 0.2.0
    When the mirror runs
    Then that milestone is closed
    And it still exists

  @B-061 @boundary
  Scenario: A milestone filed by hand for no release is left alone
    Given a milestone titled "Someday" created by hand
    And the release plan names no release "Someday"
    When the mirror runs
    Then that milestone is unchanged

  @B-062 @boundary
  Scenario: A release plan without releases moves no issue
    Given the release plan lists items but names no release
    And an open work item's issue is in milestone 0.3.0
    When the mirror runs
    Then its issue is still in milestone 0.3.0

  @B-063 @boundary
  Scenario: An epic keeps its milestone
    Given an epic whose issue is in milestone 0.3.0
    When the mirror runs
    Then its issue is still in milestone 0.3.0
