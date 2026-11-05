using Rocket.Surgery.Extensions.Testing.AutoFixtures;
using Specht.Mcp.Features.Host;
using Specht.Mcp.Features.PinnedTool;

namespace Specht.Tests.Mcp;

/// <summary>Builds the tool that finds and runs <c>specht</c> over a scripted set of programs and a root, until a test supplies its own.</summary>
internal sealed class SpechtToolFixture : AutoFixtureBase<SpechtToolFixture>
{
    public SpechtToolFixture WithProcess(ScriptedProcessRun process) => With(ref _process, process);

    public SpechtToolFixture WithRoot(string root) => With(ref _root, root);

    public static implicit operator SpechtTool(SpechtToolFixture fixture) => new(fixture._process, new SessionRoot { Value = fixture._root });

    private ScriptedProcessRun _process = new();

    private string _root = "repository";
}
