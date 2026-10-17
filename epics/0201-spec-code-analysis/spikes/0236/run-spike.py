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


def run(stage, args):
    print(stage, flush=True)
    result = subprocess.run(args, cwd=root, capture_output=True, text=True)
    content = (result.stdout + result.stderr).replace(str(root), '.').replace(str(cache), '<package-cache>')
    (output / (stage + '.log')).write_text(content)
    commands.append({'stage': stage, 'arguments': [str(value).replace(str(cache), '<package-cache>') for value in args], 'exitCode': result.returncode})
    if result.returncode:
        print(content)
        raise SystemExit(result.returncode)
    return content


for folder, project in (('prototype', 'prototype.csproj'), ('reqnroll', 'reqnroll.csproj'), ('reqnroll-beta', 'reqnroll-beta.csproj')):
    path = str((trial / folder / project).relative_to(root))
    run('restore-' + folder, ['dotnet', 'restore', path, '--source', str(cache), '--ignore-failed-sources', '-p:NuGetAudit=false', '--disable-build-servers'])
    run('build-' + folder, ['dotnet', 'build', path, '--no-restore', '--disable-build-servers', '--nologo', '-v:q'])

for folder, project in (('reqnroll', 'reqnroll.csproj'), ('reqnroll-beta', 'reqnroll-beta.csproj')):
    path = str((trial / folder / project).relative_to(root))
    run('test-' + folder, ['dotnet', 'test', '--project', path, '--no-build', '--no-restore', '--results-directory', '.artifacts/0236/' + folder])

run('bindings', ['dotnet', 'run', '--project', str((trial / 'prototype/prototype.csproj').relative_to(root)), '--no-build', '--no-restore', '--', '.artifacts/0236/bindings.json', '--bridge'])
inputs = sorted(path for path in trial.rglob('*') if path.is_file() and not {'obj', 'bin'} & set(path.relative_to(trial).parts) and path.suffix in {'.cs', '.csproj', '.props', '.feature', '.json', '.py'})
digest = hashlib.sha256()
for path in inputs:
    digest.update(str(path.relative_to(trial)).encode())
    digest.update(b'\0')
    digest.update(path.read_bytes())
    digest.update(b'\0')
revision = subprocess.check_output(['git', 'rev-parse', 'HEAD'], cwd=root, text=True).strip()
evidence = json.loads((output / 'bindings.json').read_text())
manifest = {'revision': revision, 'prototypeSha256': digest.hexdigest(), 'inputs': [str(path.relative_to(root)) for path in inputs], 'commands': commands, 'assertions': evidence['Assertions'], 'observations': evidence['Cases']}
(output / 'run-manifest.json').write_text(json.dumps(manifest, indent=2) + '\n')
print(f"Passed: {manifest['assertions']} assertions, {manifest['observations']} observations, 4 Reqnroll test invocations; evidence in .artifacts/0236/.")
