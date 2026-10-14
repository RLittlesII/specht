"""Exercise the complete setup in a fresh directory with the real dependency manager."""

import hashlib
from pathlib import Path
import shutil
import subprocess
import sys
import tempfile
import tomllib
import unittest


class InstallationTests(unittest.TestCase):
    def test_skillfile_manages_only_skills(self):
        manifest = Path(__file__).resolve().parents[1] / "Skillfile"
        entries = [
            line.split()
            for line in manifest.read_text().splitlines()
            if line.strip() and not line.startswith("#") and not line.startswith("install ")
        ]
        self.assertTrue(entries)
        self.assertTrue(all(entry[1] == "skill" for entry in entries))

    def test_fresh_install_refreshes_sources_and_preserves_unrelated_agents(self):
        source = Path(__file__).resolve().parents[1]
        with tempfile.TemporaryDirectory() as directory:
            root = Path(directory)
            shutil.copytree(source / ".agents", root / ".agents")
            for name in ("Skillfile", "Skillfile.lock", "AGENTS.md", "CLAUDE.md"):
                shutil.copyfile(source / name, root / name)
            personal = root / ".codex/agents/personal.toml"
            personal.parent.mkdir(parents=True)
            personal.write_text("# unrelated personal registration\n")
            personal_markdown = root / ".claude/agents/personal.md"
            personal_markdown.parent.mkdir(parents=True)
            personal_markdown.write_text("Unrelated personal role.\n")

            def install():
                result = subprocess.run(
                    [sys.executable, str(root / ".agents/install.py")],
                    cwd=root,
                    capture_output=True,
                    text=True,
                )
                self.assertEqual(result.returncode, 0, result.stdout + result.stderr)

            def installed_files():
                return {
                    file.relative_to(root).as_posix(): hashlib.sha256(file.read_bytes()).hexdigest()
                    for folder in (".claude", ".codex")
                    for file in (root / folder).rglob("*")
                    if file.is_file()
                }

            install()
            for skill in (root / ".agents/skills").iterdir():
                for file in skill.rglob("*"):
                    if not file.is_file():
                        continue
                    for tool in (".claude", ".codex"):
                        installed = root / tool / "skills" / file.relative_to(skill.parent)
                        self.assertEqual(installed.read_bytes(), file.read_bytes())
            for role in (root / ".agents/roles").glob("*.md"):
                if role.name == "README.md":
                    continue
                self.assertEqual((root / ".claude/agents" / role.name).read_bytes(), role.read_bytes())
                agent = tomllib.loads((root / ".codex/agents" / f"{role.stem}.toml").read_text())
                self.assertEqual(agent["name"], role.stem)
                self.assertIn(f".agents/roles/{role.name}", agent["developer_instructions"])
                self.assertIn("AGENTS.md", agent["developer_instructions"])
                self.assertNotIn("model", agent)

            before = installed_files()
            install()
            self.assertEqual(installed_files(), before)
            updated = root / ".agents/skills/coding-conventions/SKILL.md"
            updated.write_text(updated.read_text() + "\nVerification refresh marker.\n")
            install()
            for tool in (".claude", ".codex"):
                self.assertEqual(
                    (root / tool / "skills/coding-conventions/SKILL.md").read_bytes(),
                    updated.read_bytes(),
                )
            role = root / ".agents/roles/implementer.md"
            role.write_text(role.read_text().replace(
                "description: Make a failing test pass", "description: Updated role guidance. Make a failing test pass"
            ))
            install()
            self.assertEqual((root / ".claude/agents/implementer.md").read_bytes(), role.read_bytes())
            agent = tomllib.loads((root / ".codex/agents/implementer.toml").read_text())
            self.assertTrue(agent["description"].startswith("Updated role guidance."))
            self.assertEqual(personal.read_text(), "# unrelated personal registration\n")
            self.assertEqual(personal_markdown.read_text(), "Unrelated personal role.\n")


if __name__ == "__main__":
    unittest.main()
