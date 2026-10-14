import json, os, pathlib, platform, shutil, subprocess, sys, tempfile
source = pathlib.Path(__file__).resolve().parent
repository = source.parents[2]
original = pathlib.Path.cwd()
session = pathlib.Path(tempfile.mkdtemp(prefix='specht-0205-')).resolve()
env = os.environ.copy()
for key, name in {'DOTNET_CLI_HOME':'home','NUGET_PACKAGES':'packages','NUGET_HTTP_CACHE_PATH':'http','NUGET_SCRATCH':'scratch','TMPDIR':'temp','TEMP':'temp','TMP':'temp'}.items():
    path = session / name
    path.mkdir(exist_ok=True)
    env[key] = str(path)
env.update(DOTNET_CLI_TELEMETRY_OPTOUT='1', DOTNET_NOLOGO='1', DOTNET_ADD_GLOBAL_TOOLS_TO_PATH='0', MSBUILDDISABLENODEREUSE='1')

def clean(text):
    for path, replacement in [(str(session), '<session>'), (str(repository), '.'), (str(pathlib.Path.home()), '<home>')]:
        text = text.replace(path, replacement).replace(path.replace('\\', '/'), replacement)
    return text

def run(arguments, show=False):
    result = subprocess.run(arguments, cwd=session, env=env, text=True, stdout=subprocess.PIPE, stderr=subprocess.STDOUT, timeout=180)
    if show or result.returncode:
        print(clean(result.stdout), end='', flush=True)
    if result.returncode:
        raise RuntimeError('probe command failed: ' + str(result.returncode))
    return result.stdout.strip()
try:
    shutil.copy(repository / 'global.json', session / 'global.json')
    shutil.copytree(source, session / 'probe', ignore=shutil.ignore_patterns('bin','obj'))
    (session / 'source').mkdir()
    (session / '.config').mkdir()
    (session / '.config' / 'dotnet-tools.json').write_text('{"version":1,"isRoot":true,"tools":{}}')
    (session / 'nuget.config').write_text('<configuration><packageSources><clear/><add key="local" value="source"/></packageSources></configuration>')
    sdk = run(['dotnet','--version'])
    print(json.dumps({'sdk':sdk,'platform':platform.platform(),'machine':platform.machine()}), flush=True)
    run(['dotnet','restore','probe/Probe.csproj','--configfile','nuget.config','-p:NuGetAudit=false'])
    run(['dotnet','pack','probe/Probe.csproj','--no-restore','-c','Release','-o','source','-p:UseSharedCompilation=false'])
    run(['dotnet','tool','install','specht.memory.probe','--local','--version','1.0.0','--tool-manifest','.config/dotnet-tools.json','--configfile','nuget.config','--create-manifest-if-needed=false'])
    assembly = str(session / 'probe' / 'bin' / 'Release' / 'net10.0' / 'Probe.dll')
    run(['dotnet', assembly, 'observe', sdk], True)
    print(json.dumps({'nativeLifecycle':platform.system(),'repeats':10,'rows':['noop','held 0','held 128']}), flush=True)
    if platform.system() == 'Linux':
        for mode in [['noop'],['held','0'],['held','128']]:
            for repeat in range(10):
                print(json.dumps({'mode':mode,'repeat':repeat}), flush=True)
                run([sys.executable, str(session / 'probe' / 'trace.py')] + mode, True)
    elif platform.system() == 'Windows':
        for mode in [['noop'],['held','0'],['held','128']]:
            for repeat in range(10):
                print(json.dumps({'mode':mode,'repeat':repeat}), flush=True)
                run(['dotnet',assembly,'debug',shutil.which('dotnet')] + mode, True)
    else:
        os.chdir(session)
        for mode in [['noop'],['held','0'],['held','128']]:
            child = os.posix_spawnp('dotnet', ['dotnet','tool','run','peak-probe','--'] + mode, env)
            waited, status, usage = os.wait4(child, 0)
            print(json.dumps({'mode':mode,'launcherPid':waited,'wait4PeakBytes':usage.ru_maxrss,'scope':'launcher wait; not attributed to tool','exit':os.waitstatus_to_exitcode(status)}), flush=True)
        os.chdir(original)
        print('{"nativeLifecycle":"no accepted per-tool resident-peak mechanism on macOS"}')
finally:
    shutil.rmtree(session)
    print('{"cleanup":"session removed"}', flush=True)
