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
            self.assertEqual(personal.read_text(), "# unrelated personal registration\n")


if __name__ == "__main__":
    unittest.main()
