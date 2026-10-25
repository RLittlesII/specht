#!/usr/bin/env python3
import argparse
import importlib.util
import json
import os
import re
import subprocess
import sys
from collections import Counter, namedtuple
from dataclasses import dataclass, field
from pathlib import Path

ROOT = Path(__file__).resolve().parents[2]
QUEUE_SCRIPT = ROOT / ".agents" / "skills" / "specht-conventions" / "scripts" / "next.py"
MARKER = re.compile(r"<!-- specht-item: (.+?) -->")
PULL = re.compile(r"\(#(\d+)\)$")
DONE_LINE = re.compile(r'\+status:[ \t]*"?done"?[ \t]*(#.*)?')
OWNED_LABELS = {"mirrored", "goal", "stale"}
OWNED_PREFIXES = ("type:", "status:")
SUB_ISSUE_CAP = 100
BODY_LIMIT = 65536
COUNTS = ("created", "edited", "closed", "reopened", "linked", "commented", "stale", "skipped")
FOOTER = (
    "This issue is a read-only mirror of the file above, written from `main` on every push that changes it. "
    "Edits here are overwritten by the next run, and comments are not read."
)
LOG = ["log", "--reverse", "--first-parent", "--format=%x1e%H%x1f%s", "-p", "--unified=0", "--no-renames", "--no-color"]

Commit = namedtuple("Commit", "sha subject done")


def load_queue():
    spec = importlib.util.spec_from_file_location("queue_script", QUEUE_SCRIPT)
    module = importlib.util.module_from_spec(spec)
    spec.loader.exec_module(module)
    return module


queue = load_queue()


@dataclass
class Entry:
    path: str
    id: str
    kind: str
    title: str
    status: str
    labels: set
    parent: str
    text: str
    sections: list
    marker: str = ""
    body: str = ""
    delivered: str = None

    @property
    def done(self):
        return self.status == "done"


@dataclass
class Summary:
    counts: dict = field(default_factory=lambda: dict.fromkeys(COUNTS, 0))
    stale: list = field(default_factory=list)
    skipped: list = field(default_factory=list)
    undelivered: list = field(default_factory=list)

    def skip(self, reason):
        self.counts["skipped"] += 1
        self.skipped.append(reason)


def is_item(path):
    parts = path.split("/")
    return len(parts) > 1 and parts[-2] == ".issue" and parts[-1].endswith(".yml")


def parse_history(text):
    commits = []
    for record in text.split("\x1e")[1:]:
        header, _, diff = record.partition("\n")
        sha, _, subject = header.partition("\x1f")
        done, path = [], None
        for line in diff.split("\n"):
            if line.startswith("diff --git "):
                path = None
            elif line.startswith("+++ "):
                target = line[4:]
                path = target[2:] if target.startswith("b/") and is_item(target[2:]) else None
            elif path and path not in done and DONE_LINE.fullmatch(line.rstrip()):
                done.append(path)
        commits.append(Commit(sha.strip(), subject.strip(), tuple(done)))
    return commits


def delivered_by(path, item_id, commits):
    first = next((commit for commit in commits if path in commit.done), None)
    if first is None:
        return None
    named = PULL.search(first.subject)
    if named:
        return f"#{named[1]}"
    claiming = re.compile(rf"[\w-]+\({re.escape(item_id)}\)!?: .*\(#(\d+)\)$")
    claimed = [match[1] for match in map(claiming.match, (commit.subject for commit in commits)) if match]
    return f"#{claimed[-1]}" if claimed else first.sha


def frontmatter(text):
    match = re.match(r"---\n(.*?)\n---\n", text, re.S)
    return match[1] if match else ""


def read_epics(root):
    epics = []
    for path in sorted(root.glob("epics/*/epic.md")):
        text = path.read_text(encoding="utf-8")
        front = frontmatter(text)
        epic_id = queue.scalar(front, "id")
        name = re.sub(rf"^Epic {re.escape(epic_id)}: ", "", queue.scalar(front, "title"))
        status = queue.scalar(front, "status")
        summary = re.search(r"^## Summary[ \t]*\n(.*?)(?=^## |\Z)", text, re.S | re.M)
        labels = {"mirrored", "type:epic"} | ({f"status:{status}"} if status and status != "done" else set())
        epics.append(
            Entry(
                path=path.relative_to(root).as_posix(),
                id=epic_id,
                kind="epic",
                title=f"Epic {epic_id}: {name}",
                status=status,
                labels=labels,
                parent="",
                text=text,
                sections=[("Summary", summary[1].strip() if summary else queue.scalar(front, "description"))],
            )
        )
    return epics


