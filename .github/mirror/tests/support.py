import copy
import importlib
import importlib.util
import sys
import tempfile
from pathlib import Path

MIRROR = Path(__file__).resolve().parents[1]
QUEUE_SCRIPT = MIRROR.parents[1] / ".agents" / "skills" / "specht-conventions" / "scripts" / "next.py"


def load_mirror():
    if str(MIRROR) not in sys.path:
        sys.path.insert(0, str(MIRROR))
    try:
        return importlib.import_module("mirror_items")
    except ModuleNotFoundError as missing:
        if missing.name != "mirror_items":
            raise
        return None


def load_queue():
    spec = importlib.util.spec_from_file_location("queue_script", QUEUE_SCRIPT)
    module = importlib.util.module_from_spec(spec)
    spec.loader.exec_module(module)
    return module


def write_tree(test, files):
    directory = tempfile.TemporaryDirectory()
    test.addCleanup(directory.cleanup)
    root = Path(directory.name)
    for relative, text in files.items():
        path = root / relative
        path.parent.mkdir(parents=True, exist_ok=True)
        path.write_text(text, encoding="utf-8")
    return root


def git_log(*commits):
    records = []
    for sha, subject, done_paths in commits:
        record = f"\x1e{sha}\x1f{subject}\n"
        for path in done_paths:
            record += (
                f"\ndiff --git a/{path} b/{path}\n"
                "index 7103161..5574250 100644\n"
                f"--- a/{path}\n"
                f"+++ b/{path}\n"
                "@@ -5 +5 @@\n"
                "-status: in-review\n"
                "+status: done\n"
            )
        records.append(record)
    return "".join(records)


class Interrupted(Exception):
    pass


class FakeGh:
    def __init__(self, issues=(), labels=(), pulls=None, fail_after=None, repo="owner/name", milestones=()):
        self.repo = repo
        self.store = {}
        for issue in issues:
            stored = {"state": "open", "state_reason": None, "labels": [], "parent": None, "milestone": None, "comments": []}
            stored.update(copy.deepcopy(issue))
            self.store[stored["number"]] = stored
        self.label_store = set(labels)
        self.milestone_store = {}
        for milestone in milestones:
            stored = {"description": "", "state": "open"}
            stored.update(copy.deepcopy(milestone))
            self.milestone_store[stored["number"]] = stored
        self.pulls = {number: {"body": body, "comments": []} for number, body in (pulls or {}).items()}
        self.writes = []
        self.fail_after = fail_after
        self.next_number = 1000
        self.active = 0
        self.most_active = 0

    def issues(self):
        return [
            {key: copy.deepcopy(value) for key, value in issue.items() if key != "comments"}
            for issue in self.store.values()
        ]

    def labels(self):
        return sorted(self.label_store)

    def milestones(self):
        return [copy.deepcopy(milestone) for milestone in self.milestone_store.values()]

    def create_milestone(self, title, description):
        with self.write("create_milestone", title=title, description=description):
            if any(milestone["title"] == title for milestone in self.milestone_store.values()):
                raise LookupError(f"milestone already exists: {title}")
            number = max(self.milestone_store, default=0) + 1
            self.milestone_store[number] = {"number": number, "title": title, "description": description, "state": "open"}
            return number

    def edit_milestone(self, number, description=None, state=None):
        fields = {"description": description, "state": state}
        with self.write("edit_milestone", number=number, **{k: v for k, v in fields.items() if v is not None}):
            milestone = self.milestone_store[number]
            if description is not None:
                milestone["description"] = description
            if state is not None:
                milestone["state"] = state

    def pr_comments(self, pr):
        return list(self.pulls[pr]["comments"])

    def create_label(self, name):
        with self.write("create_label", name=name):
            self.label_store.add(name)

    def create_issue(self, title, body, labels, milestone=None):
        extra = {"milestone": milestone} if milestone is not None else {}
        with self.write("create_issue", title=title, body=body, labels=sorted(labels), **extra):
            self.require_labels(labels)
            self.require_milestone(milestone)
            number = self.next_number
            self.next_number += 1
            self.store[number] = {
                "number": number,
                "title": title,
                "body": body,
                "state": "open",
                "state_reason": None,
                "labels": sorted(labels),
                "parent": None,
                "milestone": milestone,
                "comments": [],
            }
            return number

    def edit_issue(self, number, title=None, body=None, add_labels=(), remove_labels=(), milestone=None, remove_milestone=False):
        fields = {
            "title": title, "body": body, "add_labels": sorted(add_labels), "remove_labels": sorted(remove_labels),
            "milestone": milestone, "remove_milestone": remove_milestone,
        }
        with self.write("edit_issue", number=number, **{k: v for k, v in fields.items() if v not in (None, [], False)}):
            issue = self.store[number]
            self.require_labels(add_labels)
            self.require_milestone(milestone)
            if milestone is not None:
                issue["milestone"] = milestone
            if remove_milestone:
                issue["milestone"] = None
            if title is not None:
                issue["title"] = title
            if body is not None:
                issue["body"] = body
            issue["labels"] = sorted((set(issue["labels"]) | set(add_labels)) - set(remove_labels))

    def close_issue(self, number, reason):
        with self.write("close_issue", number=number, reason=reason):
            self.store[number]["state"] = "closed"
            self.store[number]["state_reason"] = reason

    def reopen_issue(self, number):
        with self.write("reopen_issue", number=number):
            self.store[number]["state"] = "open"
            self.store[number]["state_reason"] = None

    def add_sub_issue(self, parent, child):
        with self.write("add_sub_issue", parent=parent, child=child):
            self.store[parent]
            self.store[child]["parent"] = parent

    def comment_pr(self, pr, body):
        with self.write("comment_pr", pr=pr, body=body):
            self.pulls[pr]["comments"].append(body)

    def require_milestone(self, title):
        if title is not None and not any(milestone["title"] == title for milestone in self.milestone_store.values()):
            raise LookupError(f"milestone not in repository: {title}")

    def require_labels(self, labels):
        unknown = set(labels) - self.label_store
        if unknown:
            raise LookupError(f"label not in repository: {sorted(unknown)}")

    def write(self, op, **args):
        fake = self

        class Write:
            def __enter__(self):
                if fake.fail_after is not None and len(fake.writes) >= fake.fail_after:
                    raise Interrupted(op)
                fake.writes.append((op, args))
                fake.active += 1
                fake.most_active = max(fake.most_active, fake.active)

            def __exit__(self, *_):
                fake.active -= 1
                return False

        return Write()
