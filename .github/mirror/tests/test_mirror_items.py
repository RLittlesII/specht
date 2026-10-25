import re
import unittest

from support import FakeGh, Interrupted, git_log, load_mirror, write_tree

MIRROR_ITEMS = load_mirror()

ITEM_0045_PATH = "src/tool/.issue/0045-upgrade.yml"
ITEM_0045 = (
    'id: "0045"\n'
    "type: task\n"
    "title: Bring the schema set and templates to the pin with upgrade\n"
    "\n"
    "status: ready\n"
    "\n"
    "risk: 2\n"
    "\n"
    "parent: null\n"
    "children: []\n"
    "\n"
    "depends_on: []\n"
    "\n"
    "summary: |\n"
    "  The upgrade command rewrites the present schema and template files\n"
    "  to the pinned version.\n"
    "\n"
    "acceptance_criteria:\n"
    '  - "B-001 - present schema files match the pin"\n'
    '  - "B-002 - present templates match the pin"\n'
    "\n"
    "decisions:\n"
    "  - |\n"
    "    2026-10-14, spec-author: a template the manifest names as the\n"
    "    consumer's is left as it is.\n"
    "\n"
    "out_of_scope:\n"
    '  - "The manifest keys - 0047."\n'
)

EPIC_0055_PATH = "epics/0055-build-and-release/epic.md"
EPIC_0055 = (
    "---\n"
    'title: "Epic 0055: Build, versioning, release and consumption"\n'
    'description: "The infrastructure that lets this repository ship from its first push"\n'
    'id: "0055"\n'
    "type: epic\n"
    "status: ready-for-architecture\n"
    "priority: high\n"
    'children: ["0055-F1"]\n'
    'created: "2026-10-08"\n'
    'updated: "2026-10-24"\n'
    "github_issue: null\n"
    "---\n"
    "\n"
    "# Epic 0055: Build, versioning, release and consumption\n"
    "\n"
    "## Summary\n"
    "\n"
    "The infrastructure that turns this repository into one that ships.\n"
    "\n"
    "## Placement\n"
    "\n"
    "Every build step runs from one entry script:\n"
    "\n"
    "```sh\n"
    "./build.sh\n"
    "```\n"
)

DELIVERY = "1111111111111111111111111111111111111111"
DESIGN = "2222222222222222222222222222222222222222"
UNNAMED = "3333333333333333333333333333333333333333"
MIRRORED = "4444444444444444444444444444444444444444"
LIMIT = 65536


def seam(test):
    test.assertIsNotNone(MIRROR_ITEMS, "the seam .github/mirror/mirror_items.py does not exist yet")
    return MIRROR_ITEMS


def mirror(test, root, gh, history="", pushed=(), backfill=False):
    return seam(test).mirror(root, gh, history, pushed=pushed, backfill=backfill)


def marker_of(body):
    match = re.search(r"<!-- specht-item: (.+?) -->", body)
    return match[1] if match else None


def issue_for(test, gh, marker):
    found = [issue for issue in gh.store.values() if marker_of(issue["body"]) == marker]
    test.assertEqual(len(found), 1, f"issues marked {marker}")
    return found[0]


def status_labels(issue):
    return [label for label in issue["labels"] if label.startswith("status:")]


def fenced(test, body, language):
    lines = body.split("\n")
    for start, line in enumerate(lines):
        match = re.fullmatch(r"([`~]{3,})" + language, line)
        if not match:
            continue
        for end in range(start + 1, len(lines)):
            if lines[end] == match[1]:
                return "\n".join(lines[:start]), "\n".join(lines[start + 1 : end]), "\n".join(lines[end + 1 :])
    test.fail(f"no fenced {language} block in the body")


def words(text):
    return " ".join(text.split())


class OneIssuePerFile(unittest.TestCase):
    def test_B001_WorkItemOnMain_WhenMirroredTwice_ShouldKeepExactlyOneIssue(self):
        # Given
        root = write_tree(self, {ITEM_0045_PATH: ITEM_0045})
        gh = FakeGh()

        # When
        mirror(self, root, gh)
        mirror(self, root, gh)

        # Then
        self.assertEqual([marker_of(issue["body"]) for issue in gh.store.values()], ["0045"])

    def test_B002_EpicOnMain_WhenMirroredTwice_ShouldKeepExactlyOneIssue(self):
        # Given
        root = write_tree(self, {EPIC_0055_PATH: EPIC_0055})
        gh = FakeGh()

        # When
        mirror(self, root, gh)
        mirror(self, root, gh)

        # Then
        self.assertEqual([marker_of(issue["body"]) for issue in gh.store.values()], ["0055"])


class Marker(unittest.TestCase):
    def test_B003_IdOneFileClaims_WhenMirrored_ShouldMarkTheBodyWithThePlainMarker(self):
        # Given
        root = write_tree(self, {ITEM_0045_PATH: ITEM_0045})
        gh = FakeGh()

        # When
        mirror(self, root, gh)

        # Then
        self.assertEqual(len(gh.store), 1)
        issue = next(iter(gh.store.values()))
        self.assertIn("<!-- specht-item: 0045 -->", issue["body"])
        self.assertEqual(re.findall(r"<!-- specht-item: .*? -->", issue["body"]), ["<!-- specht-item: 0045 -->"])

    def test_B049_TwoFilesClaimingOneId_WhenMirrored_ShouldMarkEachBodyWithTheIdAndItsOwnPath(self):
        # Given
        spike = ".issue/0220-rule-page-spike.yml"
        baseline = "bench/Startup/.issue/0220-cold-start.yml"
        root = write_tree(
            self,
            {
                spike: 'id: "0220"\ntype: spike\ntitle: Rule pages\nstatus: ready\n',
                baseline: 'id: "0220"\ntype: task\ntitle: Cold start\nstatus: ready\n',
            },
        )
        gh = FakeGh()

        # When
        mirror(self, root, gh)

        # Then
        markers = sorted(marker_of(issue["body"]) for issue in gh.store.values())
        self.assertEqual(len(markers), 2)
        self.assertTrue(all(marker.startswith("0220") for marker in markers), markers)
        for issue in gh.store.values():
            own = spike if "Rule pages" in issue["title"] else baseline
            other = baseline if own == spike else spike
            self.assertIn(own, marker_of(issue["body"]))
            self.assertNotIn(other, marker_of(issue["body"]))


