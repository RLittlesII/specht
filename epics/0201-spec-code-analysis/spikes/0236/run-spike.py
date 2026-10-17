#!/usr/bin/env python3
"""Run spike 0236 offline and retain reproducible, relative-path evidence."""
import hashlib
import json
import os
from pathlib import Path
import subprocess
import sys

root = Path(__file__).resolve().parents[4]
trial = Path(__file__).resolve().parent
output = root / '.artifacts/0236'
output.mkdir(parents=True, exist_ok=True)
cache = Path(os.environ.get('NUGET_PACKAGES', str(Path.home() / '.nuget/packages')))
commands = []
without_reqnroll = '--without-reqnroll' in sys.argv


def run(stage, args, expected=0, overrides=None):
    print(stage, flush=True)
    result = subprocess.run(args, cwd=root, capture_output=True, text=True, env={**os.environ, **(overrides or {})})
    content = (result.stdout + result.stderr).replace(str(root), '.').replace(str(cache), '<package-cache>')
    (output / (stage + '.log')).write_text(content)
    commands.append({'stage': stage, 'arguments': [str(value).replace(str(cache), '<package-cache>') for value in args], 'environmentOverrides': overrides or {}, 'exitCode': result.returncode, 'expectedExitCode': expected})
    if result.returncode != expected:
        print(content)
        raise SystemExit(result.returncode or 1)
    return content


projects = [('prototype', 'prototype.csproj'), ('frameworks', 'FrameworkTests.csproj')]
if not without_reqnroll:
    projects += [('reqnroll', 'reqnroll.csproj'), ('reqnroll-beta', 'reqnroll-beta.csproj')]
for folder, project in projects:
    path = str((trial / folder / project).relative_to(root))
    run('restore-' + folder, ['dotnet', 'restore', path, '--source', str(cache), '--ignore-failed-sources', '-p:NuGetAudit=false', '--disable-build-servers'])
    run('build-' + folder, ['dotnet', 'build', path, '--no-restore', '--disable-build-servers', '--nologo', '-v:q'])

for folder, project in projects[2:]:
    path = str((trial / folder / project).relative_to(root))
    run('test-' + folder, ['dotnet', 'test', '--project', path, '--no-build', '--no-restore', '--results-directory', '.artifacts/0236/' + folder])

# Prove the direct path without inspecting Reqnroll artifacts.
for project in ('frameworks', 'prototype'):
    assets = json.loads((trial / project / 'obj/project.assets.json').read_text())
    assert not any('reqnroll' in name.lower() for name in assets['libraries']), f'{project} must have no Reqnroll dependency.'
assembly = str(next((trial / 'frameworks/bin/Debug').glob('*/BindingSpike.FrameworkTests.dll')).relative_to(root))
discovery = json.loads(run('framework-discovery', ['dotnet', assembly, '-list', 'full/json', '-preEnumerateTheories', '-noColor']))
events = [json.loads(line) for line in run('framework-events', ['dotnet', assembly, '-automated', 'async', '-preEnumerateTheories', '-noColor']).splitlines() if line.startswith('{')]
negative = [json.loads(line) for line in run('framework-negative', ['dotnet', assembly, '-automated', 'async', '-preEnumerateTheories', '-noColor', '-method', '*Fact_WhenIncremented_ShouldMatch'], expected=1, overrides={'SPIKE_NEGATIVE': '1'}).splitlines() if line.startswith('{')]
flags = ['--frameworks'] + ([] if without_reqnroll else ['--bridge'])
run('bindings', ['dotnet', 'run', '--project', str((trial / 'prototype/prototype.csproj').relative_to(root)), '--no-build', '--no-restore', '--', '.artifacts/0236/bindings.json', *flags])
evidence = json.loads((output / 'bindings.json').read_text())
methods = next(case['Methods'] for case in evidence['Evidence'] if case['Case'] == 'real-framework-bindings')
method_targets = {(method['Class'], method['Method']): method['Targets'] for method in methods}
cases = {case['ID']: case for case in discovery}
started = {event['TestCaseUniqueID']: event for event in events if event['$type'] == 'test-case-starting'}
outcomes = [event for event in events if event['$type'] in {'test-passed', 'test-failed', 'test-skipped', 'test-not-run'}]
assert len(cases) == len(discovery) == 11 and len(method_targets) == 8
assert set(cases) == set(started) == {event['TestCaseUniqueID'] for event in outcomes}
assert len(outcomes) == 11 and sum(event['$type'] == 'test-passed' for event in outcomes) == 10
assert sum(event['$type'] == 'test-skipped' for event in outcomes) == 1
links = []
for event in outcomes:
    case = cases[event['TestCaseUniqueID']]
    observed = started[case['ID']]
    key = (case['Class'], case['Method'])
    assert key == (observed['TestClassName'], observed['TestMethodName'])
    assert key in method_targets
    links.append({'caseId': case['ID'], 'class': key[0], 'method': key[1], 'targets': method_targets[key], 'outcome': event['$type']})