def read_work_items(root):
    goal = set(queue.read_goal(root))
    items = []
    for path, item in sorted(queue.read_items(root).items()):
        status = item["status"]
        labels = {"mirrored"} | ({f"type:{item['type']}"} if item["type"] else set())
        labels |= {f"status:{status}"} if status and status != "done" else set()
        labels |= {"goal"} if item["id"] in goal else set()
        items.append(
            Entry(
                path=path,
                id=item["id"],
                kind="item",
                title=f"{item['id']}: {item['title']}",
                status=status,
                labels=labels,
                parent="" if item["parent"] in ("", "null") else item["parent"],
                text=(root / path).read_text(encoding="utf-8"),
                sections=[
                    ("Summary", item["summary"]),
                    ("Acceptance criteria", item["acceptance_criteria"]),
                    ("Decisions", item["decisions"]),
                    ("Out of scope", item["out_of_scope"]),
                ],
            )
        )
    return items


def fence(text):
    runs = re.findall(r"^[ \t]*(`{3,})", text, re.M)
    return "`" * max(3, max(map(len, runs), default=0) + 1)


def render(entry, others, blob=None, sections=None):
    lines = [f"<!-- specht-item: {entry.marker} -->", "", f"Mirrored from `{entry.path}` on `main`."]
    if entry.delivered:
        lines.append(f"Delivered by {entry.delivered}.")
    lines += [f"Id {entry.id} is also claimed by `{other}`." for other in others]
    for heading, content in entry.sections[:sections]:
        if not content:
            continue
        lines += ["", f"## {heading}", ""]
        lines += [f"- {line}" for line in content] if isinstance(content, list) else [content]
    if blob:
        source = [f"[`{entry.path}` at `{blob.rsplit('/', 1)[1][:12]}`]({blob}/{entry.path})"]
    else:
        marks = fence(entry.text)
        source = [f"{marks}{'yaml' if entry.kind == 'item' else 'markdown'}", entry.text.rstrip("\n"), marks]
    lines += ["", "## Source", "", *source, "", "---", "", FOOTER]
    return "\n".join(lines)


def fitted(entry, others, blob):
    for arguments in ((), (blob,), (blob, 1)):
        body = render(entry, others, *arguments)
        if len(body) <= BODY_LIMIT:
            return body
    return body


def read_tree(root, commits, blob):
    entries = read_epics(root) + read_work_items(root)
    claims = Counter(entry.id for entry in entries)
    for entry in entries:
        entry.marker = entry.id if claims[entry.id] == 1 else f"{entry.id} {entry.path}"
        if entry.kind == "item" and entry.done:
            entry.delivered = delivered_by(entry.path, entry.id, commits)
    for entry in entries:
        others = [other.path for other in entries if other.id == entry.id and other is not entry]
        entry.body = fitted(entry, others, blob)
    return entries


def same(left, right):
    return (left or "").replace("\r\n", "\n").strip() == (right or "").replace("\r\n", "\n").strip()


def owned(label):
    return label in OWNED_LABELS or label.startswith(OWNED_PREFIXES)


