using Specht.Acceptance;

namespace Specht.Tests.Mcp;

/// <summary>
/// Each command line a fake can be given, the state of the manifest ("removed", "rejected" or "valid"), the exit code with which the fake
/// ends and what it writes on standard error.
/// </summary>
public sealed class FailingFakeCommands : TheoryData<string[], string, int, string>
{
    public FailingFakeCommands()
    {
        foreach (var command in new[] { new[] { "--json" }, ["rules", "--json"], ["manifest", "--json"], ["--explain", AcceptanceTool.FakeTools.KnownRule] })
        {
            Add(command, "removed", 2, AcceptanceTool.FakeTools.MissingManifestMessage);
            Add(command, "rejected", 3, AcceptanceTool.FakeTools.RejectedManifestMessage);
        }

        Add(
            ["--explain", AcceptanceTool.FakeTools.UnknownRule],
            "valid",
            4,
            AcceptanceTool.FakeTools.UnknownRuleMessage(AcceptanceTool.FakeTools.UnknownRule));
    }
}
