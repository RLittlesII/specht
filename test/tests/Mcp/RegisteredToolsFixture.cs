using Rocket.Surgery.Extensions.Testing.AutoFixtures;

namespace Specht.Tests.Mcp;

/// <summary>Builds the registered tools of the reading slice over a scripted set of programs, until a test supplies its own.</summary>
internal sealed class RegisteredToolsFixture : AutoFixtureBase<RegisteredToolsFixture>
{
    public RegisteredToolsFixture WithProcess(ScriptedProcessRun process) => With(ref _process, process);

    public static implicit operator RegisteredTools(RegisteredToolsFixture fixture) => new(fixture._process);

    private ScriptedProcessRun _process = ScriptedProcessRun.Tool(pinned: true, "0.5.0");
}
