"""Install shared skills and register the repository's role contracts."""

import json
from pathlib import Path
import re
import shutil
import subprocess
import sys


def main():
    if shutil.which("skillfile") is None:
        sys.exit("Install Skillfile first; see .agents/README.md.")
    root = Path(__file__).resolve().parents[1]
    registrations = {}
    for source in sorted((root / ".agents/roles").glob("*.md")):
        if source.name == "README.md":
            continue
        frontmatter = source.read_text(encoding="utf-8").split("---", 2)[1]
        name = re.search(r"^name: (.+)$", frontmatter, re.MULTILINE).group(1)
        description = re.search(r"^description: (.+)$", frontmatter, re.MULTILINE).group(1)
        instructions = (
            "Read and follow AGENTS.md, then read and follow the complete role contract at "
            f".agents/roles/{source.name}. Resolve its relative links from that source file. "
            "Read the skills the contract names before acting in this role."
        )
        registrations[name] = (
            f"name = {json.dumps(name)}\n"
            f"description = {json.dumps(description, ensure_ascii=False)}\n"
            f"developer_instructions = {json.dumps(instructions)}\n"
        )

    subprocess.run(["skillfile", "validate"], cwd=root, check=True)
    subprocess.run(["skillfile", "install", "--update"], cwd=root, check=True)
    destination = root / ".codex/agents"
    destination.mkdir(parents=True, exist_ok=True)
    for name, content in registrations.items():
        target = destination / f"{name}.toml"
        temporary = target.with_suffix(".toml.tmp")
        temporary.write_text(content, encoding="utf-8")
        temporary.replace(target)
    print(f"Registered {len(registrations)} roles in .codex/agents/.")


if __name__ == "__main__":
    try:
        main()
    except subprocess.CalledProcessError as error:
        sys.exit(error.returncode)
