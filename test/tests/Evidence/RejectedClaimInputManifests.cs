using Xunit;

namespace Specht.Tests.Evidence;

public sealed class RejectedClaimInputManifests : TheoryData<string, string, string?>
{
    public RejectedClaimInputManifests()
    {
        Add("codeAnalysis/records", "claimSource", "\"markdown\"");
        Add("claimTag", "tag", "\"@claim\"");
        Add("codeAnalysis/records", "claimFileSets", "[{\"feature\":\"cap:read\"}]");
        Add("codeAnalysis/records", "claimFileSets",
            "[{\"glob\":\"src/read/.spec/*.feature\",\"feature\":\"cap:read\"},{\"glob\":\"src/list/.spec/*.feature\",\"feature\":\"cap:read\"}]");
        Add("codeAnalysis/records", "claimSource", null);
    }
}
