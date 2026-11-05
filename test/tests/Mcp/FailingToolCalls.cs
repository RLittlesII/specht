using Specht.Acceptance;

namespace Specht.Tests.Mcp;

/// <summary>
/// Each tool of rule text, the rule list and the manifest, the rule its call names if any, the state of the manifest ("removed",
/// "rejected" or "valid") and what the fake writes on standard error for that call.
/// </summary>
public sealed class FailingToolCalls : TheoryData<string, string?, string, string>
{
    public FailingToolCalls()
    {
        foreach (var tool in new[] { "explain", "rules", "manifest" })
        {
            var rule = tool == "explain" ? AcceptanceTool.FakeTools.KnownRule : null;
            Add(tool, rule, "removed", AcceptanceTool.FakeTools.MissingManifestMessage);
            Add(tool, rule, "rejected", AcceptanceTool.FakeTools.RejectedManifestMessage);
        }

        Add("explain", AcceptanceTool.FakeTools.UnknownRule, "valid", AcceptanceTool.FakeTools.UnknownRuleMessage(AcceptanceTool.FakeTools.UnknownRule));
    }
}
