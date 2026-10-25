using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using Specht.Analysis.Bindings;
using Specht.Analysis.Documentation;
using Specht.Analysis.Policy;

namespace Specht.Codefixes.Documentation;

internal sealed class DocumentationRepairs
{
    public async Task<IReadOnlyList<DocumentationRepair>> Read(Document document, Diagnostic diagnostic, CancellationToken cancellationToken)
    {
        var compilation = await document.Project.GetCompilationAsync(cancellationToken).ConfigureAwait(false);
        var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
        var tree = root?.SyntaxTree;
        if (compilation is null || root is null || tree is null || diagnostic.Location.SourceSpan.End > root.FullSpan.End)
        {
            return Array.Empty<DocumentationRepair>();
        }

        var inputs = new AnalysisInputAdapter().Read(document.Project.AnalyzerOptions.AdditionalFiles,
            document.Project.AnalyzerOptions.AnalyzerConfigOptionsProvider, compilation.SyntaxTrees, cancellationToken);
        if (!inputs.Available)
        {
            return Array.Empty<DocumentationRepair>();
        }

        var model = compilation.GetSemanticModel(tree);
        var declaration = root.DescendantNodes().OfType<MemberDeclarationSyntax>()
            .Where(node => node.FullSpan.Contains(diagnostic.Location.SourceSpan))
            .OrderBy(static node => node.FullSpan.Length)
            .FirstOrDefault(node => model.GetDeclaredSymbol(node, cancellationToken) is not null || node is BaseFieldDeclarationSyntax);
        var symbol = declaration is BaseFieldDeclarationSyntax field
            ? model.GetDeclaredSymbol(field.Declaration.Variables[0], cancellationToken)
            : declaration is null ? null : model.GetDeclaredSymbol(declaration, cancellationToken);
        if (symbol is null)
        {
            return Array.Empty<DocumentationRepair>();
        }

        var resolver = new BindingResolver(inputs.Catalog!, inputs.Policy!.Bindings);
        var recognition = new DocumentationTestMethods().Read(document.Project.AnalyzerOptions, compilation, cancellationToken);
        var recognized = recognition.Methods.Contains(symbol.GetDocumentationCommentId() ?? symbol.MetadataName);
        var analysis = new DocumentationBindingReader().Read(symbol, resolver, recognized, cancellationToken, !recognition.Authoritative);
        var binding = analysis.Elements.FirstOrDefault(element => element.Location.SourceTree == tree
            && element.Location.SourceSpan.Contains(diagnostic.Location.SourceSpan));
        var plans = new List<DocumentationRepair>();
        if (diagnostic.Id != "SPCT0007" && !analysis.Findings.Any(finding => finding.Id == diagnostic.Id
            && finding.Location.SourceTree == tree && finding.Location.SourceSpan == diagnostic.Location.SourceSpan))
        {
            return plans;
        }

        if (diagnostic.Id == "SPCT0009" && binding?.IsEvidence == true)
        {
            var copies = analysis.Elements.Where(element => element.IsEvidence && element.Role == binding.Role
                && element.Resolution!.Target == binding.Resolution!.Target)
                .OrderBy(element => RelativePath(document.Project, element), StringComparer.Ordinal)
                .ThenBy(static element => element.Location.SourceSpan.Start).ToArray();
            var changes = new List<(DocumentId, TextChange)>();
            foreach (var copy in copies.Skip(1))
            {
                var owner = document.Project.GetDocument(copy.Location.SourceTree!);
                if (owner is null)
                {
                    return plans;
                }

                changes.Add((owner.Id, new TextChange(copy.Location.SourceSpan, string.Empty)));
            }

            if (changes.Count > 0)
            {
                plans.Add(new DocumentationRepair("Specht.RemoveDuplicates", "Remove duplicate bindings", changes));
            }

            return plans;
        }

        string role;
        TextSpan span;
        string prefix = string.Empty;
        string suffix = string.Empty;
        if (binding is not null)
        {
            role = binding.Role;
            var node = root.FindNode(binding.Location.SourceSpan, findInsideTrivia: true, getInnermostNodeForTie: true);
            var attributes = node is XmlEmptyElementSyntax empty ? empty.Attributes
                : node is XmlElementSyntax paired ? paired.StartTag.Attributes : default;
            var allowed = role == "follows" ? new[] { "epic", "feature", "adr" } : new[] { "epic", "feature", "claim" };
            if (role is not ("verifies" or "implements" or "follows") || node.ContainsDiagnostics || node.ContainsSkippedText
                || !new DocumentationXml().IsWellFormed(node)
                || node.DescendantTokens().Any(static token => token.IsMissing)
                || node is XmlElementSyntax { Content.Count: > 0 }
                || node.Ancestors().OfType<XmlElementSyntax>().Any()
                || attributes.Any(attribute => attribute is not XmlTextAttributeSyntax text
                    || !allowed.Contains(text.Name.ToString(), StringComparer.Ordinal))
                || attributes.Count != binding.Attributes.Count
                || role == "verifies" && (symbol is not IMethodSymbol { MethodKind: MethodKind.Ordinary } || !recognized))
            {
                return plans;
            }

            span = binding.Location.SourceSpan;
            if (binding.Attributes.Count == 1 && binding.Attributes.TryGetValue("claim", out var claim)
                && analysis.AuthoringDefault is { } defaults && symbol is not INamedTypeSymbol)
            {
                var resolved = resolver.Resolve(new ClaimReference(Value(defaults, "epic"), Value(defaults, "feature"), claim));
                if (resolved.Status == BindingResolution.Resolved && resolved.IsEligible == true)
                {
                    plans.Add(new DocumentationRepair("Specht.MaterializeDefault", "Materialize binding from type default",
                        new[] { (document.Id, new TextChange(span, Xml(role, resolved.Target!))) }));
                }
            }
        }
        else if (diagnostic.Id == "SPCT0007" && declaration is not null
            && diagnostic.GetMessage().EndsWith(" requires verifies", StringComparison.Ordinal))
        {
            var options = document.Project.AnalyzerOptions.AnalyzerConfigOptionsProvider;
            if (!options.GlobalOptions.TryGetValue("build_property.SpechtProjectPath", out var project)
                || !options.GlobalOptions.TryGetValue("build_property.TargetFramework", out var framework))
            {
                return plans;
            }

            var path = tree.FilePath.Replace('\\', '/');
            if (options.GetOptions(tree).TryGetValue("specht_relative_path", out var relative))
            {
                path = relative;
            }

            var generated = path.EndsWith(".g.cs", StringComparison.OrdinalIgnoreCase)
                || path.EndsWith(".generated.cs", StringComparison.OrdinalIgnoreCase) || root.GetLeadingTrivia().ToFullString().Contains("<auto-generated");
            var selected = inputs.Policy.Select(new AnalysisDeclaration(project, framework, path, "method",
                symbol.GetDocumentationCommentId() ?? symbol.MetadataName, generated, recognized));
            if (symbol is not IMethodSymbol { MethodKind: MethodKind.Ordinary } || !recognized
                || !selected.RequiredRoles.Contains("verifies", StringComparer.Ordinal)
                || analysis.Relationships.Any(static element => element.Role == "verifies")
                || analysis.Elements.Any(static element => element.Role == "verifies"))
            {
                return plans;
            }

            role = "verifies";
            var text = await document.GetTextAsync(cancellationToken).ConfigureAwait(false);
            var line = text.Lines.GetLineFromPosition(declaration.SpanStart);
            var indentation = text.ToString(TextSpan.FromBounds(line.Start, declaration.SpanStart));
            var inline = indentation.Any(static character => !char.IsWhiteSpace(character));
            if (inline)
            {
                indentation = string.Concat(indentation.TakeWhile(static character => char.IsWhiteSpace(character)));
            }

            var newline = text.ToString().Contains("\r\n") ? "\r\n" : "\n";
            span = new TextSpan(declaration.SpanStart, 0);
            prefix = (inline ? newline + indentation : string.Empty) + "/// ";
            suffix = newline + indentation;
        }
        else
        {
            return plans;
        }

        foreach (var target in Targets(inputs.Catalog!, resolver, role))
        {
            cancellationToken.ThrowIfCancellationRequested();
            var xml = Xml(role, target);
            plans.Add(new DocumentationRepair("Specht.Select:" + role + ":" + xml,
                "Bind " + role + " to " + string.Join(" / ", new[] { target.Epic, target.Feature, target.Id }.Where(static part => part is not null)),
                new[] { (document.Id, new TextChange(span, prefix + xml + suffix)) }));
        }

        return plans;
    }

