namespace Specht.Analysis.Tests.Gherkin;

public sealed class InvalidGherkinPolicies : TheoryData<string, string>
{
    public InvalidGherkinPolicies()
    {
        const string claimTag = "Native claim input requires claim.tag, mapped as /claim.";
        const string tagLists = "Claim tag lists must hold @-led tags without whitespace.";
        const string sets = "codeAnalysis.records.claimFileSets is missing or invalid.";
        Add("""{"codeAnalysis":{"records":{"claimSource":"yaml"}}}""", "codeAnalysis.records.claimSource must be \"gherkin\".");
        Add("""{"codeAnalysis":{"mappings":{"/claim":null}}}""", claimTag);
        Add("""{"claim":{"tag":"@claim"}}""", claimTag);
        Add("""{"claim":{"tag":"@{claim}{claim}"}}""", claimTag);
        Add("""{"claim":{"tag":null}}""", claimTag);
        Add("""{"claim":{"tagIgnoreCase":"yes"}}""", "claim.tagIgnoreCase must be a Boolean.");
        Add("""{"codeAnalysis":{"records":{"plannedTags":["ignore"]}}}""", tagLists);
        Add("""{"codeAnalysis":{"records":{"withdrawnTags":["@a b"]}}}""", tagLists);
        Add("""{"codeAnalysis":{"records":{"supersededTags":["@"]}}}""", tagLists);
        Add("""{"codeAnalysis":{"records":{"claimFileSets":null}}}""", sets);
        Add("""{"codeAnalysis":{"records":{"claimFileSets":[]}}}""", sets);
        Add("""{"codeAnalysis":{"records":{"claimFileSets":[{"feature":"arithmetic"}]}}}""", sets);
        Add("""{"codeAnalysis":{"records":{"claimFileSets":[{"glob":".spec/features/arithmetic/*.feature","feature":"Not-A-Feature"}]}}}""", sets);
        Add("""{"codeAnalysis":{"records":{"claimFileSets":[{"glob":".spec/features/arithmetic/*.feature","feature":"arithmetic","epic":"0201"}]}}}""", sets);
        Add("""{"identifiers":{"epic":"^[0-9]{4}$"},"codeAnalysis":{"features":[{"epic":"0201","feature":"arithmetic"}]}}""", sets);
        Add("""
            {"codeAnalysis":{"records":{"claimFileSets":[
              {"glob":".spec/features/arithmetic/*.feature","feature":"arithmetic"},{"glob":"other/*.feature","feature":"arithmetic"}]}}}
            """,
            "Two claim file sets name one Feature.");
        Add("""{"codeAnalysis":{"records":{"claimFileSets":[{"glob":"nothing/*.feature","feature":"arithmetic"}]}}}""",
            "A claim file set matches no supplied scenario input.");
        Add("""{"codeAnalysis":{"features":[{"feature":"arithmetic"},{"feature":"billing"}]}}""",
            "A selected consumer Feature has no claim file set.");
    }
}