def mirror(root, gh, history, *, pushed=(), backfill=False):
    root = Path(root)
    commits = parse_history(history)
    mirrored = commits[-1].sha if commits else "main"
    entries = read_tree(root, commits, f"https://github.com/{gh.repo}/blob/{mirrored}")
    summary = Summary()
    summary.undelivered = [entry.path for entry in entries if entry.kind == "item" and entry.done and not entry.delivered]
    counts = summary.counts

    current = sorted(gh.issues(), key=lambda issue: issue["number"])
    marked = {}
    for issue in current:
        found = MARKER.search(issue["body"] or "")
        if found:
            marked.setdefault(found[1], issue)
    markers = {entry.marker for entry in entries}
    orphans = [issue for marker, issue in marked.items() if marker not in markers]

    wanted = set().union(*(entry.labels for entry in entries)) | ({"stale"} if orphans else set())
    for label in sorted(wanted - set(gh.labels())):
        gh.create_label(label)

    for entry in entries:
        issue = marked.get(entry.marker)
        if issue is None:
            number = gh.create_issue(entry.title, entry.body, sorted(entry.labels))
            counts["created"] += 1
            issue = {"number": number, "labels": sorted(entry.labels), "state": "open", "state_reason": None, "parent": None}
            marked[entry.marker] = issue
        else:
            labels = set(issue["labels"])
            changes = {
                "title": None if same(issue["title"], entry.title) else entry.title,
                "body": None if same(issue["body"], entry.body) else entry.body,
                "add_labels": sorted(entry.labels - labels),
                "remove_labels": sorted({label for label in labels if owned(label)} - entry.labels),
            }
            if any(changes.values()):
                gh.edit_issue(issue["number"], **{key: value for key, value in changes.items() if value})
                counts["edited"] += 1
        if entry.done and (issue["state"] != "closed" or issue["state_reason"] != "completed"):
            gh.close_issue(issue["number"], "completed")
            counts["closed"] += 1
        elif not entry.done and issue["state"] != "open":
            gh.reopen_issue(issue["number"])
            counts["reopened"] += 1

    by_id = {}
    for entry in entries:
        by_id.setdefault(entry.id, []).append(entry)
    children = Counter(issue["parent"] for issue in current if issue.get("parent"))
    for entry in entries:
        parents = by_id.get(entry.parent, [])
        if len(parents) != 1:
            continue
        parent, child = marked[parents[0].marker]["number"], marked[entry.marker]
        if child.get("parent") == parent:
            continue
        if children[parent] >= SUB_ISSUE_CAP:
            summary.skip(f"`{entry.path}`: #{parent} already has {SUB_ISSUE_CAP} sub-issues, so no link was made")
            continue
        gh.add_sub_issue(parent, child["number"])
        children[parent] += 1
        counts["linked"] += 1

    for issue in orphans:
        summary.stale.append(issue["number"])
        if "stale" not in issue["labels"]:
            gh.edit_issue(issue["number"], add_labels=["stale"])
            counts["stale"] += 1

    deliveries = []
    files = {entry.path: entry for entry in entries}
    for commit in commits:
        if commit.sha not in pushed:
            continue
        for path in commit.done:
            entry = files.get(path)
            if entry is None or not entry.done:
                continue
            named = PULL.search(commit.subject)
            if named:
                deliveries.append((int(named[1]), entry))
            else:
                summary.skip(f"`{path}`: commit {commit.sha} set it done and names no pull request")
    if backfill:
        deliveries += [
            (int(entry.delivered[1:]), entry)
            for entry in entries
            if entry.kind == "item" and entry.done and entry.delivered and entry.delivered.startswith("#")
        ]
    posted = {}
    for pr, entry in deliveries:
        body = f"Delivers #{marked[entry.marker]['number']} ({entry.id})."
        if pr not in posted:
            posted[pr] = set(gh.pr_comments(pr))
        if body in posted[pr]:
            continue
        gh.comment_pr(pr, body)
        posted[pr].add(body)
        counts["commented"] += 1

    return summary


def step_summary(summary):
    lines = ["## Work-item mirror", ""]
    lines += [f"- {kind}: {count}" for kind, count in summary.counts.items()]
    if summary.skipped:
        lines += ["", "### Skipped", ""] + [f"- {reason}" for reason in summary.skipped]
    if summary.stale:
        lines += ["", "### Stale", ""]
        lines += [
            f"- Warning: #{number} carries a `specht-item` marker no file in the tree claims; labelled `stale`."
            for number in summary.stale
        ]
    return "\n".join(lines) + "\n"


