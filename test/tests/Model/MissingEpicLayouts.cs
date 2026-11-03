using System.Text.Json.Nodes;

namespace Specht.Tests.Model;

/// <summary>
/// Where a specification declaring <c>F2</c> and no <c>epic</c> sits, in the features and the epics layout, and the
/// <c>expected</c> its missing-epic finding carries: the epic grammar as its form, and the path's epic segment as its
/// value in the epics layout alone (<c>0001-F5</c> B-035; OQ-27, the owner's ruling of 2026-11-03).
/// </summary>
public sealed class MissingEpicLayouts : TheoryData<string, JsonObject>
{
    public MissingEpicLayouts()
    {
        Add("src/area/.spec/README.md", new JsonObject { ["form"] = "^[0-9]{4}$" });
        Add("epics/0001-epic/F2-feature/spec.md", new JsonObject { ["form"] = "^[0-9]{4}$", ["value"] = "0001" });
    }
}