class Labels(unittest.TestCase):
    def test_B004_MirroredIssues_WhenMirrored_ShouldEachCarryTheMirroredLabel(self):
        # Given
        root = write_tree(self, {ITEM_0045_PATH: ITEM_0045, EPIC_0055_PATH: EPIC_0055})
        gh = FakeGh()

        # When
        mirror(self, root, gh)

        # Then
        self.assertEqual(len(gh.store), 2)
        for issue in gh.store.values():
            self.assertIn("mirrored", issue["labels"])

    def test_B007_BugItem_WhenMirrored_ShouldLabelItsIssueTypeBug(self):
        # Given
        root = write_tree(self, {ITEM_0045_PATH: ITEM_0045.replace("type: task", "type: bug")})
        gh = FakeGh()

        # When
        mirror(self, root, gh)

        # Then
        labels = issue_for(self, gh, "0045")["labels"]
        self.assertIn("type:bug", labels)
        self.assertEqual([label for label in labels if label.startswith("type:")], ["type:bug"])

    def test_B008_BlockedItemLabelledReady_WhenMirrored_ShouldCarryOnlyStatusBlocked(self):
        # Given
        root = write_tree(self, {ITEM_0045_PATH: ITEM_0045.replace("status: ready", "status: blocked")})
        gh = FakeGh(
            issues=[{"number": 7, "title": "0045: old", "body": "<!-- specht-item: 0045 -->", "labels": ["status:ready"]}],
            labels=["status:ready"],
        )

        # When
        mirror(self, root, gh)

        # Then
        self.assertEqual(status_labels(issue_for(self, gh, "0045")), ["status:blocked"])

    def test_B009_DoneItemLabelledInReview_WhenMirrored_ShouldCarryNoStatusLabel(self):
        # Given
        root = write_tree(self, {ITEM_0045_PATH: ITEM_0045.replace("status: ready", "status: done")})
        gh = FakeGh(
            issues=[
                {"number": 7, "title": "0045: old", "body": "<!-- specht-item: 0045 -->", "labels": ["status:in-review"]}
            ],
            labels=["status:in-review"],
        )

        # When
        mirror(self, root, gh, git_log((DELIVERY, "task(0045): deliver (#270)", [ITEM_0045_PATH])))

        # Then
        self.assertEqual(status_labels(issue_for(self, gh, "0045")), [])

    def test_B010_ItemAgainstTheGoalFile_WhenMirrored_ShouldCarryGoalExactlyWhenListed(self):
        for goal, expected in (("0045\n", True), ("0046\n", False)):
            with self.subTest(goal=goal):
                # Given
                root = write_tree(self, {ITEM_0045_PATH: ITEM_0045, ".issue/.goal": goal})
                gh = FakeGh()

                # When
                mirror(self, root, gh)

                # Then
                self.assertEqual("goal" in issue_for(self, gh, "0045")["labels"], expected)

    def test_B011_LabelTheRepositoryLacks_WhenMirrored_ShouldBeCreatedBeforeItIsApplied(self):
        # Given
        root = write_tree(self, {ITEM_0045_PATH: ITEM_0045.replace("type: task", "type: spike")})
        gh = FakeGh(labels=["mirrored"])

        # When
        mirror(self, root, gh)

        # Then
        self.assertIn("type:spike", gh.labels())
        self.assertIn("type:spike", issue_for(self, gh, "0045")["labels"])
        created = gh.writes.index(("create_label", {"name": "type:spike"}))
        applied = next(
            index
            for index, (_, args) in enumerate(gh.writes)
            if "type:spike" in args.get("labels", []) + args.get("add_labels", [])
        )
        self.assertLess(created, applied)

    def test_B043_Epic_WhenMirrored_ShouldLabelItsIssueTypeEpic(self):
        # Given
        root = write_tree(self, {EPIC_0055_PATH: EPIC_0055})
        gh = FakeGh()

        # When
        mirror(self, root, gh)

        # Then
        self.assertIn("type:epic", issue_for(self, gh, "0055")["labels"])

    def test_B044_OpenEpicLabelledInProgress_WhenMirrored_ShouldCarryOnlyItsStatusLabel(self):
        # Given
        root = write_tree(self, {EPIC_0055_PATH: EPIC_0055})
        gh = FakeGh(
            issues=[{"number": 7, "title": "Epic 0055: old", "body": "<!-- specht-item: 0055 -->", "labels": ["status:in-progress"]}],
            labels=["status:in-progress"],
        )

        # When
        mirror(self, root, gh)

        # Then
        self.assertEqual(status_labels(issue_for(self, gh, "0055")), ["status:ready-for-architecture"])

    def test_B050_DoneEpicLabelledInProgress_WhenMirrored_ShouldCarryNoStatusLabel(self):
        # Given
        root = write_tree(
            self, {EPIC_0055_PATH: EPIC_0055.replace("status: ready-for-architecture", "status: done")}
        )
        gh = FakeGh(
            issues=[{"number": 7, "title": "Epic 0055: old", "body": "<!-- specht-item: 0055 -->", "labels": ["status:in-progress"]}],
            labels=["status:in-progress"],
        )

        # When
        mirror(self, root, gh)

        # Then
        self.assertEqual(status_labels(issue_for(self, gh, "0055")), [])