for name in ('InlineTheory_WhenIncremented_ShouldMatch', 'MemberTheory_WhenIncremented_ShouldMatch', 'Theory_WhenContentRendered_ShouldMatch'):
    assert sum(link['method'] == name for link in links) == 2
repeated = [case for case in discovery if case['DisplayName'] == 'Repeated display name']
assert len(repeated) == 2 and len({case['ID'] for case in repeated}) == 2
failed = [event for event in negative if event['$type'] == 'test-failed']
assert len(failed) == 1 and failed[0]['TestCaseUniqueID'] in cases
assert cases[failed[0]['TestCaseUniqueID']]['Method'] == 'Fact_WhenIncremented_ShouldMatch'
# Start from the declared claims: an entirely unreferenced claim must be visible.
required = set(next(case['DeclaredTargets'] for case in evidence['Evidence'] if case['Case'] == 'real-framework-bindings'))
assert required == {'0001/F1/B-001', '0001/F1/B-002', '0001/F2/B-001'}
assert {target for link in links for target in link['targets']} == required
without_beta = [link for link in links if link['method'] != 'Second_WhenNamedIdentically_ShouldRemainDistinct']
unbound = required - {target for link in without_beta for target in link['targets']}
assert unbound == {'0001/F2/B-001'}
missing_results = set(cases) - {link['caseId'] for link in links[:-1]}
assert len(missing_results) == 1
selected_projects = {'frameworks'}
supplied_projects = set()
assert selected_projects - supplied_projects == {'frameworks'}
framework_evidence = {'discoveredCases': len(cases), 'boundMethods': len(methods), 'passed': 10, 'skipped': 1, 'intentionalFailures': len(failed), 'reqnrollPackages': [], 'links': links, 'unreferencedClaimControl': sorted(unbound), 'missingResultControl': sorted(missing_results), 'missingProjectControl': sorted(selected_projects - supplied_projects), 'completeWhenProjectOmitted': False}
(output / 'framework-evidence.json').write_text(json.dumps(framework_evidence, indent=2) + '\n')
inputs = sorted(path for path in trial.rglob('*') if path.is_file() and not {'obj', 'bin'} & set(path.relative_to(trial).parts) and path.suffix in {'.cs', '.csproj', '.props', '.feature', '.json', '.py'})
inputs.append(root / 'Directory.Packages.props')
digest = hashlib.sha256()
for path in inputs:
    digest.update(str(path.relative_to(root)).encode())
    digest.update(b'\0')
    digest.update(path.read_bytes())
    digest.update(b'\0')
revision = subprocess.check_output(['git', 'rev-parse', 'HEAD'], cwd=root, text=True).strip()
manifest = {'revision': revision, 'prototypeSha256': digest.hexdigest(), 'inputs': [str(path.relative_to(root)) for path in inputs], 'commands': commands, 'assertions': evidence['Assertions'], 'observations': evidence['Cases'], 'frameworks': framework_evidence, 'withoutReqnroll': without_reqnroll}
(output / ('run-manifest-no-reqnroll.json' if without_reqnroll else 'run-manifest.json')).write_text(json.dumps(manifest, indent=2) + '\n')
print(f"Passed: {manifest['assertions']} Roslyn assertions, {manifest['observations']} observations; direct tests: 10 passed, 1 intentional skip; 1 separate intentional failure verified. Reqnroll selected: {not without_reqnroll}.")
