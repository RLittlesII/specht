using System.Text.Json.Nodes;

namespace Specht.Analysis.Tests.TestEvidence;

internal sealed class InvalidCatalogDocuments : TheoryData<string, string>
{
    public InvalidCatalogDocuments()
    {
        Add(TestEvidenceJson.WithCatalog(TestEvidenceJson.Valid, 1), "invalidContract");
        Add(Edit(static catalog => catalog["extra"] = true), "invalidContract");
        Add(Edit(static catalog => Feature(catalog)["extra"] = true), "invalidContract");
        Add(Edit(static catalog => Claim(catalog)["extra"] = true), "invalidContract");
        Add(Edit(static catalog => Claim(catalog)["cases"]![0]!["extra"] = true), "invalidContract");
        Add(Edit(static catalog => catalog.Remove("features")), "invalidContract");
        Add(Edit(static catalog => Claim(catalog).Remove("element")), "invalidContract");
        Add(Edit(static catalog => Claim(catalog)["element"] = "Scenario"), "invalidContract");
        Add(Edit(static catalog => Claim(catalog)["cases"]![0]!["planned"] = "yes"), "invalidContract");
        Add(Edit(static catalog => Claim(catalog)["line"] = 0), "invalidContract");
        Add(Edit(static catalog => Claim(catalog)["path"] = "/tmp/arithmetic.feature"), "invalidContract");
        Add(Edit(static catalog => Feature(catalog)["epic"] = 7), "invalidContract");
        Add(Edit(static catalog => Feature(catalog)["feature"] = null), "invalidContract");

        var withCaseFacts = JsonNode.Parse(TestEvidenceJson.WithCatalog(TestEvidenceJson.Valid, 2))!;
        withCaseFacts["discovery"]!["cases"]![0]!["owned"] = true;
        Add(withCaseFacts.ToJsonString(), "invalidContract");
    }

    private static string Edit(Action<JsonObject> change)
    {
        var artifact = JsonNode.Parse(TestEvidenceJson.VersionTwo)!;
        change(artifact["catalog"]!.AsObject());
        return artifact.ToJsonString();
    }

    private static JsonObject Feature(JsonObject catalog) => catalog["features"]![0]!.AsObject();

    private static JsonObject Claim(JsonObject catalog) => Feature(catalog)["claims"]![0]!.AsObject();
}