class Titles(unittest.TestCase):
    def test_B005_WorkItem_WhenMirrored_ShouldTitleItsIssueByIdAndTitle(self):
        # Given
        root = write_tree(self, {ITEM_0045_PATH: ITEM_0045})
        gh = FakeGh()

        # When
        mirror(self, root, gh)

        # Then
        self.assertEqual(
            issue_for(self, gh, "0045")["title"], "0045: Bring the schema set and templates to the pin with upgrade"
        )

    def test_B006_Epic_WhenMirrored_ShouldTitleItsIssueAsAnEpic(self):
        # Given
        root = write_tree(self, {EPIC_0055_PATH: EPIC_0055})
        gh = FakeGh()

        # When
        mirror(self, root, gh)

        # Then
        self.assertEqual(issue_for(self, gh, "0055")["title"], "Epic 0055: Build, versioning, release and consumption")


class State(unittest.TestCase):
    def test_B012_DoneItemWithOpenIssue_WhenMirrored_ShouldCloseItAsCompleted(self):
        # Given
        root = write_tree(self, {ITEM_0045_PATH: ITEM_0045.replace("status: ready", "status: done")})
        gh = FakeGh(issues=[{"number": 7, "title": "0045: old", "body": "<!-- specht-item: 0045 -->"}])

        # When
        mirror(self, root, gh, git_log((DELIVERY, "task(0045): deliver (#270)", [ITEM_0045_PATH])))

        # Then
        issue = issue_for(self, gh, "0045")
        self.assertEqual((issue["state"], issue["state_reason"]), ("closed", "completed"))

    def test_B013_ReadyItemWithClosedIssue_WhenMirrored_ShouldReopenIt(self):
        # Given
        root = write_tree(self, {ITEM_0045_PATH: ITEM_0045})
        gh = FakeGh(
            issues=[
                {"number": 7, "title": "0045: old", "body": "<!-- specht-item: 0045 -->", "state": "closed", "state_reason": "not_planned"}
            ]
        )

        # When
        mirror(self, root, gh)

        # Then
        self.assertEqual(issue_for(self, gh, "0045")["state"], "open")

    def test_B045_DoneEpicWithOpenIssue_WhenMirrored_ShouldCloseItAsCompleted(self):
        # Given
        root = write_tree(
            self, {EPIC_0055_PATH: EPIC_0055.replace("status: ready-for-architecture", "status: done")}
        )
        gh = FakeGh(issues=[{"number": 7, "title": "Epic 0055: old", "body": "<!-- specht-item: 0055 -->"}])

        # When
        mirror(self, root, gh)

        # Then
        issue = issue_for(self, gh, "0055")
        self.assertEqual((issue["state"], issue["state_reason"]), ("closed", "completed"))

    def test_B046_OpenEpicWithClosedIssue_WhenMirrored_ShouldReopenIt(self):
        # Given
        root = write_tree(self, {EPIC_0055_PATH: EPIC_0055})
        gh = FakeGh(
            issues=[
                {"number": 7, "title": "Epic 0055: old", "body": "<!-- specht-item: 0055 -->", "state": "closed", "state_reason": "completed"}
            ]
        )

        # When
        mirror(self, root, gh)

        # Then
        self.assertEqual(issue_for(self, gh, "0055")["state"], "open")


class SubIssues(unittest.TestCase):
    def test_B014_TaskWhoseParentIsAnItem_WhenMirrored_ShouldBeASubIssueOfTheParentsIssue(self):
        # Given
        root = write_tree(
            self,
            {
                "area/.issue/0041-feature.yml": 'id: "0041"\ntype: feature\ntitle: The feature\nstatus: ready\nparent: null\n',
                "area/.issue/0042-task.yml": 'id: "0042"\ntype: task\ntitle: The task\nstatus: ready\nparent: "0041"\n',
            },
        )
        gh = FakeGh()

        # When
        mirror(self, root, gh)

        # Then
        self.assertEqual(issue_for(self, gh, "0042")["parent"], issue_for(self, gh, "0041")["number"])
        self.assertIsNone(issue_for(self, gh, "0041")["parent"])

    def test_B015_FeatureItemWhoseParentIsAnEpic_WhenMirrored_ShouldBeASubIssueOfTheEpicsIssue(self):
        # Given
        root = write_tree(
            self,
            {
                EPIC_0055_PATH: EPIC_0055,
                ".github/mirror/.issue/0293-mirror.yml": 'id: "0293"\ntype: feature\ntitle: Work-item mirror\nstatus: ready\nparent: "0055"\n',
            },
        )
        gh = FakeGh()

        # When
        mirror(self, root, gh)

        # Then
        self.assertEqual(issue_for(self, gh, "0293")["parent"], issue_for(self, gh, "0055")["number"])


class Provenance(unittest.TestCase):
    def test_B016_MirroredFiles_WhenMirrored_ShouldNameTheirRelativePathInTheBody(self):
        # Given
        root = write_tree(self, {ITEM_0045_PATH: ITEM_0045, EPIC_0055_PATH: EPIC_0055})
        gh = FakeGh()

        # When
        mirror(self, root, gh)

        # Then
        self.assertIn(ITEM_0045_PATH, issue_for(self, gh, "0045")["body"])
        self.assertIn(EPIC_0055_PATH, issue_for(self, gh, "0055")["body"])


