import ctypes, json, os, signal, sys
libc = ctypes.CDLL(None, use_errno=True)
libc.ptrace.restype = ctypes.c_long

def ptrace(request, pid, addr=0, data=0):
    result = libc.ptrace(request, pid, ctypes.c_void_p(addr), ctypes.c_void_p(data))
    if result == -1:
        raise OSError(ctypes.get_errno(), 'ptrace')
    return result

def emit(**record):
    print(json.dumps(record), flush=True)

pid = os.fork()
if pid == 0:
    ptrace(0, 0)
    os.kill(os.getpid(), signal.SIGSTOP)
    os.execvp('dotnet', ['dotnet', 'tool', 'run', 'peak-probe', '--'] + sys.argv[1:])
os.waitpid(pid, 0)
ptrace(0x4200, pid, data=2 | 4 | 8 | 16 | 64 | 0x100000)
ptrace(7, pid)
live = {pid}
tools = set()
observations = {}
while live:
    current, status, usage = os.wait4(-1, 0x40000000)
    if os.WIFEXITED(status) or os.WIFSIGNALED(status):
        if current in tools:
            emit(kind='reaped', pid=current, wait4PeakBytes=usage.ru_maxrss * 1024, exit=os.waitstatus_to_exitcode(status))
        live.discard(current)
        continue
    event = status >> 16
    if event in (1, 2, 3):
        child = ctypes.c_ulong()
        ptrace(0x4201, current, data=ctypes.addressof(child))
        live.add(child.value)
    if event == 4:
        command = open(f'/proc/{current}/cmdline', 'rb').read().replace(b'\0', b' ').decode()
        is_tool = 'Probe.dll' in command and 'tool run' not in command
        if is_tool:
            tools.add(current)
        emit(kind='exec', pid=current, root=pid, isTool=is_tool, command=command.replace(os.getcwd(), '<session>'))
    if event == 6 and current in tools:
        status_text = open(f'/proc/{current}/status').read().splitlines()
        peak = next((line for line in status_text if line.startswith('VmHWM:')), None)
        observations[current] = peak
        emit(kind='exit-stop', pid=current, vmHwm=peak)
    stop = os.WSTOPSIG(status)
    ptrace(7, current, data=0 if event or stop in (signal.SIGSTOP, signal.SIGTRAP) else stop)
emit(kind='coverage', identifiedTools=len(tools), observedTools=len(observations))
if len(tools) != 1 or len(observations) != 1:
    sys.exit(3)
