using System.Text.Json.Nodes;
using Specht.Model;
using Specht.Tests.Shared;

namespace Specht.Tests.Report;

/// <summary>
/// Synthetic manifests and Feature specifications whose table each manifest rejects, for 0001-F3 B-011: the manifest, the
/// specification, and the section and headers that manifest gives the table.
/// </summary>
public sealed class TableFailures : TheoryData<string, JsonObject, FeatureSpec, string, string[]>
{
    public TableFailures()
    {
        Add(
            "a matrix with another column",
            new JsonObject { ["tables"] = new JsonObject { ["matrix"] = new JsonArray("Claim ID", "Scenario", "Proof", "Status") } },
            new FeatureSpecFixture().WithDocument(SpecDocument.Parse(
                "## 9. Traceability Matrix\n\n| Claim ID | Scenario | Test | Status |\n| - | - | - | - |\n| B-001 | It | A test | Covered |\n",
                "src/sample/.spec/README.md")),
            "9. Traceability Matrix",
            ["Claim ID", "Scenario", "Proof", "Status"]);
        Add(
            "a matrix with no table",
            new JsonObject { ["tables"] = new JsonObject { ["matrix"] = new JsonArray("Claim ID", "Scenario", "Proof", "Status") } },
            new FeatureSpecFixture().WithDocument(SpecDocument.Parse("## 9. Traceability Matrix\n\nNone.\n", "src/sample/.spec/README.md")),
            "9. Traceability Matrix",
            ["Claim ID", "Scenario", "Proof", "Status"]);
        Add(
            "a matrix under the title the manifest's roles give it",
            new JsonObject
            {
                ["sections"] = new JsonArray("3. Acceptance Criteria", "9. Coverage", "12. Sign-off"),
                ["roles"] = new JsonObject { ["matrix"] = "9. Coverage" },
                ["tables"] = new JsonObject { ["matrix"] = new JsonArray("Claim ID", "Scenario", "Test", "Status") },
            },
            new FeatureSpecFixture().WithDocument(SpecDocument.Parse(
                "## 9. Coverage\n\n| Claim ID | Scenario | Status |\n| - | - | - |\n| B-001 | It | Covered |\n",
                "src/sample/.spec/README.md")),
            "9. Coverage",
            ["Claim ID", "Scenario", "Test", "Status"]);
        Add(
            "a claims table the manifest gives headers",
            new JsonObject { ["tables"] = new JsonObject { ["claims"] = new JsonArray("ID", "Claim", "Source", "Status") } },
            new FeatureSpecFixture().WithDocument(SpecDocument.Parse(
                "## 3. Acceptance Criteria\n\n| ID | Claim | Status |\n| - | - | - |\n| B-001 | It does the thing. | Active |\n",
                "src/sample/.spec/README.md")),
            "3. Acceptance Criteria",
            ["ID", "Claim", "Source", "Status"]);
    }
}