class DeliveredBy(unittest.TestCase):
    def test_B017_GitLog_WhenParsed_ShouldListCommitsOldestFirstWithTheItemFilesEachSetDone(self):
        # Given
        log = (
            f"\x1e{DESIGN}\x1ffeature(0045): design the upgrade (#264)\n"
            "\n"
            "diff --git a/src/tool/Upgrade.cs b/src/tool/Upgrade.cs\n"
            "index 1111111..2222222 100644\n"
            "--- a/src/tool/Upgrade.cs\n"
            "+++ b/src/tool/Upgrade.cs\n"
            "@@ -1 +1 @@\n"
            "-status: done\n"
            "+status: done\n"
            "diff --git a/src/tool/.issue/0045-upgrade.yml b/src/tool/.issue/0045-upgrade.yml\n"
            "index 3333333..4444444 100644\n"
            "--- a/src/tool/.issue/0045-upgrade.yml\n"
            "+++ b/src/tool/.issue/0045-upgrade.yml\n"
            "@@ -5 +5 @@\n"
            "-status: ready\n"
            "+status: in-progress\n"
            f"\x1e{UNNAMED}\x1fchore: tidy the queue\n"
            f"\x1e{DELIVERY}\x1ftask(0045): bring the schema set to the pin (#270)\n"
            "\n"
            "diff --git a/src/tool/.issue/0045-upgrade.yml b/src/tool/.issue/0045-upgrade.yml\n"
            "index 4444444..5555555 100644\n"
            "--- a/src/tool/.issue/0045-upgrade.yml\n"
            "+++ b/src/tool/.issue/0045-upgrade.yml\n"
            "@@ -5 +5 @@\n"
            "-status: in-review\n"
            "+status: done\n"
            "diff --git a/.issue/0003-scaffold.yml b/.issue/0003-scaffold.yml\n"
            "new file mode 100644\n"
            "index 0000000..6666666\n"
            "--- /dev/null\n"
            "+++ b/.issue/0003-scaffold.yml\n"
            "@@ -0,0 +1,3 @@\n"
            '+id: "0003"\n'
            "+title: Scaffold\n"
            "+status: done\n"
        )

        # When
        commits = seam(self).parse_history(log)

        # Then
        self.assertEqual(
            [(commit.sha, commit.subject, set(commit.done)) for commit in commits],
            [
                (DESIGN, "feature(0045): design the upgrade (#264)", set()),
                (UNNAMED, "chore: tidy the queue", set()),
                (
                    DELIVERY,
                    "task(0045): bring the schema set to the pin (#270)",
                    {"src/tool/.issue/0045-upgrade.yml", ".issue/0003-scaffold.yml"},
                ),
            ],
        )

    def test_B017_FirstDoneCommitNamesAPullRequest_WhenDerived_ShouldNameItOverEveryOtherSubject(self):
        # Given
        commits = seam(self).parse_history(
            git_log(
                (DESIGN, "feature(0045): design the upgrade (#264)", []),
                (DELIVERY, "task(0045): deliver the upgrade (#270)", [ITEM_0045_PATH]),
                ("4" * 40, "fix(0045): follow up on the upgrade (#281)", []),
                ("5" * 40, "chore(0045): set it done again (#290)", [ITEM_0045_PATH]),
            )
        )

        # When
        delivered = seam(self).delivered_by(ITEM_0045_PATH, "0045", commits)

        # Then
        self.assertEqual(delivered, "#270")

    def test_B018_FirstDoneCommitNamesNoPullRequest_WhenDerived_ShouldNameTheLatestSubjectNamingTheId(self):
        # Given
        path = ".issue/0046-cut.yml"
        commits = seam(self).parse_history(
            git_log(
                ("4" * 40, "task(0046): first part (#120)", []),
                (UNNAMED, "chore: set items done", [path]),
                ("5" * 40, "fix(0046): second part (#130)", []),
                ("6" * 40, "task(0047): another item (#140)", []),
                ("7" * 40, "docs: mention 0046 in passing (#150)", []),
            )
        )

        # When
        delivered = seam(self).delivered_by(path, "0046", commits)

        # Then
        self.assertEqual(delivered, "#130")

    def test_B019_NoPullRequestAndNoSubjectNamingTheId_WhenDerived_ShouldNameTheDoneCommitsSha(self):
        # Given
        path = ".issue/0003-scaffold.yml"
        commits = seam(self).parse_history(
            git_log(
                ("4" * 40, "chore: scaffold the repository", []),
                (UNNAMED, "chore: set the scaffold done", [path]),
                ("5" * 40, "task(0004): next item (#12)", []),
            )
        )

        # When
        delivered = seam(self).delivered_by(path, "0003", commits)

        # Then
        self.assertEqual(delivered, UNNAMED)

    def test_B017_DoneItem_WhenMirrored_ShouldWriteDeliveredByThePullRequestThatSetItDone(self):
        # Given
        root = write_tree(self, {ITEM_0045_PATH: ITEM_0045.replace("status: ready", "status: done")})
        gh = FakeGh()
        history = git_log(
            (DESIGN, "feature(0045): design the upgrade (#264)", []),
            (DELIVERY, "task(0045): deliver the upgrade (#270)", [ITEM_0045_PATH]),
        )

        # When
        mirror(self, root, gh, history)

        # Then
        body = issue_for(self, gh, "0045")["body"]
        lines = [line for line in body.split("\n") if "Delivered by" in line]
        self.assertEqual(len(lines), 1, body)
        self.assertIn("#270", lines[0])
        self.assertNotIn("#264", lines[0])

    def test_B019_DoneItemFromBeforePullRequests_WhenMirrored_ShouldWriteDeliveredByItsCommit(self):
        # Given
        path = ".issue/0003-scaffold.yml"
        root = write_tree(self, {path: 'id: "0003"\ntype: chore\ntitle: Scaffold\nstatus: done\n'})
        gh = FakeGh()

        # When
        mirror(self, root, gh, git_log((UNNAMED, "chore: scaffold the repository", [path])))

        # Then
        body = issue_for(self, gh, "0003")["body"]
        lines = [line for line in body.split("\n") if "Delivered by" in line]
        self.assertEqual(len(lines), 1, body)
        self.assertIn(UNNAMED[:7], lines[0])


