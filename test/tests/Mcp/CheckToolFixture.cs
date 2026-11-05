using Rocket.Surgery.Extensions.Testing.AutoFixtures;
using Specht.Mcp.Features.PinnedTool;
using Specht.Mcp.Features.Reading;

namespace Specht.Tests.Mcp;

/// <summary>Builds the tool <c>check</c> over a scripted set of programs, until a test supplies its own.</summary>
internal sealed class CheckToolFixture : AutoFixtureBase<CheckToolFixture>
{
    public CheckToolFixture WithProcess(ScriptedProcessRun process) => With(ref _process, process);

    public static implicit operator CheckTool(CheckToolFixture fixture) => new(new SpechtToolFixture().WithProcess(fixture._process));

    private ScriptedProcessRun _process = new();
}
