import importlib.util
import json
import os
import subprocess
import sys
import textwrap
import unittest
from pathlib import Path
from unittest.mock import patch

from support import write_tree

ACTION = Path(__file__).resolve().parents[2] / 'actions' / 'mirror'


class ActionTests(unittest.TestCase):
    def load(self):
        path = ACTION / 'run.py'
        self.assertTrue(path.exists(), 'the reusable action entry point must exist')
        spec = importlib.util.spec_from_file_location('mirror_action', path)
        module = importlib.util.module_from_spec(spec)
        spec.loader.exec_module(module)
        return module

    def test_B067_ActionInputs_WhenSelected_ShouldPassSeparateArgumentsAndWriteSummary(self):
        entry = self.load()
        arguments = entry.arguments({
            'MIRROR_ROOT': 'checkout with spaces', 'MIRROR_REPOSITORY': 'sample/consumer',
            'MIRROR_BEFORE': 'a' * 40, 'MIRROR_AFTER': 'b' * 40,
            'MIRROR_BACKFILL': 'true', 'MIRROR_DRY_RUN': 'false',
        })
        self.assertEqual(arguments, [
            '--apply', '--root', 'checkout with spaces', '--repo', 'sample/consumer',
            '--before', 'a' * 40, '--after', 'b' * 40, '--backfill',
        ])
        result, calls, summary = self.run_action(backfill='true')
        self.assertEqual(result.returncode, 0, result.stderr)
        self.assertIn('Work-item mirror', summary)
        self.assertIn('created', summary)

    def test_B067_InvalidBoolean_WhenSelected_ShouldFailBeforeStartingTheMirror(self):
        entry = self.load()
        for name in ('MIRROR_BACKFILL', 'MIRROR_DRY_RUN'):
            with self.subTest(name=name), self.assertRaises(ValueError):
                entry.arguments({'MIRROR_ROOT': '.', 'MIRROR_REPOSITORY': 'sample/consumer', name: 'yes'})

    def test_B066_SeparateCheckout_WhenActionRuns_ShouldMirrorConsumerItemsAndMilestones(self):
        result, calls, summary = self.run_action()
        self.assertEqual(result.returncode, 0, result.stderr)
        writes = [call for call in calls if call[:2] == ['issue', 'create']]
        self.assertEqual(len(writes), 1)
        self.assertIn('0041: Consumer task', writes[0])
        self.assertIn('sample/consumer', writes[0])
        self.assertIn('0.8.0', writes[0])
        self.assertTrue(any('repos/sample/consumer/milestones' in call and 'POST' in call for call in calls))
        self.assertIn('milestones', summary)
        self.assertNotIn('specht.tool', result.stdout)

    def test_B068_DryRun_WhenActionRuns_ShouldPlanWithoutWrites(self):
        result, calls, summary = self.run_action(dry_run='true')
        self.assertEqual(result.returncode, 0, result.stderr)
        self.assertIn('create_issue', result.stdout)
        self.assertIn('Consumer task', result.stdout)
        self.assertFalse(any('POST' in call or 'PATCH' in call or 'create' in call or 'edit' in call for call in calls))

    def test_B070_ScriptInput_WhenSelected_ShouldBuildAnExecutableCommandWithoutAShell(self):
        entry = self.load()
        env = {'MIRROR_ROOT': '.', 'MIRROR_REPOSITORY': 'sample/consumer',
               'GH_TOKEN': 'synthetic-token', 'MIRROR_SCRIPT': 'scripts/custom process.sh'}
        with patch.dict(entry.os.environ, env, clear=True), patch.object(entry.subprocess, 'run') as run:
            run.return_value.returncode = 17
            self.assertEqual(entry.main(), 17)
        args, options = run.call_args
        self.assertEqual(args[0], [str(Path('scripts/custom process.sh').resolve()),
                                   '--apply', '--root', str(Path('.').resolve()), '--repo', 'sample/consumer'])
        self.assertEqual(Path(options['cwd']), Path('.').resolve())
        self.assertFalse(options.get('shell', False))
        self.assertNotIn('env', options)

    def test_B070_ConsumerScript_WhenActionRuns_ShouldOwnTheProcessAndReceiveInputs(self):
        result, calls, summary = self.run_action(
            script='scripts/custom $(touch injected).sh', backfill='true', dry_run='true', relative_root=True)
        self.assertEqual(result.returncode, 0, result.stderr)
        self.assertIn('consumer process', result.stdout)
        self.assertIn('root matches cwd', result.stdout)
        self.assertIn('token inherited', result.stdout)
        self.assertIn('<--dry-run>\n<--root>\n', result.stdout)
        self.assertIn('<--repo>\n<sample/consumer>\n<--backfill>', result.stdout)
        self.assertEqual(summary, 'Consumer summary\n')
        self.assertEqual(calls, [])

    def test_B071_ProcessStartFailure_WhenSelected_ShouldReturnTwoWithoutFallback(self):
        entry = self.load()
        env = {'MIRROR_ROOT': '.', 'MIRROR_REPOSITORY': 'sample/consumer',
               'GH_TOKEN': 'synthetic-token', 'MIRROR_SCRIPT': 'missing.sh'}
        with patch.dict(entry.os.environ, env, clear=True), patch.object(entry.sys, 'stderr'), \
                patch.object(entry.subprocess, 'run', side_effect=FileNotFoundError) as run:
            self.assertEqual(entry.main(), 2)
            run.assert_called_once()

    def test_B071_ConsumerScriptFailure_WhenActionRuns_ShouldReturnItsExitCodeWithoutFallback(self):
        result, calls, summary = self.run_action(script='scripts/custom process.sh', script_exit=17)
        self.assertEqual(result.returncode, 17, result.stderr)
        self.assertIn('consumer process', result.stdout)
        self.assertEqual(summary, 'Consumer summary\n')
        self.assertEqual(calls, [])

    def test_B071_UnstartableScript_WhenActionRuns_ShouldFailWithoutFallback(self):
        for script, executable in [('missing.sh', True), ('scripts/custom process.sh', False)]:
            with self.subTest(script=script):
                result, calls, summary = self.run_action(script=script, executable=executable)
                self.assertEqual(result.returncode, 2, result.stderr)
                self.assertIn('script could not be started', result.stderr)
                self.assertNotIn('Traceback', result.stderr)
                self.assertEqual(calls, [])
                self.assertEqual(summary, '')

    def run_action(self, dry_run='false', backfill='false', script='', script_exit=0, executable=True, relative_root=False):
        root = write_tree(self, {
            'consumer checkout/.issue/0041-task.yml':
                'id: "0041"\ntype: task\ntitle: Consumer task\nstatus: ready\nrisk: 1\nparent: null\ndepends_on: []\nsummary: |\n  Consumer work.\n',
            'consumer checkout/.issue/.goal': '0.8.0\n0041\n',
            'bin/gh': '#!' + sys.executable + '\n' + textwrap.dedent('''
                import json, os, sys
                args = sys.argv[1:]
                with open(os.environ['FAKE_GH_CALLS'], 'a') as output:
                    output.write(json.dumps(args) + '\\n')
                if args[:2] == ['issue', 'create']:
                    print('https://github.com/sample/consumer/issues/1')
                elif 'POST' in args:
                    print('1')
                elif args[:2] == ['label', 'list'] or args[:2] == ['label', 'create']:
                    pass
                elif args[:2] == ['api', 'graphql'] or args[0] == 'api':
                    print('[]')
                else:
                    raise SystemExit('unexpected gh command')
            '''),
        })
        checkout = (root / 'consumer checkout').resolve()
        (root / 'bin/gh').chmod(0o755)
        for args in (['init', '-b', 'main'], ['add', '.'], ['-c', 'core.hooksPath=disabled-hooks', '-c', 'commit.gpgsign=false', '-c', 'user.name=Tester', '-c', 'user.email=tester@example.invalid', 'commit', '-m', 'Initial consumer tree']):
            subprocess.run(['git', '-C', str(checkout), *args], check=True, capture_output=True)
        if script and script != 'missing.sh':
            selected = checkout / script
            selected.parent.mkdir(parents=True, exist_ok=True)
            selected.write_text(textwrap.dedent('''\
                #!/bin/sh
                printf 'consumer process\\n'
                [ "$PWD" = "$3" ] && printf 'root matches cwd\\n'
                [ "$GH_TOKEN" = 'synthetic-token' ] && printf 'token inherited\\n'
                printf '<%s>\\n' "$@"
                printf 'Consumer summary\\n' >> "$GITHUB_STEP_SUMMARY"
                exit "$CUSTOM_SCRIPT_EXIT"
            '''))
            selected.chmod(0o755 if executable else 0o644)
        metadata = ACTION / 'action.yml'
        self.assertTrue(metadata.exists(), 'the reusable action metadata must exist')
        shell = textwrap.dedent(metadata.read_text().split('      run: |\n', 1)[1])
        env = dict(os.environ, PATH=str(root / 'bin') + os.pathsep + os.environ['PATH'],
                   MIRROR_ACTION_PATH=str(ACTION), MIRROR_ROOT='consumer checkout' if relative_root else str(checkout),
                   MIRROR_REPOSITORY='sample/consumer', MIRROR_BACKFILL=backfill,
                   MIRROR_DRY_RUN=dry_run, MIRROR_BEFORE='', MIRROR_AFTER='',
                   MIRROR_SCRIPT=script, CUSTOM_SCRIPT_EXIT=str(script_exit),
                   GH_TOKEN='synthetic-token', FAKE_GH_CALLS=str(root / 'calls.jsonl'),
                   GITHUB_STEP_SUMMARY=str(root / 'summary.md'), PYTHONDONTWRITEBYTECODE='1')
        before = {p.relative_to(checkout).as_posix(): p.read_bytes() for p in checkout.rglob('*') if p.is_file()}
        result = subprocess.run(['bash', '-e', '-o', 'pipefail', '-c', shell], cwd=root, env=env, text=True, capture_output=True)
        self.assertEqual(before, {p.relative_to(checkout).as_posix(): p.read_bytes() for p in checkout.rglob('*') if p.is_file()})
        calls = [json.loads(line) for line in (root / 'calls.jsonl').read_text().splitlines()] if (root / 'calls.jsonl').exists() else []
        summary = (root / 'summary.md').read_text() if (root / 'summary.md').exists() else ''
        return result, calls, summary