class Body(unittest.TestCase):
    def test_B020_ItemWithContent_WhenMirrored_ShouldRenderSummaryCriteriaDecisionsAndOutOfScopeAboveTheFile(self):
        # Given
        root = write_tree(self, {ITEM_0045_PATH: ITEM_0045})
        gh = FakeGh()

        # When
        mirror(self, root, gh)

        # Then
        before, _, _ = fenced(self, issue_for(self, gh, "0045")["body"], "yaml")
        rendered = words(before)
        for expected in (
            "The upgrade command rewrites the present schema and template files to the pinned version.",
            "B-001 - present schema files match the pin",
            "B-002 - present templates match the pin",
            "2026-10-14, spec-author: a template the manifest names as the consumer's is left as it is.",
            "The manifest keys - 0047.",
        ):
            self.assertIn(expected, rendered)

    def test_B021_WorkItem_WhenMirrored_ShouldCarryTheFileVerbatimInAFencedYamlBlock(self):
        # Given
        root = write_tree(self, {ITEM_0045_PATH: ITEM_0045})
        gh = FakeGh()

        # When
        mirror(self, root, gh)

        # Then
        _, inner, _ = fenced(self, issue_for(self, gh, "0045")["body"], "yaml")
        self.assertEqual(inner, ITEM_0045.rstrip("\n"))

    def test_B022_MirroredIssues_WhenMirrored_ShouldEndWithTheReadOnlyFooter(self):
        # Given
        root = write_tree(self, {ITEM_0045_PATH: ITEM_0045, EPIC_0055_PATH: EPIC_0055})
        gh = FakeGh()

        # When
        mirror(self, root, gh)

        # Then
        for marker, language in (("0045", "yaml"), ("0055", "markdown")):
            _, _, after = fenced(self, issue_for(self, gh, marker)["body"], language)
            footer = words(after).lower()
            self.assertIn("read-only mirror", footer)
            self.assertIn("comments", footer)
            self.assertIn("not read", footer)

    def test_B023_TwoFilesClaimingOneId_WhenMirrored_ShouldKeepTwoIssuesEachNamingTheOthersPath(self):
        # Given
        spike = ".issue/0220-rule-page-spike.yml"
        baseline = "bench/Startup/.issue/0220-cold-start.yml"
        root = write_tree(
            self,
            {
                spike: 'id: "0220"\ntype: spike\ntitle: Rule pages\nstatus: ready\n',
                baseline: 'id: "0220"\ntype: task\ntitle: Cold start\nstatus: ready\n',
            },
        )
        gh = FakeGh()

        # When
        mirror(self, root, gh)
        mirror(self, root, gh)

        # Then
        issues = {issue["title"]: issue for issue in gh.store.values()}
        self.assertEqual(sorted(issues), ["0220: Cold start", "0220: Rule pages"])
        self.assertIn(baseline, issues["0220: Rule pages"]["body"])
        self.assertIn(spike, issues["0220: Cold start"]["body"])

    def test_B047_EpicWithSummary_WhenMirrored_ShouldRenderTheSummaryAboveTheFile(self):
        # Given
        root = write_tree(self, {EPIC_0055_PATH: EPIC_0055})
        gh = FakeGh()

        # When
        mirror(self, root, gh)

        # Then
        before, _, _ = fenced(self, issue_for(self, gh, "0055")["body"], "markdown")
        self.assertIn("The infrastructure that turns this repository into one that ships.", words(before))

    def test_B048_Epic_WhenMirrored_ShouldCarryTheFileVerbatimInAFencedMarkdownBlock(self):
        # Given
        root = write_tree(self, {EPIC_0055_PATH: EPIC_0055})
        gh = FakeGh()

        # When
        mirror(self, root, gh)

        # Then
        _, inner, _ = fenced(self, issue_for(self, gh, "0055")["body"], "markdown")
        self.assertEqual(inner, EPIC_0055.rstrip("\n"))

    def test_B051_EpicWithoutSummary_WhenMirrored_ShouldRenderTheDescriptionAboveTheFile(self):
        # Given
        epic = EPIC_0055.replace(
            "## Summary\n\nThe infrastructure that turns this repository into one that ships.\n\n", ""
        )
        root = write_tree(self, {EPIC_0055_PATH: epic})
        gh = FakeGh()

        # When
        mirror(self, root, gh)

        # Then
        before, inner, _ = fenced(self, issue_for(self, gh, "0055")["body"], "markdown")
        self.assertIn("The infrastructure that lets this repository ship from its first push", words(before))
        self.assertEqual(inner, epic.rstrip("\n"))


