namespace Specht.Analysis.Tests.TestEvidence;

internal sealed class InvalidTestEvidenceDocuments : TheoryData<string, string>
{
    public InvalidTestEvidenceDocuments()
    {
        foreach (var member in new[] { "categories", "owned", "selected" })
        {
            var missingMember = System.Text.Json.Nodes.JsonNode.Parse(TestEvidenceJson.VersionThree)!;
            missingMember["discovery"]!["cases"]![0]!.AsObject().Remove(member);
            Add(missingMember.ToJsonString(), "invalidContract");
        }

        var unorderedCategories = System.Text.Json.Nodes.JsonNode.Parse(TestEvidenceJson.VersionThree)!;
        unorderedCategories["discovery"]!["cases"]![0]!["categories"] = new System.Text.Json.Nodes.JsonArray("visual", "Smoke");
        Add(unorderedCategories.ToJsonString(), "invalidContract");

        var repeatedCategory = System.Text.Json.Nodes.JsonNode.Parse(TestEvidenceJson.VersionThree)!;
        repeatedCategory["discovery"]!["cases"]![0]!["categories"] = new System.Text.Json.Nodes.JsonArray("visual", "visual");
        Add(repeatedCategory.ToJsonString(), "invalidContract");

        var emptyCategory = System.Text.Json.Nodes.JsonNode.Parse(TestEvidenceJson.VersionThree)!;
        emptyCategory["discovery"]!["cases"]![0]!["categories"] = new System.Text.Json.Nodes.JsonArray("");
        Add(emptyCategory.ToJsonString(), "invalidContract");

        var resultForUnselectedCase = System.Text.Json.Nodes.JsonNode.Parse(TestEvidenceJson.VersionThree)!;
        resultForUnselectedCase["results"]!["cases"]!.AsArray().Add(System.Text.Json.Nodes.JsonNode.Parse("""
            { "caseId": "case-18", "outcome": "passed", "detail": null }
            """));
        Add(resultForUnselectedCase.ToJsonString(), "invalidContract");

        var selectedCaseWithoutResults = System.Text.Json.Nodes.JsonNode.Parse(TestEvidenceJson.VersionThree)!;
        selectedCaseWithoutResults["results"] = new System.Text.Json.Nodes.JsonObject { ["state"] = "absent" };
        Add(selectedCaseWithoutResults.ToJsonString(), "invalidContract");

        var versionOneWithOwnership = System.Text.Json.Nodes.JsonNode.Parse(TestEvidenceJson.Valid)!;
        versionOneWithOwnership["discovery"]!["cases"]![0]!["owned"] = true;
        Add(versionOneWithOwnership.ToJsonString(), "invalidContract");

        var laterVersion = System.Text.Json.Nodes.JsonNode.Parse(TestEvidenceJson.VersionThree)!;
        laterVersion["formatVersion"] = 4;
        Add(laterVersion.ToJsonString(), "unsupportedContract");

        var unsupportedVersion = System.Text.Json.Nodes.JsonNode.Parse(TestEvidenceJson.Valid)!;
        unsupportedVersion["formatVersion"] = 0;
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
