using Xunit;

namespace Specht.Tests.Evidence;

public sealed class MalformedNativeClaimKeys : TheoryData<string, string, string?>
{
    public MalformedNativeClaimKeys()
    {
        Add("codeAnalysis/records", "claimSource", "\"markdown\"");
        Add("codeAnalysis/records", "claimSource", "1");
        Add("codeAnalysis/mappings", "/claim", null);
        Add("claimTag", "tag", null);
        Add("claimTag", "tag", "7");
        Add("claimTag", "tag", "\"@claim\"");
        Add("claimTag", "tag", "\"@{claim}{claim}\"");
        Add("claimTag", "tagIgnoreCase", "\"yes\"");
        Add("codeAnalysis", "scenarioScopes", "{\"featureTag\":\"@feature\"}");
        Add("codeAnalysis", "scenarioScopes", "{\"epicTag\":\"@epic:{feature}\"}");
        Add("codeAnalysis/records", "plannedTags", "\"@ignore\"");
        Add("codeAnalysis/records", "plannedTags", "[1]");
        Add("codeAnalysis/records", "withdrawnTags", "[\"@\"]");
        Add("codeAnalysis/records", "withdrawnTags", "[\"withdrawn\"]");
        Add("codeAnalysis/records", "supersededTags", "[\"@super seded\"]");
    }
}
