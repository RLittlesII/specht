import contextlib
import io
import sys
import unittest

from support import load_queue, write_tree

QUEUE_TREE = {
    ".issue/.goal": "0010\n",
    ".issue/0010-goal-feature.yml": (
        'id: "0010"\n'
        "type: feature\n"
        "title: Ship the goal feature\n"
        "status: ready-for-implementation\n"
        "value: 5\n"
        "risk: 2\n"
        'parent: "0001"\n'
        'children: ["0011", "0012"]\n'
        "depends_on: []\n"
        "spec: area/.spec/README.md\n"
        "summary: |\n"
        "  The feature the product goal names. It has two tasks.\n"
    ),
    "area/.issue/0011-first-task.yml": (
        'id: "0011"\n'
        "type: task\n"
        "title: Build the first part\n"
        "status: ready\n"
        "risk: 2\n"
        'parent: "0010"\n'
        'depends_on: ["0012"]\n'
        "spec: area/.spec/README.md\n"
        "summary: |\n"
        "  Builds the first part. Then the rest.\n"
    ),
    "area/.issue/0012-blocker.yml": (
        'id: "0012"\n'
        "type: task\n"
        "title: Build the part the first one waits on\n"
        "status: ready\n"
        "risk: 1\n"
        'parent: "0010"\n'
        "depends_on: []\n"
        "spec: area/.spec/README.md\n"
        "summary: |\n"
        "  Builds the blocker.\n"
    ),
    ".issue/0013-spike.yml": (
        'id: "0013"\n'
        "type: spike\n"
        "title: Try the other approach\n"
        "status: in-progress\n"
        "risk: 1\n"
        "parent: null\n"
        "depends_on: []\n"
        "spec: null\n"
        "summary: |\n"
        "  A spike in progress.\n"
    ),
    ".issue/0014-done.yml": (
        'id: "0014"\n'
        "type: bug\n"
        "title: Fixed already\n"
        "status: done\n"
        "risk: 1\n"
        "parent: null\n"
        "depends_on: []\n"
        "spec: null\n"
        "summary: |\n"
        "  Done.\n"
    ),
    ".issue/0015-held.yml": (
        'id: "0015"\n'
        "type: chore\n"
        "title: Wait for the owner\n"
        "status: blocked\n"
        "risk: 1\n"
        "parent: null\n"
        "depends_on: []\n"
        "spec: null\n"
        "summary: |\n"
        "  Held by status.\n"
    ),
}

QUEUE_OUTPUT = (
    "## Pick one per lane\n"
    "\n"
    "| Lane | Pick | Rank | Priority | Toward | Gates | Status | Title | Then | Shares |\n"
    "| --- | --- | --- | --- | --- | --- | --- | --- | --- | --- |\n"
    "| 1 | 0012 | 167 | high | 0010 (3) | 0011 | ready | Build the part the first one waits on | - | area/.spec/README.md |\n"
    "\n"
    "## Startable\n"
    "\n"
    "| ID | Rank | Priority | Toward | Gates | Status | Title | Description |\n"
    "| --- | --- | --- | --- | --- | --- | --- | --- |\n"
    "| 0012 | 167 | high | 0010 (3) | 0011 | ready | Build the part the first one waits on | Builds the blocker. |\n"
    "\n"
    "## Blocked at or above that rank\n"
    "\n"
    "- 0011 (high 167) Build the first part - waits on 0012\n"
    "\n"
    "## Blocked by status\n"
    "\n"
    "- 0015 (low 0) Wait for the owner\n"
    "\n"
    "## Taken\n"
    "\n"
    "- 0013 (in-progress) Try the other approach\n"
)


class SharedReaderKeysByPath(unittest.TestCase):
    def test_C7_TwoFilesClaimingOneId_WhenRead_ShouldKeepBothKeyedByRelativePath(self):
        # Given
        root = write_tree(
            self,
            {
                ".issue/0220-rule-page-spike.yml": 'id: "0220"\ntype: spike\ntitle: Rule pages\nstatus: done\n',
                "bench/Startup/.issue/0220-cold-start.yml": 'id: "0220"\ntype: task\ntitle: Cold start\nstatus: ready\n',
            },
        )

        # When
        items = load_queue().read_items(root)

        # Then
        self.assertEqual(
            sorted(items), [".issue/0220-rule-page-spike.yml", "bench/Startup/.issue/0220-cold-start.yml"]
        )
        self.assertEqual(items[".issue/0220-rule-page-spike.yml"]["title"], "Rule pages")
        self.assertEqual(items["bench/Startup/.issue/0220-cold-start.yml"]["title"], "Cold start")
        self.assertEqual({item["id"] for item in items.values()}, {"0220"})


class SharedReaderReadsTheMirrorFields(unittest.TestCase):
    def test_C7_ItemWithTypeAndListFields_WhenRead_ShouldCarryTypeCriteriaDecisionsAndOutOfScope(self):
        # Given
        root = write_tree(
            self,
            {
                "area/.issue/0045-upgrade.yml": (
                    'id: "0045"\n'
                    "type: bug\n"
                    "title: Bring the schema set to the pin\n"
                    "status: ready\n"
                    "\n"
                    "acceptance_criteria:\n"
                    '  - "B-001 - the schemas match the pin"\n'
                    '  - "B-002 - the templates match the pin"\n'
                    "\n"
                    "decisions:\n"
                    "  - |\n"
                    "    2026-10-14, spec-author: the upgrade rewrites\n"
                    "    only files that are present.\n"
                    "\n"
                    "out_of_scope:\n"
                    '  - "The manifest keys - 0047."\n'
                ),
                ".issue/0046-empty.yml": (
                    'id: "0046"\n'
                    "type: chore\n"
                    "title: Nothing listed\n"
                    "status: ready\n"
                    "acceptance_criteria: []\n"
                    "decisions: []\n"
                    "out_of_scope: []\n"
                ),
            },
        )

        # When
        items = load_queue().read_items(root)

        # Then
        self.assertEqual(sorted(items), [".issue/0046-empty.yml", "area/.issue/0045-upgrade.yml"])
        listed = items["area/.issue/0045-upgrade.yml"]
        self.assertEqual(listed["type"], "bug")
        self.assertEqual(
            listed["acceptance_criteria"], ["B-001 - the schemas match the pin", "B-002 - the templates match the pin"]
        )
        self.assertEqual(
            [" ".join(entry.split()) for entry in listed["decisions"]],
            ["2026-10-14, spec-author: the upgrade rewrites only files that are present."],
        )
        self.assertEqual(listed["out_of_scope"], ["The manifest keys - 0047."])
        empty = items[".issue/0046-empty.yml"]
        self.assertEqual(
            (empty["type"], empty["acceptance_criteria"], empty["decisions"], empty["out_of_scope"]),
            ("chore", [], [], []),
        )


class QueueOutputUnchanged(unittest.TestCase):
    def test_C7_TreeWithNoDuplicatedId_WhenQueued_ShouldPrintTheQueueAsBeforeTheReaderChange(self):
        # Given
        root = write_tree(self, QUEUE_TREE)
        queue = load_queue()
        output = io.StringIO()
        arguments = sys.argv

        # When
        sys.argv = ["next.py", "--root", str(root)]
        try:
            with contextlib.redirect_stdout(output):
                queue.main()
        finally:
            sys.argv = arguments

        # Then
        self.assertEqual(output.getvalue(), QUEUE_OUTPUT)


if __name__ == "__main__":
    unittest.main()
