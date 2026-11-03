import os
import subprocess
import sys
from pathlib import Path


def arguments(env):
    flags = {}
    for name in ('MIRROR_BACKFILL', 'MIRROR_DRY_RUN'):
        value = env.get(name, 'false').lower()
        if value not in ('true', 'false'):
            raise ValueError(f'{name} must be true or false')
        flags[name] = value == 'true'
    if not env.get('MIRROR_ROOT') or not env.get('MIRROR_REPOSITORY'):
        raise ValueError('root and repository must be supplied')
    result = ['--dry-run' if flags['MIRROR_DRY_RUN'] else '--apply',
              '--root', env['MIRROR_ROOT'], '--repo', env['MIRROR_REPOSITORY']]
    for name in ('before', 'after'):
        if env.get('MIRROR_' + name.upper()):
            result += ['--' + name, env['MIRROR_' + name.upper()]]
    if flags['MIRROR_BACKFILL']:
        result.append('--backfill')
    return result


def main():
    try:
        args = arguments(os.environ)
        if not os.environ.get('GH_TOKEN'):
            raise ValueError('token must be supplied')
    except ValueError as error:
        print(str(error), file=sys.stderr)
        return 2
    script = Path(__file__).resolve().parents[2] / 'mirror' / 'mirror_items.py'
    return subprocess.run([sys.executable, '-B', str(script), *args]).returncode


if __name__ == '__main__':
    sys.exit(main())