class Gh:
    ISSUES = """
query($owner: String!, $name: String!, $endCursor: String) {
  repository(owner: $owner, name: $name) {
    issues(first: 100, after: $endCursor, orderBy: {field: CREATED_AT, direction: ASC}) {
      nodes { id number title body state stateReason labels(first: 100) { nodes { name } } parent { number } }
      pageInfo { hasNextPage endCursor }
    }
  }
}"""
    NODE = "query($owner: String!, $name: String!, $number: Int!) { repository(owner: $owner, name: $name) { issue(number: $number) { id } } }"
    LINK = (
        "mutation($parent: ID!, $child: ID!) "
        "{ addSubIssue(input: {issueId: $parent, subIssueId: $child, replaceParent: true}) { issue { number } } }"
    )

    def __init__(self, repo):
        self.repo = repo
        self.owner, self.name = repo.split("/")
        self.nodes = {}

    def run(self, *arguments, stdin=None):
        done = subprocess.run(["gh", *arguments], input=stdin, text=True, capture_output=True)
        if done.returncode != 0:
            raise RuntimeError(f"gh {' '.join(arguments[:2])} failed: {done.stderr.strip()}")
        return done.stdout

    def graphql(self, query, *variables, jq=None, paginate=False):
        arguments = ["api", "graphql", "-f", f"query={query}", "-f", f"owner={self.owner}", "-f", f"name={self.name}"]
        arguments += [flag for variable in variables for flag in ("-F", variable)]
        arguments += ["--paginate"] if paginate else []
        arguments += ["--jq", jq] if jq else []
        return values(self.run(*arguments))

    def issues(self):
        issues = []
        for page in self.graphql(self.ISSUES, jq=".data.repository.issues.nodes", paginate=True):
            for node in page:
                self.nodes[node["number"]] = node["id"]
                issues.append(
                    {
                        "number": node["number"],
                        "title": node["title"],
                        "body": node["body"],
                        "state": node["state"].lower(),
                        "state_reason": (node["stateReason"] or "").lower() or None,
                        "labels": [label["name"] for label in node["labels"]["nodes"]],
                        "parent": (node["parent"] or {}).get("number"),
                    }
                )
        return issues

    def labels(self):
        return self.run("label", "list", "--repo", self.repo, "--limit", "1000", "--json", "name", "--jq", ".[].name").split()

    def pr_comments(self, pr):
        pages = values(self.run("api", f"repos/{self.repo}/issues/{pr}/comments", "--paginate", "--jq", "[.[].body]"))
        return [body for page in pages for body in page]

    def create_label(self, name):
        self.run("label", "create", name, "--repo", self.repo)

    def create_issue(self, title, body, labels):
        arguments = ["issue", "create", "--repo", self.repo, "--title", title, "--body-file", "-"]
        arguments += [flag for label in labels for flag in ("--label", label)]
        return int(self.run(*arguments, stdin=body).strip().rsplit("/", 1)[1])

    def edit_issue(self, number, title=None, body=None, add_labels=(), remove_labels=()):
        arguments = ["issue", "edit", str(number), "--repo", self.repo]
        arguments += ["--title", title] if title is not None else []
        arguments += ["--body-file", "-"] if body is not None else []
        arguments += [flag for label in add_labels for flag in ("--add-label", label)]
        arguments += [flag for label in remove_labels for flag in ("--remove-label", label)]
        self.run(*arguments, stdin=body)

    def close_issue(self, number, reason):
        self.run("api", "-X", "PATCH", f"repos/{self.repo}/issues/{number}", "-f", "state=closed", "-f", f"state_reason={reason}")

    def reopen_issue(self, number):
        self.run("api", "-X", "PATCH", f"repos/{self.repo}/issues/{number}", "-f", "state=open")

    def add_sub_issue(self, parent, child):
        ids = [self.node(parent), self.node(child)]
        self.run("api", "graphql", "-f", f"query={self.LINK}", "-f", f"parent={ids[0]}", "-f", f"child={ids[1]}")

    def node(self, number):
        if number not in self.nodes:
            self.nodes[number] = self.graphql(self.NODE, f"number={number}", jq=".data.repository.issue.id")[0]
        return self.nodes[number]

    def comment_pr(self, pr, body):
        self.run("pr", "comment", str(pr), "--repo", self.repo, "--body-file", "-", stdin=body)


class Plan:
    def __init__(self, gh):
        self.gh = gh
        self.repo = gh.repo
        self.writes = []

    def issues(self):
        return self.gh.issues()

    def labels(self):
        return self.gh.labels()

    def pr_comments(self, pr):
        return self.gh.pr_comments(pr)

    def record(self, op, **arguments):
        self.writes.append({"op": op, **arguments})

    def create_label(self, name):
        self.record("create_label", name=name)

    def create_issue(self, title, body, labels):
        number = f"new-{sum(write['op'] == 'create_issue' for write in self.writes) + 1}"
        self.record("create_issue", number=number, title=title, body=body, labels=list(labels))
        return number

    def edit_issue(self, number, title=None, body=None, add_labels=(), remove_labels=()):
        fields = {"title": title, "body": body, "add_labels": list(add_labels), "remove_labels": list(remove_labels)}
        self.record("edit_issue", number=number, **{key: value for key, value in fields.items() if value})

    def close_issue(self, number, reason):
        self.record("close_issue", number=number, reason=reason)

    def reopen_issue(self, number):
        self.record("reopen_issue", number=number)

    def add_sub_issue(self, parent, child):
        self.record("add_sub_issue", parent=parent, child=child)

    def comment_pr(self, pr, body):
        self.record("comment_pr", pr=pr, body=body)