    private static IEnumerable<BindingTarget> Targets(BindingCatalog catalog, BindingResolver resolver, string role)
    {
        var results = role == "follows"
            ? catalog.Adrs.Select(adr => resolver.Resolve(new AdrReference(adr.Owner?.Epic, adr.Owner?.Feature, adr.Id)))
            : catalog.Features.SelectMany(feature => feature.Claims.Select(claim =>
                resolver.Resolve(new ClaimReference(feature.Identity.Epic, feature.Identity.Feature, claim.Id))));
        return results.Where(static result => result.Status == BindingResolution.Resolved && result.IsEligible == true)
            .Select(static result => result.Target!).Distinct().OrderBy(static target => target.RelativePath, StringComparer.Ordinal)
            .ThenBy(static target => target.Line).ThenBy(static target => target.Id, StringComparer.Ordinal);
    }

    private static string Xml(string role, BindingTarget target)
    {
        var element = new XElement(role);
        if (target.Epic is not null)
        {
            element.Add(new XAttribute("epic", target.Epic));
        }

        if (target.Feature is not null)
        {
            element.Add(new XAttribute("feature", target.Feature));
        }

        element.Add(new XAttribute(role == "follows" ? "adr" : "claim", target.Id));
        return element.ToString(SaveOptions.DisableFormatting);
    }

    private static string RelativePath(Project project, DocumentationBinding binding)
    {
        var tree = binding.Location.SourceTree!;
        return project.AnalyzerOptions.AnalyzerConfigOptionsProvider.GetOptions(tree).TryGetValue("specht_relative_path", out var relative)
            ? relative.Replace('\\', '/') : tree.FilePath.Replace('\\', '/');
    }

    private static string? Value(DocumentationBinding binding, string key) => binding.Attributes.TryGetValue(key, out var value) ? value : null;
}
