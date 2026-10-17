using System.Collections.Immutable;
using System.Text.Json.Nodes;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;

namespace BindingSpike;

public sealed record Fixture(ImmutableArray<AdditionalText> Files, string Feature, string OtherFeature, string Claim, string OtherClaim, string? Epic)
{
    public Target Alpha => new(Feature, Epic, Claim);
    public Target AlphaOther => new(Feature, Epic, OtherClaim);
    public Target Beta => new(OtherFeature, Epic, Claim);
    public Catalog Catalog => Catalog.Load(Files);

    public static Fixture Create(bool custom = false, bool epics = true)
    {
        var feature = custom ? "CAP-012" : "F1";
        var otherFeature = custom ? "CAP-013" : "F2";
        var claim = custom ? "REQ-AU-054" : "B-001";
        var otherClaim = custom ? "REQ-AU-055" : "B-002";
        var epic = epics ? (custom ? "EP-07" : "0001") : null;
        var identifiers = new JsonObject
        {
            ["feature"] = custom ? "^CAP-[0-9]{3}$" : "^F[0-9]+$",
            ["claim"] = custom ? "^REQ-[A-Z]{2}-[0-9]{3}$" : "^B-[0-9]{3}$",
        };
        if (epics) identifiers["epic"] = custom ? "^EP-[0-9]{2}$" : "^[0-9]{4}$";
        var manifest = new JsonObject { ["schemaVersion"] = "0.2.0", ["identifiers"] = identifiers };
        string Document(string id, params string[] claims) => "---\nid: \"" + id + "\"\n" +
            (epic is null ? "" : "epic: \"" + epic + "\"\n") + "---\n## 3. Acceptance Criteria\n\n| ID | Claim |\n| --- | --- |\n" +
            string.Join("\n", claims.Select(value => $"| {value} | Synthetic observable behavior. |")) + "\n";
        return new([
            new TextFile(".specht/manifest.json", manifest.ToJsonString()),
            new TextFile("src/alpha/.spec/README.md", Document(feature, claim, otherClaim)),
            new TextFile("src/beta/.spec/README.md", Document(otherFeature, claim)),
        ], feature, otherFeature, claim, otherClaim, epic);
    }

    public string Scope(Candidate candidate, bool beta = false) => candidate switch
    {
        Candidate.A => $"/// <spec path=\"src/{(beta ? "beta" : "alpha")}/.spec/README.md\" />\n",
        Candidate.B => "",
        _ => $"/// <spec feature=\"{(beta ? OtherFeature : Feature)}\"{EpicAttribute} />\n",
    };
    public string EpicAttribute => Epic is null ? "" : $" epic=\"{Epic}\"";
    public string Tag(Candidate candidate, bool beta = false, bool second = false, bool explicitMember = false) => candidate switch
    {
        Candidate.A => $"<requirement>{(second ? OtherClaim : Claim)}</requirement>",
        Candidate.B => BindingFixes.Explicit(second ? AlphaOther : beta ? Beta : Alpha),
        _ when explicitMember => BindingFixes.Explicit(second ? AlphaOther : beta ? Beta : Alpha),
        _ => $"<verifies claim=\"{(second ? OtherClaim : Claim)}\" />",
    };
    public string Class(Candidate candidate, string methods, bool beta = false, string name = "Checks") => Scope(candidate, beta) + $"public class {name}\n{{\n{methods}\n}}\n";
    public static string Method(string tag, string name = "Check") => $"/// <summary>Synthetic behavior.</summary>\n/// {tag}\n[Xunit.Fact]\npublic void {name}() {{}}\n";
    public Document Document(string source, AdhocWorkspace workspace)
    {
        var project = workspace.AddProject("SyntheticTests", LanguageNames.CSharp)
            .WithCompilationOptions(new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary))
            .WithParseOptions(new CSharpParseOptions(documentationMode: DocumentationMode.Diagnose))
            .AddMetadataReferences(References);
        var attributes = project.AddDocument("Attributes.cs", SourceText.From("namespace Xunit { public sealed class FactAttribute : System.Attribute {} }"));
        return attributes.Project.AddDocument("Bindings.cs", SourceText.From(source), filePath: "test/Bindings.cs");
    }
    public static readonly MetadataReference[] References = ((string)AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES")!).Split(Path.PathSeparator)
        .Select(path => MetadataReference.CreateFromFile(path)).ToArray();
}
