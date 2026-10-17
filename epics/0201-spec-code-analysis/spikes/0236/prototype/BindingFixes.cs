using System.Collections.Immutable;
using System.Xml.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace BindingSpike;

public sealed class BindingFixes(Candidate candidate, Catalog catalog) : CodeFixProvider
{
    public override ImmutableArray<string> FixableDiagnosticIds => ["SPK001", "SPK004", "SPK009"];
    public override FixAllProvider? GetFixAllProvider() => null;

    public override async Task RegisterCodeFixesAsync(CodeFixContext context)
    {
        if (context.Diagnostics.Any(diagnostic => diagnostic.Id == "SPK009"))
        {
            context.RegisterCodeFix(CodeAction.Create("Make inherited identities explicit",
                _ => MaterializeAsync(context.Document, catalog), "materialize"), context.Diagnostics);
            return;
        }
        var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken);
        var method = root!.FindNode(context.Span).FirstAncestorOrSelf<MethodDeclarationSyntax>()!;
        var model = (await context.Document.GetSemanticModelAsync(context.CancellationToken))!;
        var xml = BindingReader.Xml(method);
        if (xml.Elements(candidate == Candidate.A ? "requirement" : "verifies").Any()) return;
        Spec[] scope = [];
        try
        {
            if (candidate != Candidate.B) scope = BindingReader.Scope(method, model, catalog, candidate);
        }
        catch (InvalidOperationException) { return; }
        if (scope.Length > 1) return;
        if (candidate == Candidate.A && scope.Length != 1) return;
        var choices = catalog.AllTargets().Where(target => scope.Length != 1 || (target.Feature == scope[0].Feature && target.Epic == scope[0].Epic));
        foreach (var target in choices)
        {
            var tag = candidate == Candidate.A ? $"<requirement>{target.Claim}</requirement>" : Explicit(target);
            context.RegisterCodeFix(CodeAction.Create("Bind " + target.Key,
                _ => Task.FromResult(Append(context.Document, root, method, tag)), target.Key), context.Diagnostics);
        }
    }

    public static async Task<Document> MaterializeAsync(Document document, Catalog catalog)
    {
        var root = (await document.GetSyntaxRootAsync())!;
        var model = (await document.GetSemanticModelAsync())!;
        var replacements = new Dictionary<MethodDeclarationSyntax, MethodDeclarationSyntax>();
        foreach (var method in root.DescendantNodes().OfType<MethodDeclarationSyntax>())
        {
            var inherited = BindingReader.Xml(method).Elements("verifies").Where(tag => tag.Attribute("feature") is null).ToArray();
            if (inherited.Length == 0) continue;
            var result = BindingReader.Read(method, model, catalog, Candidate.C);
            if (result.Findings.Count > 0) continue;
            var leading = method.GetLeadingTrivia().ToFullString();
            foreach (var tag in inherited)
            {
                var target = result.Targets.Single(value => value.Claim == (string?)tag.Attribute("claim"));
                var old = tag.ToString(SaveOptions.DisableFormatting);
                leading = leading.Replace(old, Explicit(target), StringComparison.Ordinal);
            }
            replacements.Add(method, method.WithLeadingTrivia(SyntaxFactory.ParseLeadingTrivia(leading)));
        }
        return document.WithSyntaxRoot(root.ReplaceNodes(replacements.Keys, (old, _) => replacements[old]));
    }

    public static string Explicit(Target target) => new XElement("verifies", new XAttribute("feature", target.Feature),
        target.Epic is null ? null : new XAttribute("epic", target.Epic), new XAttribute("claim", target.Claim)).ToString(SaveOptions.DisableFormatting);

    private static Document Append(Document document, SyntaxNode root, MethodDeclarationSyntax method, string tag)
    {
        var leading = method.GetLeadingTrivia().ToFullString();
        var updated = method.WithLeadingTrivia(SyntaxFactory.ParseLeadingTrivia(leading + "/// " + tag + "\n"));
        return document.WithSyntaxRoot(root.ReplaceNode(method, updated));
    }
}
