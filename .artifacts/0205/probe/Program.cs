using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Json;

if (args[0] == "noop") return 0;
if (args[0] == "pid") { Console.WriteLine(Environment.ProcessId); return 0; }
if (args[0] == "held")
{
    var bytes = new byte[int.Parse(args[1]) * 1024 * 1024];
    for (var i = 0; i < bytes.Length; i += 4096) bytes[i] = 1;
    Console.WriteLine(Environment.ProcessId);
    while (!File.Exists("release")) Thread.Sleep(1);
    GC.KeepAlive(bytes);
    return 0;
}
if (args[0] == "debug") return WindowsDebug.Run(args.Skip(1).ToArray());
if (args[0] == "observe")
{
    Console.WriteLine(JsonSerializer.Serialize(new { os = RuntimeInformation.OSDescription, sdk = args[1], runtime = Environment.Version.ToString(), arch = RuntimeInformation.ProcessArchitecture.ToString() }));
    foreach (var size in new[] { 0, 128 })
    {
        File.Delete("release");
        using var launcher = Start("held", size.ToString());
        var toolPid = int.Parse((await launcher.StandardOutput.ReadLineAsync())!);
        using var tool = Process.GetProcessById(toolPid);
        _ = tool.Handle;
        tool.Refresh(); launcher.Refresh();
        var before = Read(tool);
        var nativeBefore = OperatingSystem.IsWindows() ? NativePeak(tool.Handle) : null;
        var launcherBefore = Read(launcher);
        File.WriteAllText("release", "go");
        await launcher.WaitForExitAsync();
        tool.WaitForExit();
        tool.Refresh(); launcher.Refresh();
        Console.WriteLine(JsonSerializer.Serialize(new { mode = "held", sizeMiB = size, launcherPid = launcher.Id, toolPid, toolBefore = before, launcherBefore, toolAfter = Read(tool), launcherAfter = Read(launcher), nativeBefore, nativeAfter = OperatingSystem.IsWindows() ? NativePeak(tool.Handle) : null, exit = launcher.ExitCode }));
    }
    var retained = 0;
    var missing = 0;
    for (var i = 0; i < 30; i++)
    {
        using var launcher = Start("pid");
        var toolPid = int.Parse((await launcher.StandardOutput.ReadLineAsync())!);
        try { using var tool = Process.GetProcessById(toolPid); _ = tool.Handle; retained++; }
        catch (ArgumentException) { missing++; }
        await launcher.WaitForExitAsync();
        if (launcher.ExitCode != 0) throw new Exception("launch failed");
    }
    Console.WriteLine(JsonSerializer.Serialize(new { mode = "immediate-pid", launches = 30, retained, missing }));
    for (var i = 0; i < 10; i++) { using var launcher = Start("noop"); await launcher.WaitForExitAsync(); if (launcher.ExitCode != 0) throw new Exception("noop failed"); }
    Console.WriteLine("{\"mode\":\"no-output\",\"launches\":10,\"toolIdentity\":\"unobserved\"}");
    return 0;
}
return 2;