def values(text):
    decoder, index, found = json.JSONDecoder(), 0, []
    while True:
        while index < len(text) and text[index].isspace():
            index += 1
        if index == len(text):
            return found
        value, index = decoder.raw_decode(text, index)
        found.append(value)


def describe(write):
    target = write.get("number", write.get("pr", write.get("name", "")))
    detail = {
        "create_issue": lambda: write["title"],
        "edit_issue": lambda: ", ".join(key for key in ("title", "body", "add_labels", "remove_labels") if key in write)
        + "".join(f" +{label}" for label in write.get("add_labels", []))
        + "".join(f" -{label}" for label in write.get("remove_labels", [])),
        "close_issue": lambda: write["reason"],
        "add_sub_issue": lambda: f"under #{write['parent']}",
        "comment_pr": lambda: write["body"],
    }.get(write["op"], lambda: "")()
    return f"| {write['op']} | {'#' if write['op'] != 'create_label' else ''}{target} | {detail} |"


def plan_markdown(summary, writes):
    lines = [step_summary(summary)]
    if summary.undelivered:
        lines += ["### Done with no Delivered by line", ""] + [f"- `{path}`" for path in summary.undelivered] + [""]
    lines += ["### Planned writes", "", "| Write | Target | Detail |", "| --- | --- | --- |"]
    lines += [describe(write) for write in writes]
    return "\n".join(lines) + "\n"


def git(root, *arguments):
    return subprocess.run(["git", "-C", str(root), *arguments], text=True, capture_output=True, check=True).stdout


def pushed_commits(root, before, after):
    if not after:
        return ()
    if not before or set(before) == {"0"}:
        return (after,)
    return tuple(git(root, "rev-list", "--first-parent", f"{before}..{after}").split())


def main():
    parser = argparse.ArgumentParser(description="Mirror the work items and epics as GitHub issues, one way.")
    mode = parser.add_mutually_exclusive_group(required=True)
    mode.add_argument("--dry-run", action="store_true", help="read GitHub and print the plan; write nothing")
    mode.add_argument("--apply", action="store_true", help="make the planned writes, one at a time")
    parser.add_argument("--root", type=Path, default=ROOT)
    parser.add_argument("--repo", default=os.environ.get("GITHUB_REPOSITORY"))
    parser.add_argument("--backfill", action="store_true", help="post every missing Delivers comment (B-027)")
    parser.add_argument("--before", help="the sha main pointed at before the push")
    parser.add_argument("--after", help="the sha the push moved main to")
    parser.add_argument("--json", action="store_true", help="with --dry-run, print the plan as JSON")
    args = parser.parse_args()

    root = args.root.resolve()
    repo = args.repo or subprocess.run(
        ["gh", "repo", "view", "--json", "nameWithOwner", "--jq", ".nameWithOwner"],
        cwd=root, text=True, capture_output=True, check=True,
    ).stdout.strip()
    history = git(root, *LOG, "HEAD")
    pushed = pushed_commits(root, args.before, args.after)
    gh = Gh(repo) if args.apply else Plan(Gh(repo))

    summary = mirror(root, gh, history, pushed=pushed, backfill=args.backfill)

    text = step_summary(summary)
    if os.environ.get("GITHUB_STEP_SUMMARY"):
        with open(os.environ["GITHUB_STEP_SUMMARY"], "a", encoding="utf-8") as output:
            output.write(text)
    if args.apply:
        sys.stdout.write(text)
    elif args.json:
        plan = {"counts": summary.counts, "stale": summary.stale, "skipped": summary.skipped,
                "undelivered": summary.undelivered, "writes": gh.writes}
        sys.stdout.write(json.dumps(plan, indent=2) + "\n")
    else:
        sys.stdout.write(plan_markdown(summary, gh.writes))


if __name__ == "__main__":
    main()
