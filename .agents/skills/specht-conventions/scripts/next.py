#!/usr/bin/env python3
import argparse
import os
import re
from pathlib import Path

SKIPPED = {".git", "bin", "obj", "node_modules", "graphify-out", ".artifacts", "worktrees"}
STARTABLE = {"ready", "ready-for-architecture", "ready-for-implementation"}
TAKEN = {"in-progress", "in-review"}
PRIORITIES = ["high", "medium", "low"]
SHARED_WRITE_SETS = [
    (".build/ContinuousIntegration", "generated workflows"),
    (".build/Releasing", "generated workflows"),
    (".github", "generated workflows"),
    (".build", ".build/SpechtBuild.cs"),
]


def scalar(text, key):
    match = re.search(rf"^{key}:[ \t]*(.*)$", text, re.M)
    return match[1].split(" #")[0].strip().strip('"') if match else ""


def block(text, key):
    match = re.search(rf"^{key}:[ \t]*\|[^\n]*\n((?:[ \t]+.*\n?|\n)*)", text, re.M)
    return " ".join(match[1].split()) if match else ""


def describe(summary, width):
    sentence = re.split(r"(?<=[.!?])\s", summary, maxsplit=1)[0]
    return sentence if len(sentence) <= width else sentence[: width - 1].rstrip() + "…"


def read_items(root):
    items = {}
    for directory, folders, files in os.walk(root):
        folders[:] = sorted(f for f in folders if f not in SKIPPED)
        if os.path.basename(directory) != ".issue":
            continue
        for name in sorted(files):
            if not re.match(r"\d{4}-.*\.yml$", name):
                continue
            path = Path(directory, name)
            text = path.read_text(encoding="utf-8")
            home = path.parent.parent.relative_to(root).as_posix()
            items[name[:4]] = {
                "id": name[:4],
                "title": scalar(text, "title"),
                "status": scalar(text, "status"),
                "value": scalar(text, "value"),
                "risk": int(scalar(text, "risk") or 0),
                "parent": scalar(text, "parent"),
                "spec": scalar(text, "spec"),
                "home": "" if home == "." else home,
                "depends_on": re.findall(r'"(\d{4})"', scalar(text, "depends_on")),
                "summary": block(text, "summary"),
            }
    return items


def write_sets(item):
    sets = set()
    if item["spec"] not in ("", "null"):
        sets.add(item["spec"])
    for prefix, name in SHARED_WRITE_SETS:
        if item["home"] == prefix or item["home"].startswith(prefix + "/"):
            sets.add(name)
            break
    return sets


def lanes(items):
    lanes = []
    for item in items:
        sets = write_sets(item)
        joined = [lane for lane in lanes if lane["sets"] & sets]
        merged = {"items": [item], "sets": set(sets)}
        for lane in joined:
            merged["items"] = lane["items"] + merged["items"]
            merged["sets"] |= lane["sets"]
            lanes.remove(lane)
        merged["items"].sort(key=order)
        lanes.append(merged)
    return sorted(lanes, key=lambda lane: order(lane["items"][0]))


def order(item):
    return (item["tier"], -item["rank"], -item["waiting"], item["id"])


def read_goal(root):
    path = root / ".issue" / ".goal"
    return re.findall(r"^\d{4}$", path.read_text(encoding="utf-8"), re.M) if path.exists() else []


def value(item, items):
    if item["value"] not in ("", "null"):
        return int(item["value"])
    parent = items.get(item["parent"])
    return value(parent, items) if parent else 0


def reach(ids, open_items, children):
    seen, stack = set(), list(ids)
    while stack:
        current = stack.pop()
        if current in seen or current not in open_items:
            continue
        seen.add(current)
        stack += open_items[current]["depends_on"] + children.get(current, [])
    return seen


def score(item, closure, items, open_items):
    cost = sum(open_items[i]["risk"] for i in closure)
    return round(value(item, items) * 100 / max(cost, 1)), cost


