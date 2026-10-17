using System.Collections.Immutable;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;
using Specht.Manifest;
using Specht.Model;
using Specht.Resources;
using Specht.Versioning;

namespace BindingSpike;

public enum Candidate { A, B, C }
public sealed record Target(string Feature, string? Epic, string Claim)
{
    public string Key => $"{Epic ?? "-"}/{Feature}/{Claim}";
}
public sealed record Spec(string Path, string Feature, string? Epic, IReadOnlySet<string> Claims);
public sealed record Finding(string Id, string Message);
public sealed record BindingResult(IReadOnlyList<Target> Targets, IReadOnlyList<Finding> Findings);
public sealed class TextFile(string relativePath, string text) : AdditionalText
{
    public override string Path => relativePath;
    public override SourceText GetText(CancellationToken cancellationToken = default) => SourceText.From(text);
}

public sealed class Catalog
{
    public required SpecStructure Structure { get; init; }
    public required Spec[] Specs { get; init; }

    public static Catalog Load(ImmutableArray<AdditionalText> files)
    {
        var manifest = files.Single(file => file.Path == ".specht/manifest.json").GetText()!.ToString();
        var structure = new SpecManifestParser(new SchemaVersions(new SpechtResources().Texts)).Parse(manifest);
        var grammar = new Regex(structure.Identifiers["claim"], RegexOptions.CultureInvariant);
        var specs = files.Where(file => file.Path.EndsWith(".md", StringComparison.Ordinal)).Select(file =>
        {
            var doc = SpecDocument.Parse(file.GetText()!.ToString(), file.Path);
            return new Spec(file.Path, doc.Frontmatter.Text("id")!, doc.Frontmatter.Text("epic"),
                new HashSet<string>(new SpecClaims(doc, structure, grammar).Ids.Keys, StringComparer.Ordinal));
        }).ToArray();
        return new Catalog { Structure = structure, Specs = specs };
    }

    public bool Valid(string kind, string id) => Regex.IsMatch(id, $"\\A(?:{Structure.Identifiers[kind]})\\z", RegexOptions.CultureInvariant);
    public Spec[] ById(string feature, string? epic) => Specs.Where(spec => spec.Feature == feature && spec.Epic == epic).ToArray();
    public Target[] AllTargets() => Specs.SelectMany(spec => spec.Claims.Select(claim => new Target(spec.Feature, spec.Epic, claim))).Distinct().OrderBy(target => target.Key, StringComparer.Ordinal).ToArray();
}

public static class BindingReader
{
    public static XElement Xml(SyntaxNode node)
    {
        var docs = node.GetLeadingTrivia().Where(trivia => trivia.GetStructure() is DocumentationCommentTriviaSyntax);
        var text = string.Join("\n", docs.SelectMany(trivia => trivia.ToFullString().Split('\n')).Select(line =>
        {
            var value = line.TrimStart();
            return value.StartsWith("///", StringComparison.Ordinal) ? value[3..] : value;
        }));
        return XElement.Parse("<documentation>" + text + "</documentation>", LoadOptions.PreserveWhitespace);
    }

    public static Spec[] Scope(MethodDeclarationSyntax method, SemanticModel model, Catalog catalog, Candidate candidate)
    {
        foreach (var type in method.Ancestors().OfType<TypeDeclarationSyntax>())
        {
            var symbol = model.GetDeclaredSymbol(type)!;
            var nodes = symbol.DeclaringSyntaxReferences.Select(reference => reference.GetSyntax()).OfType<TypeDeclarationSyntax>();
            var declarations = nodes.SelectMany(node => Xml(node).Elements("spec")).ToArray();
            if (declarations.Length == 0) continue;
            var scopes = declarations.SelectMany(declaration =>
            {
                if (candidate == Candidate.A)
                    return catalog.Specs.Where(spec => spec.Path == (string?)declaration.Attribute("path"));
                var feature = (string?)declaration.Attribute("feature") ?? "";
                var epic = (string?)declaration.Attribute("epic");
                if (!catalog.Valid("feature", feature) || (epic is not null && !catalog.Valid("epic", epic))) return [];
                return catalog.ById(feature, epic);
            }).ToArray();
            var declaredKeys = declarations.Select(declaration => candidate == Candidate.A
                ? "path:" + (string?)declaration.Attribute("path")
                : "identity:" + (string?)declaration.Attribute("epic") + "/" + (string?)declaration.Attribute("feature")).Distinct().Count();
            if (declaredKeys > 1) throw new InvalidOperationException("Conflicting type scopes across declarations.");
            return scopes.Distinct().ToArray();
        }
        return [];
    }

