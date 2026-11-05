using System.Diagnostics;
using Rocket.Surgery.Extensions.Testing.AutoFixtures;
using Specht.Acceptance;

namespace Specht.Tests.Mcp;

/// <summary>
/// Arranges a run of <c>specht</c> through <c>dotnet</c>, the local tool lookup, in a directory over the environment of fake tools, or
/// of a shell command that finds it on the path.
/// </summary>
internal sealed class SpechtRunFixture : AutoFixtureBase<SpechtRunFixture>
{
    public SpechtRunFixture WithRoot(string root) => With(ref _root, root);

    public SpechtRunFixture WithTools(AcceptanceTool.FakeTools tools) => With(ref _tools, tools);

    public SpechtRunFixture WithArguments(params string[] arguments) => With(ref _arguments, arguments);

    public SpechtRunFixture WithShellCommand(string command) => With(ref _shellCommand, command);

    public static implicit operator ProcessStartInfo(SpechtRunFixture fixture)
    {
        var start = fixture._shellCommand is null ? new ProcessStartInfo("dotnet") : new ProcessStartInfo("sh");
        start.WorkingDirectory = fixture._root;
        foreach (var (name, value) in fixture._tools?.Environment ?? new Dictionary<string, string>())
        {
            start.Environment[name] = value;
        }

        foreach (var argument in fixture._shellCommand is null ? ["specht", .. fixture._arguments] : new[] { "-c", fixture._shellCommand })
        {
            start.ArgumentList.Add(argument);
        }

        return start;
    }

    private string _root = ".";

    private AcceptanceTool.FakeTools? _tools;

    private string[] _arguments = [];

    private string? _shellCommand;
}