def rank(items, open_items, goal):
    children = {}
    for item in open_items.values():
        children.setdefault(item["parent"], []).append(item["id"])
    work = {i: reach([i], open_items, children) - set(children) for i in open_items}
    features = {i: score(open_items[i], work[i], items, open_items) for i in open_items if i in children}
    product = reach(goal, open_items, children)

    for item in open_items.values():
        toward = [(features[f][0], f) for f in features if item["id"] in work[f] | {f}]
        if toward:
            item["rank"], feature = max(toward, key=lambda t: (t[0], -int(t[1])))
            item["toward"] = f"{feature} ({features[feature][1]})"
        else:
            item["rank"], item["toward"] = score(item, work[item["id"]], items, open_items)[0], "-"
        item["tier"] = 0 if item["id"] in product else 1 if toward else 2
        item["priority"] = PRIORITIES[item["tier"]]
        item["waiting"] = sum(1 for i in open_items if i != item["id"] and item["id"] in work[i])
        item["gates"] = ", ".join(sorted(i for i in open_items if item["id"] in open_items[i]["depends_on"])) or "-"
    return children


def main():
    parser = argparse.ArgumentParser(description="Rank the .issue/ queue and group parallel lanes.")
    parser.add_argument("--root", type=Path, default=Path(__file__).resolve().parents[4])
    parser.add_argument("--top", type=int, default=8)
    parser.add_argument("--width", type=int, default=160)
    args = parser.parse_args()

    root = args.root.resolve()
    items = read_items(root)
    containers = rank(items, {i["id"]: i for i in items.values() if i["status"] != "done"}, read_goal(root))
    open_items = sorted((i for i in items.values() if i["status"] != "done"), key=order)
    for item in open_items:
        item["blockers"] = [
            d if items.get(d) else f"{d}?" for d in item["depends_on"] if items.get(d, {}).get("status") != "done"
        ]

    startable = [
        i for i in open_items if i["status"] in STARTABLE and not i["blockers"] and i["id"] not in containers
    ][: args.top]
    floor = order(startable[-1])[:2] if startable else (0, 0)
    blocked = [
        i for i in open_items if i["status"] in STARTABLE | {"blocked"} and i["blockers"] and order(i)[:2] <= floor
    ]
    held = [i for i in open_items if i["status"] == "blocked" and not i["blockers"]]
    taken = [i for i in open_items if i["status"] in TAKEN]

    print("## Pick one per lane\n")
    if not startable:
        print("- nothing startable")
    else:
        print("| Lane | Pick | Rank | Priority | Toward | Gates | Status | Title | Then | Shares |")
        print("| --- | --- | --- | --- | --- | --- | --- | --- | --- | --- |")
    for number, lane in enumerate(lanes(startable), 1):
        head, rest = lane["items"][0], lane["items"][1:]
        then = ", ".join(f"{i['id']} ({i['rank']})" for i in rest) or "-"
        shares = ", ".join(sorted(lane["sets"])) or "nothing"
        print(f"| {number} | {head['id']} | {head['rank']} | {head['priority']} | {head['toward']} | {head['gates']} | {head['status']} | {head['title']} | {then} | {shares} |")

    print("\n## Startable\n")
    print("| ID | Rank | Priority | Toward | Gates | Status | Title | Description |")
    print("| --- | --- | --- | --- | --- | --- | --- | --- |")
    for i in startable:
        print(f"| {i['id']} | {i['rank']} | {i['priority']} | {i['toward']} | {i['gates']} | {i['status']} | {i['title']} | {describe(i['summary'], args.width)} |")

    print("\n## Blocked at or above that rank\n")
    for i in blocked or []:
        print(f"- {i['id']} ({i['priority']} {i['rank']}) {i['title']} - waits on {', '.join(i['blockers'])}")
    if not blocked:
        print("- none")

    print("\n## Blocked by status\n")
    for i in held:
        print(f"- {i['id']} ({i['priority']} {i['rank']}) {i['title']}")
    if not held:
        print("- none")

    print("\n## Taken\n")
    for i in taken:
        print(f"- {i['id']} ({i['status']}) {i['title']}")
    if not taken:
        print("- none")


if __name__ == "__main__":
    main()
