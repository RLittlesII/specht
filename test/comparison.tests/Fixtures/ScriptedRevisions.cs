using Specht.Comparison.Tests.Evidence;

namespace Specht.Comparison.Tests.Collection;

internal sealed class ScriptedRevisions(string root)
{
    public const string EngineCase = "Specht.Benchmarks.Engine.CheckBenchmarks.Check";
    public const string StartupCase = "Specht.Benchmarks.Startup.ColdStartBenchmarks.Start";

    public IReadOnlyList<ProcessRequest> Requests
    {
        get
        {
            lock (_gate) return [.. _requests];
        }
    }

    public int MostConcurrent { get; private set; }

    public Dictionary<string, string> Heads { get; } = new()
    {
        ["previous"] = new string('1', 40),
        ["current"] = new string('2', 40),
    };

    public HashSet<string> WithoutHarness { get; } = [];

    public Dictionary<(string Side, string Step), int> ExitCodes { get; } = [];

    public Dictionary<string, string[]> Listed { get; } = new()
    {
        ["previous"] = [StartupCase, EngineCase],
        ["current"] = [StartupCase, EngineCase],
    };

    public Dictionary<string, (string Name, string Json)[]> Exports { get; } = new()
    {
        ["previous"] =
        [
            ("Specht.Benchmarks.Startup.ColdStartBenchmarks-report-full.json", ExportData.CollectedFullJson),
            ("Specht.Benchmarks.Engine.CheckBenchmarks-report-full.json", ExportData.CollectedFullJson),
        ],
        ["current"] =
        [
            ("Specht.Benchmarks.Startup.ColdStartBenchmarks-report-full.json", ExportData.CollectedFullJson),
            ("Specht.Benchmarks.Engine.CheckBenchmarks-report-full.json", ExportData.CollectedFullJson),
        ],
    };

    public Func<string, string, CancellationToken, Task> During { get; set; } = static (_, _, _) => Task.CompletedTask;

    public static string SideOf(ProcessRequest request) =>
        $"{request.WorkingDirectory} {string.Join(' ', request.Arguments)}".Contains("/previous/", StringComparison.Ordinal)
            ? "previous"
            : "current";

    public async Task<ProcessOutcome> RunAsync(ProcessRequest request, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        lock (_gate)
        {
            _requests.Add(request);
            MostConcurrent = Math.Max(MostConcurrent, ++_active);
        }

        try
        {
            await Task.Yield();
            var side = SideOf(request);
            var step = StepOf(request);
            await During.Invoke(side, step, cancellationToken);
            if (ExitCodes.TryGetValue((side, step), out var exitCode))
                return new ProcessOutcome(exitCode, [$"fatal: secret-detail under {root}"]);

            var workspace = Path.Combine(root, ".artifacts", "benchmarks", "comparison", "session-1", side);
            switch (step)
            {
                case "clone":
                    Directory.CreateDirectory(Path.Combine(workspace, "source"));
                    break;
                case "checkout" when !WithoutHarness.Contains(side):
                    var harness = Path.Combine(workspace, "source", ".performance", "benchmarks");
                    Directory.CreateDirectory(harness);
                    await File.WriteAllTextAsync(Path.Combine(harness, "benchmarks.csproj"), "<Project />", cancellationToken);
                    break;
                case "rev-parse":
                    return new ProcessOutcome(0, [Heads[side]]);
                case "list":
                    return new ProcessOutcome(0, ["// Available benchmarks:", .. Listed[side]]);
                case "run":
                    var results = Path.Combine(workspace, "results", "results");
                    Directory.CreateDirectory(results);
                    foreach (var (name, json) in Exports[side])
                        await File.WriteAllTextAsync(Path.Combine(results, name), json, cancellationToken);
                    await File.WriteAllTextAsync(Path.Combine(results, "Specht.Benchmarks.Engine.CheckBenchmarks-report-github.md"), "| Method |",
                        cancellationToken);
                    Directory.CreateDirectory(Path.Combine(workspace, "home"));
                    await File.WriteAllTextAsync(Path.Combine(workspace, "home", "cache.bin"), "cache", cancellationToken);
                    Directory.CreateDirectory(Path.Combine(workspace, "temp"));
                    await File.WriteAllTextAsync(Path.Combine(workspace, "temp", "scratch.tmp"), "scratch", cancellationToken);
                    break;
            }

            return new ProcessOutcome(0, []);
        }
        finally
        {
            lock (_gate) _active--;
        }
    }

    private static string StepOf(ProcessRequest request) => (request.FileName, request.Arguments.FirstOrDefault()) switch
    {
        ("dotnet", "run") when request.Arguments.Contains("--list") => "list",
        (_, { } verb) => verb,
        _ => string.Empty,
    };

    private readonly Lock _gate = new();
    private readonly List<ProcessRequest> _requests = [];
    private int _active;
}