    public static BindingResult Read(MethodDeclarationSyntax method, SemanticModel model, Catalog catalog, Candidate candidate)
    {
        var targets = new List<Target>();
        var findings = new List<Finding>();
        try
        {
            var xml = Xml(method);
            var tags = xml.Elements(candidate == Candidate.A ? "requirement" : "verifies").ToArray();
            if (tags.Length == 0) return new([], [new("SPK001", "The test has no claim binding.")]);
            foreach (var tag in tags)
            {
                var claim = candidate == Candidate.A ? tag.Value.Trim() : (string?)tag.Attribute("claim") ?? "";
                if (!catalog.Valid("claim", claim))
                {
                    findings.Add(new("SPK003", "The claim does not match the manifest grammar."));
                    continue;
                }
                Spec[] scope;
                var feature = (string?)tag.Attribute("feature");
                var epic = (string?)tag.Attribute("epic");
                if (candidate != Candidate.A && feature is not null)
                {
                    if (!catalog.Valid("feature", feature) || (epic is not null && !catalog.Valid("epic", epic)))
                    {
                        findings.Add(new("SPK003", "The Feature or epic does not match the manifest grammar."));
                        continue;
                    }
                    scope = catalog.ById(feature, epic);
                }
                else if (candidate == Candidate.B || epic is not null)
                {
                    findings.Add(new("SPK001", "The member needs a complete explicit Feature identity."));
                    continue;
                }
                else scope = Scope(method, model, catalog, candidate);
                if (scope.Length != 1)
                {
                    findings.Add(new(scope.Length == 0 ? "SPK004" : "SPK005", "The specification scope does not resolve exactly once."));
                    continue;
                }
                if (!scope[0].Claims.Contains(claim))
                {
                    findings.Add(new("SPK004", "The selected specification does not declare this claim."));
                    continue;
                }
                var target = new Target(scope[0].Feature, scope[0].Epic, claim);
                if (targets.Contains(target)) findings.Add(new("SPK006", "The same target is bound twice."));
                else targets.Add(target);
            }
        }
        catch (XmlException) { findings.Add(new("SPK002", "The documentation is malformed XML.")); }
        catch (InvalidOperationException) { findings.Add(new("SPK007", "The containing type has conflicting scope declarations.")); }
        return new(targets, findings);
    }
}

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class BindingAnalyzer(Candidate candidate, bool requireExplicit = false) : DiagnosticAnalyzer
{
    public static readonly ImmutableArray<DiagnosticDescriptor> Rules = Enumerable.Range(1, 9).Select(index => new DiagnosticDescriptor(
        $"SPK{index:000}", "Experimental binding", "{0}", "Spike", index == 9 ? DiagnosticSeverity.Error : DiagnosticSeverity.Warning, true)).ToImmutableArray();
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => Rules;

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterCompilationStartAction(start =>
        {
            Catalog catalog;
            try { catalog = Catalog.Load(start.Options.AdditionalFiles); }
            catch (Exception error) when (error is ArgumentException or InvalidOperationException or System.Text.Json.JsonException or Specht.Manifest.SpechtManifestException)
            {
                start.RegisterCompilationEndAction(end => end.ReportDiagnostic(Diagnostic.Create(Rules[7], Location.None, "The declared analysis inputs are missing or invalid.")));
                return;
            }
            start.RegisterSyntaxNodeAction(node =>
            {
                var method = (MethodDeclarationSyntax)node.Node;
                var symbol = node.SemanticModel.GetDeclaredSymbol(method)!;
                if (!symbol.GetAttributes().Any(attribute => attribute.AttributeClass?.ToDisplayString() == "Xunit.FactAttribute")) return;
                var result = BindingReader.Read(method, node.SemanticModel, catalog, candidate);
                foreach (var finding in result.Findings)
                    node.ReportDiagnostic(Diagnostic.Create(Rules.Single(rule => rule.Id == finding.Id), method.Identifier.GetLocation(), finding.Message));
                if (candidate == Candidate.C && requireExplicit && result.Findings.Count == 0
                    && BindingReader.Xml(method).Elements("verifies").Any(tag => tag.Attribute("feature") is null))
                    node.ReportDiagnostic(Diagnostic.Create(Rules[8], method.Identifier.GetLocation(), "Persist an explicit target before treating this test as bound."));
            }, Microsoft.CodeAnalysis.CSharp.SyntaxKind.MethodDeclaration);
        });
    }
}
