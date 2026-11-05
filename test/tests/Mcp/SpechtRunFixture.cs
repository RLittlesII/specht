using System.Diagnostics;
using Rocket.Surgery.Extensions.Testing.AutoFixtures;
using Specht.Acceptance;

namespace Specht.Tests.Mcp;

/// <summary>
/// Runs <c>specht</c> through <c>dotnet</c>, the local tool lookup, in a directory over the environment of fake tools, or runs a shell
/// command that finds it on the path, and gives back what it wrote and how it ended.
/// </summary>
internal sealed class SpechtRunFixture : AutoFixtureBase<SpechtRunFixture>
{
    public SpechtRunFixture WithRoot(string root) => With(ref _root, root);

    public SpechtRunFixture WithTools(AcceptanceTool.FakeTools tools) => With(ref _tools, tools);

    public SpechtRunFixture WithArguments(params string[] arguments) => With(ref _arguments, arguments);

    public SpechtRunFixture WithShellCommand(string command) => With(ref _shellCommand, command);

    public void Deconstruct(out string stdout, out string stderr, out int exitCode)
    {
        var start = _shellCommand is null ? new ProcessStartInfo("dotnet") : new ProcessStartInfo("sh");
        start.WorkingDirectory = _root;
        foreach (var (name, value) in _tools?.Environment ?? new Dictionary<string, string>())
        {
            start.Environment[name] = value;
        }

        foreach (var argument in _shellCommand is null ? ["specht", .. _arguments] : new[] { "-c", _shellCommand })
        {
            start.ArgumentList.Add(argument);
        }

        (stdout, stderr, exitCode) = AcceptanceTool.Run(start);
    }

    private string _root = ".";

    private AcceptanceTool.FakeTools? _tools;

    private string[] _arguments = [];

    private string? _shellCommand;
}
