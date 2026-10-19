namespace Specht.Analysis.Tests.TestEvidence;

internal sealed class InvalidTestEvidenceDocuments : TheoryData<string, string>
{
    public InvalidTestEvidenceDocuments()
    {
        var unsupportedVersion = System.Text.Json.Nodes.JsonNode.Parse(TestEvidenceJson.Valid)!;
        unsupportedVersion["formatVersion"] = 2;
        Add(unsupportedVersion.ToJsonString(), "unsupportedContract");

        var unsupportedFormat = System.Text.Json.Nodes.JsonNode.Parse(TestEvidenceJson.Valid)!;
        unsupportedFormat["format"] = "unknown";
        Add(unsupportedFormat.ToJsonString(), "unsupportedContract");

        var unknownProperty = System.Text.Json.Nodes.JsonNode.Parse(TestEvidenceJson.Valid)!;
        unknownProperty["extra"] = true;
        Add(unknownProperty.ToJsonString(), "invalidContract");

        var invalidHash = System.Text.Json.Nodes.JsonNode.Parse(TestEvidenceJson.Valid)!;
        invalidHash["assembly"]!["sha256"] = "not-a-sha256";
        Add(invalidHash.ToJsonString(), "invalidContract");

        var unknownMethod = System.Text.Json.Nodes.JsonNode.Parse(TestEvidenceJson.Valid)!;
        unknownMethod["discovery"]!["cases"]![0]!["methodSymbolId"] = "M:Missing.Method";
        Add(unknownMethod.ToJsonString(), "invalidContract");

        var conflictingResults = System.Text.Json.Nodes.JsonNode.Parse(TestEvidenceJson.Valid)!;
        conflictingResults["results"]!["cases"]!.AsArray().Add(System.Text.Json.Nodes.JsonNode.Parse("""
            { "caseId": "case-17", "outcome": "failed", "detail": "conflict" }
            """));
        Add(conflictingResults.ToJsonString(), "invalidContract");

        var absolutePath = System.Text.Json.Nodes.JsonNode.Parse(TestEvidenceJson.Valid)!;
        absolutePath["project"]!["path"] = "/tmp/Example.Tests.csproj";
        Add(absolutePath.ToJsonString(), "invalidContract");

        var mismatchedSourceFingerprint = System.Text.Json.Nodes.JsonNode.Parse(TestEvidenceJson.Valid)!;
        mismatchedSourceFingerprint["sourceFingerprint"] = new string('2', 64);
        Add(mismatchedSourceFingerprint.ToJsonString(), "invalidContract");

        var mismatchedPolicyFingerprint = System.Text.Json.Nodes.JsonNode.Parse(TestEvidenceJson.Valid)!;
        mismatchedPolicyFingerprint["policyFingerprint"] = new string('3', 64);
        Add(mismatchedPolicyFingerprint.ToJsonString(), "invalidContract");

        var duplicateBindingAttribute = TestEvidenceJson.Valid.Replace(
            "\"feature\":\"0201-F3\",", "\"feature\":\"0201-F3\",\"feature\":\"other\",");
        Add(duplicateBindingAttribute, "invalidContract");
    }
}