class OversizeBody(unittest.TestCase):
    HISTORY = git_log((DESIGN, "spec(0055): design it (#264)", []), (MIRRORED, "task(0055): latest (#300)", []))

    def test_B048_EpicWhoseBodyIsExactlyTheLimit_WhenMirrored_ShouldStillCarryTheFileVerbatim(self):
        # Given
        measured = FakeGh()
        mirror(self, write_tree(self, {EPIC_0055_PATH: EPIC_0055}), measured, self.HISTORY)
        epic = EPIC_0055 + "x" * (LIMIT - 1 - len(next(iter(measured.store.values()))["body"])) + "\n"
        root = write_tree(self, {EPIC_0055_PATH: epic})
        gh = FakeGh()

        # When
        mirror(self, root, gh, self.HISTORY)

        # Then
        body = issue_for(self, gh, "0055")["body"]
        self.assertEqual(len(body), LIMIT)
        _, inner, _ = fenced(self, body, "markdown")
        self.assertEqual(inner, epic.rstrip("\n"))

    def test_B052_EpicOneCharacterOverTheLimit_WhenMirrored_ShouldLinkTheFileAtTheMirroredCommitInItsPlace(self):
        # Given
        measured = FakeGh()
        mirror(self, write_tree(self, {EPIC_0055_PATH: EPIC_0055}), measured, self.HISTORY)
        padding = "x" * (LIMIT - len(next(iter(measured.store.values()))["body"]))
        root = write_tree(self, {EPIC_0055_PATH: EPIC_0055 + padding + "\n"})
        gh = FakeGh()

        # When
        mirror(self, root, gh, self.HISTORY)

        # Then
        body = issue_for(self, gh, "0055")["body"]
        target = f"{gh.repo}/blob/{MIRRORED}/{EPIC_0055_PATH}"
        linking = [line for line in body.split("\n") if target in line]
        self.assertEqual(len(linking), 1, f"lines linking {target}")
        self.assertRegex(linking[0], rf"\[[^\]]+\]\(https://\S+/{re.escape(target)}\)")
        self.assertFalse(padding in body, "the epic file is copied into the body")
        self.assertFalse("./build.sh" in body, "the epic file is copied into the body")

    def test_B053_ItemStillOverTheLimitWithItsFileLinked_WhenMirrored_ShouldLeaveOutTheSectionsAfterTheSummary(self):
        # Given
        criteria = "".join(f'  - "B-{n:03} - criterion {n} {"y" * 100}"\n' for n in range(700))
        item = ITEM_0045.replace(
            '  - "B-001 - present schema files match the pin"\n', '  - "B-001 - present schema files match the pin"\n' + criteria
        )
        root = write_tree(self, {ITEM_0045_PATH: item})
        gh = FakeGh()

        # When
        mirror(self, root, gh, self.HISTORY)

        # Then
        body = issue_for(self, gh, "0045")["body"]
        rendered = words(body)
        self.assertTrue(
            "The upgrade command rewrites the present schema and template files to the pinned version." in rendered,
            "the summary is rendered",
        )
        self.assertTrue(f"{gh.repo}/blob/{MIRRORED}/{ITEM_0045_PATH}" in body, "the body links the item file")
        for left_out in (
            "B-001 - present schema files match the pin",
            "criterion 699",
            "a template the manifest names as the consumer's is left as it is.",
            "The manifest keys - 0047.",
        ):
            self.assertFalse(left_out in rendered, f"{left_out!r} is rendered")


class DeliveryComment(unittest.TestCase):
    def test_B024_PushedDoneCommitNamingAPullRequest_WhenMirrored_ShouldCommentDeliversOnIt(self):
        # Given
        root = write_tree(self, {ITEM_0045_PATH: ITEM_0045.replace("status: ready", "status: done")})
        gh = FakeGh(pulls={270: "Brings the schema set to the pin."})
        history = git_log((DELIVERY, "task(0045): deliver the upgrade (#270)", [ITEM_0045_PATH]))

        # When
        mirror(self, root, gh, history, pushed=[DELIVERY])

        # Then
        number = issue_for(self, gh, "0045")["number"]
        self.assertEqual(gh.pr_comments(270), [f"Delivers #{number} (0045)."])

    def test_B025_PushedDoneCommitNamingNoPullRequest_WhenMirrored_ShouldCommentNowhereAndCountItSkipped(self):
        # Given
        root = write_tree(self, {ITEM_0045_PATH: ITEM_0045.replace("status: ready", "status: done")})
        gh = FakeGh(pulls={270: "Brings the schema set to the pin."})
        history = git_log((UNNAMED, "chore: set the upgrade done", [ITEM_0045_PATH]))

        # When
        summary = mirror(self, root, gh, history, pushed=[UNNAMED])

        # Then
        self.assertEqual([write for write in gh.writes if write[0] == "comment_pr"], [])
        self.assertEqual(summary.counts["skipped"], 1)

    def test_B026_DeliveryAlreadyCommented_WhenMirroredAgain_ShouldLeaveOneComment(self):
        # Given
        root = write_tree(self, {ITEM_0045_PATH: ITEM_0045.replace("status: ready", "status: done")})
        gh = FakeGh(pulls={270: "Brings the schema set to the pin."})
        history = git_log((DELIVERY, "task(0045): deliver the upgrade (#270)", [ITEM_0045_PATH]))
        mirror(self, root, gh, history, pushed=[DELIVERY])

        # When
        mirror(self, root, gh, history, pushed=[DELIVERY])
        mirror(self, root, gh, history, pushed=["4" * 40])

        # Then
        self.assertEqual(len(gh.pr_comments(270)), 1)

    def test_B027_BackfillWithDoneItemsLackingTheComment_WhenMirrored_ShouldCommentOnceOnEachDeliveringPullRequest(self):
        # Given
        cut = ".issue/0046-cut.yml"
        root = write_tree(
            self,
            {
                ITEM_0045_PATH: ITEM_0045.replace("status: ready", "status: done"),
                cut: 'id: "0046"\ntype: chore\ntitle: Cut the items\nstatus: done\n',
            },
        )
        gh = FakeGh(pulls={270: "Brings the schema set to the pin.", 130: "Cuts the items."})
        history = git_log(
            (DELIVERY, "task(0045): deliver the upgrade (#270)", [ITEM_0045_PATH]),
            ("4" * 40, "chore(0046): cut the items (#130)", []),
            (UNNAMED, "chore: set items done", [cut]),
        )

        # When
        mirror(self, root, gh, history, backfill=True)
        mirror(self, root, gh, history, backfill=True)

        # Then
        self.assertEqual(gh.pr_comments(270), [f"Delivers #{issue_for(self, gh, '0045')['number']} (0045)."])
        self.assertEqual(gh.pr_comments(130), [f"Delivers #{issue_for(self, gh, '0046')['number']} (0046)."])

    def test_B028_RunThatComments_WhenMirrored_ShouldLeaveEveryPullRequestBodyAsItWas(self):
        # Given
        root = write_tree(self, {ITEM_0045_PATH: ITEM_0045.replace("status: ready", "status: done")})
        gh = FakeGh(pulls={270: "Brings the schema set to the pin.", 264: "Designs the upgrade."})
        history = git_log(
            (DESIGN, "feature(0045): design the upgrade (#264)", []),
            (DELIVERY, "task(0045): deliver the upgrade (#270)", [ITEM_0045_PATH]),
        )

        # When
        mirror(self, root, gh, history, pushed=[DELIVERY], backfill=True)

        # Then
        self.assertEqual(len(gh.pr_comments(270)), 1)
        self.assertEqual(
            {number: pull["body"] for number, pull in gh.pulls.items()},
            {270: "Brings the schema set to the pin.", 264: "Designs the upgrade."},
        )


