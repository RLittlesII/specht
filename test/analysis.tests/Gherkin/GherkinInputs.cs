using System.Text.Json.Nodes;
using Specht.Analysis.Policy;

namespace Specht.Analysis.Tests.Gherkin;

internal sealed class GherkinInputs
{
    public const string ClaimFilePath = ".spec/features/arithmetic/arithmetic.feature";
    public const string RecordPath = "src/arithmetic/.spec/README.md";
    public const string ConfigurationPath = "config/consumer.json";

    public const string Arithmetic = """
        @spec.feature:arithmetic
        Feature: Arithmetic

          Rule: Addition returns the sum

            @REQ-FIX-001
            Scenario Outline: Add two operands
              Given <a> and <b>
              Then the sum is <sum>

              Examples:
                | a | b | sum |
                | 1 | 2 | 3   |
                | 2 | 2 | 4   |
        """;

    public List<AnalysisInput> Inputs { get; } =
    [
        new("configuration", ConfigurationPath, """
            {
              "schemaVersion": "0.2.0",
              "identifiers": { "feature": "^[a-z]+$", "claim": "^REQ-FIX-[0-9]{3}$" },
              "markers": { "approved": "approved", "withdrawn": "Withdrawn", "superseded": "Superseded" },
              "claim": { "tag": "@{claim}", "tagIgnoreCase": false },
              "codeAnalysis": {
                "mappings": {
                  "/schemaVersion": "/schemaVersion",
                  "/identifiers": "/identifiers",
                  "/markers": "/markers",
                  "/claim": "/claim"
                },
                "projects": [{ "path": "test/tests.csproj", "targetFrameworks": ["net10.0"] }],
                "features": [{ "feature": "arithmetic" }],
                "enforcement": [
                  { "project": "test/tests.csproj", "include": ["test/**/*.cs"], "exclude": [], "role": "verifies", "declarationKinds": ["testMethod"] }
                ],
                "records": {
                  "claimSource": "gherkin",
                  "claimFileSets": [{ "glob": ".spec/features/arithmetic/*.feature", "feature": "arithmetic" }],
                  "plannedTags": ["@ignore"],
                  "withdrawnTags": ["@withdrawn"],
                  "supersededTags": ["@superseded"]
                },
                "scenarioScopes": { "featureTag": "@spec.feature:{feature}" }
              }
            }
            """),
        new("schema:feature", "contracts/feature.json", """
            {"type":"object","required":["id","spec_status"],"properties":{"id":{"type":"string"},"spec_status":{"type":"string"}}}
            """),
        new("scenario", ClaimFilePath, Arithmetic),
    ];

    public GherkinInputs Merge(string patch)
    {
        var index = Inputs.FindIndex(static input => input.Kind == "configuration");
        var configuration = JsonNode.Parse(Inputs[index].Text!)!;
        Apply(configuration.AsObject(), JsonNode.Parse(patch)!.AsObject());
        Inputs[index] = Inputs[index] with { Text = configuration.ToJsonString() };
        return this;
    }

    public GherkinInputs ClaimFile(string text) => Replace("scenario", ClaimFilePath, text);

    public GherkinInputs Replace(string kind, string path, string? text)
    {
        var index = Inputs.FindIndex(input => input.Kind == kind && input.RelativePath == path);
        if (index < 0)
        {
            Inputs.Add(new AnalysisInput(kind, path, text));
        }
        else
        {
            Inputs[index] = Inputs[index] with { Text = text };
        }

        return this;
    }

    private static void Apply(JsonObject target, JsonObject patch)
    {
        foreach (var (key, value) in patch.ToArray())
        {
            if (value is null)
            {
                target.Remove(key);
            }
            else if (value is JsonObject child && target[key] is JsonObject existing)
            {
                Apply(existing, child);
            }
            else
            {
                target[key] = value.DeepClone();
            }
        }
    }
}
