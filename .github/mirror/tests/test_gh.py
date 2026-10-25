import json
import subprocess
import unittest
from unittest import mock

from support import load_mirror, write_tree

MIRROR_ITEMS = load_mirror()


def printed_by_gh(pages, arguments):
    lines = []
    for page in pages:
        if "--jq" not in arguments:
            lines.append(json.dumps(page))
            continue
        results = [page]
        for step in arguments[arguments.index("--jq") + 1].lstrip(".").split("."):
            results = [value for result in results for value in result] if step == "[]" else [result[step] for result in results]
        lines += [result if isinstance(result, str) else json.dumps(result) for result in results]
    return printed("".join(f"{line}\n" for line in lines))


def printed(stdout):
    return subprocess.CompletedProcess([], 0, stdout=stdout, stderr="")


def issue_page(*nodes, more=False):
    return {"data": {"repository": {"issues": {"nodes": list(nodes), "pageInfo": {"hasNextPage": more, "endCursor": "Y3Vyc29y"}}}}}


def node_document(number):
    return {"data": {"repository": {"issue": {"id": f"I_kwDOnode{number}"}}}}


class Reads(unittest.TestCase):
    def test_B001_GhPrintingTwoPagesOfIssues_WhenRead_ShouldReturnTheIssuesOfBothPages(self):
        # Given
        first = {
            "id": "I_kwDOnode7", "number": 7, "title": "0041: The feature", "body": "<!-- specht-item: 0041 -->",
            "state": "OPEN", "stateReason": None, "labels": {"nodes": [{"name": "mirrored"}]}, "parent": None,
        }
        second = {
            "id": "I_kwDOnode8", "number": 8, "title": "0042: The task", "body": "<!-- specht-item: 0042 -->",
            "state": "CLOSED", "stateReason": "COMPLETED", "labels": {"nodes": [{"name": "mirrored"}, {"name": "type:task"}]},
            "parent": {"number": 7},
        }
        gh = MIRROR_ITEMS.Gh("owner/name")
        pages = [issue_page(first, more=True), issue_page(second)]

        # When
        with mock.patch.object(MIRROR_ITEMS.subprocess, "run", side_effect=lambda command, **_: printed_by_gh(pages, command)):
            issues = gh.issues()

        # Then
        self.assertEqual(
            issues,
            [
                {"number": 7, "title": "0041: The feature", "body": "<!-- specht-item: 0041 -->", "state": "open",
                 "state_reason": None, "labels": ["mirrored"], "parent": None},
                {"number": 8, "title": "0042: The task", "body": "<!-- specht-item: 0042 -->", "state": "closed",
                 "state_reason": "completed", "labels": ["mirrored", "type:task"], "parent": 7},
            ],
        )

    def test_B054_GhPrintingAnIssueNodeQuery_WhenLookedUp_ShouldReturnTheIssuesNodeId(self):
        # Given
        gh = MIRROR_ITEMS.Gh("owner/name")

        # When
        with mock.patch.object(MIRROR_ITEMS.subprocess, "run", side_effect=lambda command, **_: printed_by_gh([node_document(256)], command)):
            node = gh.node(256)

        # Then
        self.assertEqual(node, "I_kwDOnode256")


class Links(unittest.TestCase):
    def test_B054_TaskAndParentWithNoIssue_WhenMirroredThroughGh_ShouldLinkTheTaskUnderItsParentInTheSameRun(self):
        # Given
        root = write_tree(
            self,
            {
                "area/.issue/0041-feature.yml": 'id: "0041"\ntype: feature\ntitle: The feature\nstatus: ready\nparent: null\n',
                "area/.issue/0042-task.yml": 'id: "0042"\ntype: task\ntitle: The task\nstatus: ready\nparent: "0041"\n',
            },
        )
        created, links = {}, []

        def gh_command(command, input=None, **_):
            arguments = command[1:]
            if arguments[:2] == ["issue", "create"]:
                created[arguments[arguments.index("--title") + 1]] = len(created) + 1
                return printed(f"https://github.com/owner/name/issues/{len(created)}\n")
            if f"query={MIRROR_ITEMS.Gh.ISSUES}" in arguments:
                return printed_by_gh([issue_page()], command)
            if f"query={MIRROR_ITEMS.Gh.NODE}" in arguments:
                return printed_by_gh([node_document(int(arguments[arguments.index("-F") + 1].split("=")[1]))], command)
            if f"query={MIRROR_ITEMS.Gh.LINK}" in arguments:
                links.append(tuple(argument.split("=", 1)[1] for argument in arguments if argument.startswith(("parent=", "child="))))
                return printed_by_gh([{"data": {"addSubIssue": {"issue": {"number": 1}}}}], command)
            if arguments[:2] == ["label", "list"]:
                return printed_by_gh([[]], command)
            return printed("")

        # When
        with mock.patch.object(MIRROR_ITEMS.subprocess, "run", side_effect=gh_command):
            MIRROR_ITEMS.mirror(root, MIRROR_ITEMS.Gh("owner/name"), "")

        # Then
        self.assertEqual(
            links,
            [(f"I_kwDOnode{created['0041: The feature']}", f"I_kwDOnode{created['0042: The task']}")],
        )


if __name__ == "__main__":
    unittest.main()