class Convergence(unittest.TestCase):
    TREE = {
        ".issue/.goal": "0045\n",
        EPIC_0055_PATH: EPIC_0055,
        ITEM_0045_PATH: ITEM_0045.replace('parent: null', 'parent: "0055"'),
        "src/tool/.issue/0046-task.yml": 'id: "0046"\ntype: task\ntitle: The task\nstatus: blocked\nparent: "0045"\n',
        ".issue/0003-scaffold.yml": 'id: "0003"\ntype: chore\ntitle: Scaffold\nstatus: done\nparent: null\n',
    }
    HISTORY = git_log((UNNAMED, "chore: scaffold the repository", [".issue/0003-scaffold.yml"]))

    def test_B029_IssuesMatchingTheTree_WhenMirroredAgain_ShouldWriteNothing(self):
        # Given
        root = write_tree(self, self.TREE)
        gh = FakeGh()
        mirror(self, root, gh, self.HISTORY)
        written = len(gh.writes)
        self.assertGreater(written, 0)

        # When
        mirror(self, root, gh, self.HISTORY)

        # Then
        self.assertEqual(gh.writes[written:], [])

    def test_B030_IssueTitleEditedByHand_WhenMirrored_ShouldWriteOnlyTheTitle(self):
        # Given
        root = write_tree(self, self.TREE)
        gh = FakeGh()
        mirror(self, root, gh, self.HISTORY)
        issue = issue_for(self, gh, "0046")
        issue["title"] = "0046: renamed on GitHub"
        written = len(gh.writes)

        # When
        mirror(self, root, gh, self.HISTORY)

        # Then
        self.assertEqual(gh.writes[written:], [("edit_issue", {"number": issue["number"], "title": "0046: The task"})])

    def test_B031_RunInterruptedPartWay_WhenMirroredAgain_ShouldMatchAnUninterruptedRun(self):
        # Given
        root = write_tree(self, self.TREE)
        clean = FakeGh()
        mirror(self, root, clean, self.HISTORY)
        interrupted = FakeGh(fail_after=len(clean.writes) // 2)
        try:
            mirror(self, root, interrupted, self.HISTORY)
        except Interrupted:
            pass
        interrupted.fail_after = None

        # When
        mirror(self, root, interrupted, self.HISTORY)

        # Then
        def state(gh):
            markers = {issue["number"]: marker_of(issue["body"]) for issue in gh.store.values()}
            return {
                marker_of(issue["body"]): (
                    issue["title"],
                    issue["body"],
                    sorted(issue["labels"]),
                    issue["state"],
                    issue["state_reason"],
                    markers.get(issue["parent"]),
                )
                for issue in gh.store.values()
            }

        self.assertGreater(len(clean.writes), 1)
        self.assertEqual(len(interrupted.store), len(clean.store))
        self.assertEqual(state(interrupted), state(clean))


class Untouched(unittest.TestCase):
    def test_B032_IssueWithoutMarker_WhenMirrored_ShouldReceiveNoWrite(self):
        # Given
        root = write_tree(self, {ITEM_0045_PATH: ITEM_0045.replace("status: ready", "status: done")})
        hand_filed = {
            "number": 256,
            "title": "0045: Bring the schema set and templates to the pin with upgrade",
            "body": "Filed by hand; specht-item 0045 is mentioned but not marked.",
            "labels": ["status:ready"],
        }
        gh = FakeGh(issues=[hand_filed], labels=["status:ready"], pulls={270: "Delivers it."})
        history = git_log((DELIVERY, "task(0045): deliver the upgrade (#270)", [ITEM_0045_PATH]))

        # When
        mirror(self, root, gh, history, pushed=[DELIVERY], backfill=True)

        # Then
        self.assertEqual([args for _, args in gh.writes if 256 in args.values()], [])
        stored = gh.store[256]
        self.assertEqual(
            (stored["title"], stored["body"], stored["labels"], stored["state"], stored["parent"]),
            (hand_filed["title"], hand_filed["body"], ["status:ready"], "open", None),
        )
        self.assertNotIn("#256", " ".join(gh.pr_comments(270)))
        self.assertEqual(len(gh.store), 2)

    def test_B042_IssueChangedByHand_WhenMirrored_ShouldLeaveEveryFileInTheTreeUnchanged(self):
        # Given
        root = write_tree(self, {ITEM_0045_PATH: ITEM_0045, ".issue/.goal": "0045\n"})
        gh = FakeGh(
            issues=[
                {
                    "number": 7,
                    "title": "0045: closed and relabelled by hand",
                    "body": "<!-- specht-item: 0045 -->\nEdited by hand.",
                    "labels": ["wontfix", "status:done"],
                    "state": "closed",
                    "state_reason": "not_planned",
                    "comments": ["Please set this item to done."],
                }
            ],
            labels=["wontfix", "status:done"],
        )
        before = {path.relative_to(root).as_posix(): path.read_bytes() for path in root.rglob("*") if path.is_file()}

        # When
        mirror(self, root, gh)

        # Then
        self.assertEqual(gh.store[7]["state"], "open")
        after = {path.relative_to(root).as_posix(): path.read_bytes() for path in root.rglob("*") if path.is_file()}
        self.assertEqual(after, before)


class RemovedFiles(unittest.TestCase):
    ORPHAN = {"number": 9, "title": "0099: Removed item", "body": "<!-- specht-item: 0099 -->", "labels": ["mirrored", "status:ready"]}

    def test_B033_MarkedIssueWhoseFileIsGone_WhenMirrored_ShouldStayOpen(self):
        # Given
        root = write_tree(self, {ITEM_0045_PATH: ITEM_0045})
        gh = FakeGh(issues=[self.ORPHAN], labels=["mirrored", "status:ready"])

        # When
        mirror(self, root, gh)

        # Then
        issue_for(self, gh, "0045")
        self.assertIn(9, gh.store)
        self.assertEqual(gh.store[9]["state"], "open")
        self.assertEqual([op for op, args in gh.writes if args.get("number") == 9 and op == "close_issue"], [])

    def test_B034_MarkedIssueWhoseFileIsGone_WhenMirrored_ShouldBeLabelledStale(self):
        # Given
        root = write_tree(self, {ITEM_0045_PATH: ITEM_0045})
        gh = FakeGh(issues=[self.ORPHAN], labels=["mirrored", "status:ready"])

        # When
        mirror(self, root, gh)

        # Then
        self.assertIn("stale", gh.store[9]["labels"])
        self.assertNotIn("stale", issue_for(self, gh, "0045")["labels"])

    def test_B035_MarkedIssueWhoseFileIsGone_WhenMirrored_ShouldBeNamedAsAWarningInTheStepSummary(self):
        # Given
        root = write_tree(self, {ITEM_0045_PATH: ITEM_0045})
        gh = FakeGh(issues=[self.ORPHAN], labels=["mirrored", "status:ready"])

        # When
        text = seam(self).step_summary(mirror(self, root, gh))

        # Then
        warnings = [line for line in text.split("\n") if "warn" in line.lower() and re.search(r"(#|issues/)9\b", line)]
        self.assertEqual(len(warnings), 1, text)


class StepSummary(unittest.TestCase):
    def test_B041_RunOfEveryKind_WhenSummarised_ShouldCountEachKindOfWrite(self):
        # Given
        root = write_tree(
            self,
            {
                ITEM_0045_PATH: ITEM_0045,
                ".issue/0046-done.yml": 'id: "0046"\ntype: task\ntitle: Done now\nstatus: done\n',
                ".issue/0047-ready.yml": 'id: "0047"\ntype: task\ntitle: Ready again\nstatus: ready\n',
                ".issue/0048-child.yml": 'id: "0048"\ntype: task\ntitle: A child\nstatus: ready\nparent: "0045"\n',
                ".issue/0049-unnamed.yml": 'id: "0049"\ntype: chore\ntitle: Done without a pull request\nstatus: done\n',
            },
        )
        gh = FakeGh(
            issues=[
                {"number": 2, "title": "0046: Done now", "body": "<!-- specht-item: 0046 -->"},
                {"number": 3, "title": "0047: Ready again", "body": "<!-- specht-item: 0047 -->", "state": "closed", "state_reason": "completed"},
                {"number": 4, "title": "0049: old", "body": "<!-- specht-item: 0049 -->", "state": "closed", "state_reason": "completed"},
                {"number": 9, "title": "0099: Removed item", "body": "<!-- specht-item: 0099 -->"},
            ],
            pulls={270: "Delivers 0046."},
        )
        history = git_log(
            (DELIVERY, "task(0046): deliver it (#270)", [".issue/0046-done.yml"]),
            (UNNAMED, "chore: set 0049 done", [".issue/0049-unnamed.yml"]),
        )

        # When
        summary = mirror(self, root, gh, history, pushed=[DELIVERY, UNNAMED])
        text = seam(self).step_summary(summary)

        # Then
        counts = dict(summary.counts)
        self.assertEqual(
            sorted(counts), sorted(["created", "edited", "closed", "reopened", "linked", "commented", "stale", "skipped"])
        )
        self.assertGreaterEqual(counts.pop("edited"), 1)
        self.assertEqual(
            counts, {"created": 2, "closed": 1, "reopened": 1, "linked": 1, "commented": 1, "stale": 1, "skipped": 1}
        )
        lines = text.lower().split("\n")
        for kind, count in summary.counts.items():
            self.assertTrue(
                any(kind in line and re.search(rf"\b{count}\b", line) for line in lines), f"{kind} {count} in {text}"
            )


class Constraints(unittest.TestCase):
    def test_C4_TreeInATemporaryRoot_WhenMirrored_ShouldWriteNoAbsolutePath(self):
        # Given
        root = write_tree(
            self,
            {
                ITEM_0045_PATH: ITEM_0045.replace("status: ready", "status: done"),
                EPIC_0055_PATH: EPIC_0055,
                ".issue/0220-a.yml": 'id: "0220"\ntype: spike\ntitle: A\nstatus: ready\n',
                "b/.issue/0220-b.yml": 'id: "0220"\ntype: task\ntitle: B\nstatus: ready\n',
            },
        )
        gh = FakeGh(
            issues=[{"number": 9, "title": "0099: Removed item", "body": "<!-- specht-item: 0099 -->"}],
            pulls={270: "Delivers 0045."},
        )
        history = git_log((DELIVERY, "task(0045): deliver it (#270)", [ITEM_0045_PATH]))

        # When
        summary = mirror(self, root, gh, history, pushed=[DELIVERY])

        # Then
        self.assertGreater(len(gh.writes), 0)
        written = [seam(self).step_summary(summary)] + [str(args) for _, args in gh.writes]
        for absolute in {str(root), str(root.resolve())}:
            self.assertEqual([text for text in written if absolute in text], [])

    def test_C10_Run_WhenMirrored_ShouldMakeOneWriteAtATime(self):
        # Given
        root = write_tree(self, Convergence.TREE)
        gh = FakeGh()

        # When
        mirror(self, root, gh, Convergence.HISTORY)

        # Then
        self.assertGreater(len(gh.writes), 1)
        self.assertEqual(gh.most_active, 1)


if __name__ == "__main__":
    unittest.main()