static Process Start(params string[] arguments)
{
    var start = new ProcessStartInfo("dotnet") { RedirectStandardOutput = true, RedirectStandardError = true };
    foreach (var arg in new[] { "tool", "run", "peak-probe", "--" }.Concat(arguments)) start.ArgumentList.Add(arg);
    return Process.Start(start)!;
}
static object Read(Process process)
{
    try { return new { status = "ok", bytes = process.PeakWorkingSet64 }; }
    catch (Exception exception) { return new { status = exception.GetType().Name, bytes = (long?)null }; }
}
static object NativePeak(IntPtr handle)
{
    var counters = new Native.Counters { cb = (uint)Marshal.SizeOf<Native.Counters>() };
    var ok = Native.GetProcessMemoryInfo(handle, ref counters, counters.cb);
    return new { ok, error = ok ? 0 : Marshal.GetLastWin32Error(), bytes = ok ? (ulong?)counters.peak : null };
}
static class Native
{
    [StructLayout(LayoutKind.Sequential)] public struct Counters { public uint cb, faults; public nuint peak, current, pp, p, np, n, page, peakPage; }
    [DllImport("psapi.dll", SetLastError = true)] public static extern bool GetProcessMemoryInfo(IntPtr handle, ref Counters counters, uint size);
}
static class WindowsDebug
{
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)] struct Startup { public uint cb; public IntPtr reserved, desktop, title; public uint x,y,xsize,ysize,xc,yc,fill,flags; public ushort show,cbReserved; public IntPtr lpReserved,stdin,stdout,stderr; }
    [StructLayout(LayoutKind.Sequential)] struct ProcessInfo { public IntPtr process,thread; public uint pid,tid; }
    [StructLayout(LayoutKind.Explicit, Size = 176)] struct DebugEvent { [FieldOffset(0)] public uint code; [FieldOffset(4)] public uint pid; [FieldOffset(8)] public uint tid; [FieldOffset(16)] public IntPtr file; [FieldOffset(24)] public IntPtr process; [FieldOffset(32)] public IntPtr thread; [FieldOffset(16)] public uint exitCode; }
    [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)] static extern bool CreateProcess(string app, StringBuilder command, IntPtr pa, IntPtr ta, bool inherit, uint flags, IntPtr env, string cwd, ref Startup startup, out ProcessInfo info);
    [DllImport("kernel32.dll", SetLastError = true)] static extern bool WaitForDebugEvent(out DebugEvent evt, uint timeout);
    [DllImport("kernel32.dll", SetLastError = true)] static extern bool ContinueDebugEvent(uint pid, uint tid, uint status);
    [DllImport("kernel32.dll")] static extern bool CloseHandle(IntPtr handle);
    [DllImport("ntdll.dll")] static extern int NtQueryInformationProcess(IntPtr process, int kind, IntPtr buffer, int length, out int needed);
    [StructLayout(LayoutKind.Sequential)] struct UnicodeString { public ushort length,maximum; public IntPtr buffer; }
    static string CommandLine(IntPtr handle)
    {
        NtQueryInformationProcess(handle, 60, IntPtr.Zero, 0, out var needed);
        if (needed <= 0) return "<unavailable>";
        var buffer = Marshal.AllocHGlobal(needed);
        try { if (NtQueryInformationProcess(handle, 60, buffer, needed, out _) != 0) return "<unavailable>"; var text = Marshal.PtrToStructure<UnicodeString>(buffer); return Marshal.PtrToStringUni(text.buffer, text.length / 2)!; }
        finally { Marshal.FreeHGlobal(buffer); }
    }
    public static int Run(string[] args)
    {
        var host = args[0];
        var startup = new Startup { cb = (uint)Marshal.SizeOf<Startup>() };
        if (!CreateProcess(host, new StringBuilder($"\"{host}\" tool run peak-probe -- {string.Join(' ', args.Skip(1))}"), IntPtr.Zero, IntPtr.Zero, false, 1, IntPtr.Zero, Environment.CurrentDirectory, ref startup, out var root)) throw new Exception("CreateProcess " + Marshal.GetLastWin32Error());
        CloseHandle(root.process); CloseHandle(root.thread);
        var handles = new Dictionary<uint, IntPtr>();
        var tools = new HashSet<uint>();
        var count = 0;
        while (true)
        {
            if (!WaitForDebugEvent(out var evt, 30000)) throw new Exception("WaitForDebugEvent " + Marshal.GetLastWin32Error());
            if (evt.code == 3)
            {
                handles.Add(evt.pid, evt.process);
                var command = CommandLine(evt.process);
                var isTool = command.Contains("Probe.dll", StringComparison.OrdinalIgnoreCase) && !command.Contains("tool run");
                if (isTool) tools.Add(evt.pid);
                Console.WriteLine(JsonSerializer.Serialize(new { kind = "create", pid = evt.pid, root = root.pid, isTool, command = command.Replace(Environment.CurrentDirectory, "<session>") }));
                if (evt.file != IntPtr.Zero) CloseHandle(evt.file);
            }
            if (evt.code == 6 && evt.file != IntPtr.Zero) CloseHandle(evt.file);
            if (evt.code == 5)
            {
                var counters = new Native.Counters { cb = (uint)Marshal.SizeOf<Native.Counters>() };
                var ok = Native.GetProcessMemoryInfo(handles[evt.pid], ref counters, counters.cb);
                if (tools.Contains(evt.pid)) count++;
                Console.WriteLine(JsonSerializer.Serialize(new { kind = "exit", pid = evt.pid, isTool = tools.Contains(evt.pid), ok, error = ok ? 0 : Marshal.GetLastWin32Error(), peakBytes = (ulong)counters.peak, exit = evt.exitCode }));
            }
            if (!ContinueDebugEvent(evt.pid, evt.tid, 0x00010002)) throw new Exception("continue failed");
            if (evt.code == 5) handles.Remove(evt.pid);
            if (handles.Count == 0) break;
        }
        Console.WriteLine(JsonSerializer.Serialize(new { kind = "coverage", identifiedTools = count }));
        return count == 1 ? 0 : 3;
    }
}
