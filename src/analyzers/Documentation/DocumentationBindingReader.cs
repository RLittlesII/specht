using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Xml;
using System.Xml.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using Specht.Analysis.Bindings;

namespace Specht.Analysis.Documentation;

/// <summary>Reads reserved XML directly from declarations without inheriting or editing checked identities.</summary>
public sealed class DocumentationBindingReader
{
    public DocumentationAnalysis Read(ISymbol symbol, BindingResolver resolver, bool recognizedTest = false,
        CancellationToken cancellationToken = default)
    {
        var result = ReadDeclaration(symbol, resolver, recognizedTest, cancellationToken);
        for (var type = symbol as INamedTypeSymbol ?? symbol.ContainingType; type is not null; type = type.ContainingType)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var context = SymbolEqualityComparer.Default.Equals(type, symbol) ? result : ReadDeclaration(type, resolver, false, cancellationToken);
            var defaults = context.Elements.Where(static element => element.Role == "spec").ToArray();
            if (defaults.Length > 0)
            {
                return result with
                {
                    AuthoringDefault = defaults.All(element => element.ShapeValid && SameDefault(element, defaults[0])) ? defaults[0] : null,
                };
            }
        }

        return result;
    }

    private static DocumentationAnalysis ReadDeclaration(ISymbol symbol, BindingResolver resolver, bool recognizedTest,
        CancellationToken cancellationToken)
    {
        var elements = new List<DocumentationBinding>();
        var relationships = new List<DocumentationBinding>();
        var findings = new List<DocumentationFinding>();
        foreach (var reference in symbol.DeclaringSyntaxReferences.OrderBy(static reference => reference.SyntaxTree.FilePath, StringComparer.Ordinal)
                     .ThenBy(static reference => reference.Span.Start))
        {
            cancellationToken.ThrowIfCancellationRequested();
            var declaration = reference.GetSyntax(cancellationToken);
            if (declaration is VariableDeclaratorSyntax)
            {
                declaration = declaration.Parent!.Parent!;
            }

            foreach (var documentation in declaration.GetLeadingTrivia().Select(static trivia => trivia.GetStructure())
                         .OfType<DocumentationCommentTriviaSyntax>())
            {
                var names = new HashSet<int>();
                foreach (var node in documentation.DescendantNodes().Where(static node => node is XmlEmptyElementSyntax or XmlElementSyntax))
                {
                    var name = node is XmlEmptyElementSyntax empty ? empty.Name : ((XmlElementSyntax)node).StartTag.Name;
                    var role = name.ToString();
                    if (role is not ("spec" or "verifies" or "implements" or "follows"))
                    {
                        continue;
                    }

                    names.Add(name.SpanStart);

                    var attributes = node is XmlEmptyElementSyntax singleton ? singleton.Attributes : ((XmlElementSyntax)node).StartTag.Attributes;
                    var values = new Dictionary<string, string>(StringComparer.Ordinal);
                    var allowed = role == "spec" ? new[] { "epic", "feature" }
                        : role == "follows" ? new[] { "epic", "feature", "adr" } : new[] { "epic", "feature", "claim" };
                    var valid = WellFormed(node) && !node.ContainsDiagnostics && !node.ContainsSkippedText
                        && !node.DescendantTokens().Any(static token => token.IsMissing)
                        && (node is not XmlElementSyntax paired || paired.StartTag.Name.ToString() == paired.EndTag.Name.ToString())
                        && !node.Ancestors().OfType<XmlElementSyntax>().Any()
                        && (node is XmlEmptyElementSyntax || ((XmlElementSyntax)node).Content.Count == 0)
                        && attributes.All(attribute => attribute is XmlTextAttributeSyntax text
                            && allowed.Contains(text.Name.ToString(), StringComparer.Ordinal))
                        && (role == "spec" ? symbol is INamedTypeSymbol
                            : role != "verifies" || symbol is IMethodSymbol { MethodKind: MethodKind.Ordinary } && recognizedTest);
                    foreach (var attribute in attributes.OfType<XmlTextAttributeSyntax>())
                    {
                        var key = attribute.Name.ToString();
                        if (values.ContainsKey(key))
                        {
                            valid = false;
                        }
                        else
                        {
                            values.Add(key, string.Concat(attribute.TextTokens.Select(static token => token.ValueText)));
                        }
                    }

                    valid &= role == "follows" ? Present(values, "adr") : Present(values, "feature") && (role == "spec" || Present(values, "claim"));
                    BindingResult? resolution = null;
                    if (valid && role != "spec")
                    {
                        resolution = role == "follows"
                            ? resolver.Resolve(new AdrReference(Value(values, "epic"), Value(values, "feature"), Value(values, "adr")))
                            : resolver.Resolve(new ClaimReference(Value(values, "epic"), Value(values, "feature"), Value(values, "claim")));
                    }

                    var binding = new DocumentationBinding(symbol.GetDocumentationCommentId() ?? symbol.MetadataName, role,
                        new ReadOnlyDictionary<string, string>(values), node.GetLocation(), valid, resolution);
                    elements.Add(binding);
                    var target = string.Join(" ", values.Select(static value => value.Key + "=\"" + value.Value + "\""));
                    if (!valid || resolution?.Status == BindingResolution.Incomplete)
                    {
                        findings.Add(new DocumentationFinding("SPCT0002", name.GetLocation(), role + " " + target));
                    }
                    else if (resolution is not null && (resolution.Status != BindingResolution.Resolved || resolution.IsEligible != true))
                    {
                        var id = resolution.Status switch
                        {
                            BindingResolution.Invalid => "SPCT0003",
                            BindingResolution.Unresolved => "SPCT0004",
                            BindingResolution.Ambiguous => "SPCT0005",
                            _ => "SPCT0006",
                        };
                        var attribute = attributes.OfType<XmlTextAttributeSyntax>().FirstOrDefault(attribute =>
                            attribute.Name.ToString() == (role == "follows" ? "adr" : "claim"));
                        findings.Add(new DocumentationFinding(id, attribute?.GetLocation() ?? name.GetLocation(), target));
                    }

                    if (!binding.IsEvidence)
                    {
                        continue;
                    }

                    if (relationships.Any(other => other.Role == binding.Role && SameTarget(other.Resolution!.Target!, resolution!.Target!)))
                    {
                        findings.Add(new DocumentationFinding("SPCT0009", name.GetLocation(), role + " " + target));
                    }
                    else
                    {
                        relationships.Add(binding);
                    }
                }

                if (documentation.Content.Count > 0)
                {
                    var text = documentation.SyntaxTree.GetText(cancellationToken).ToString(documentation.FullSpan);
                    foreach (Match match in Regex.Matches(text, @"<(spec|verifies|implements|follows)\b", RegexOptions.CultureInvariant))
                    {
                        var start = documentation.FullSpan.Start + match.Index + 1;
                        if (!names.Contains(start) && !documentation.DescendantNodes().Any(node =>
                                node is XmlCDataSectionSyntax or XmlCommentSyntax && node.Span.Contains(start)))
                        {
                            findings.Add(new DocumentationFinding("SPCT0002",
                                Location.Create(documentation.SyntaxTree, new TextSpan(start, match.Groups[1].Length)),
                                text.Substring(match.Index).Split('\r', '\n')[0]));
                        }
                    }
                }
            }
        }

        var defaults = elements.Where(static element => element.Role == "spec" && element.ShapeValid).ToArray();
        if (defaults.Length > 1 && defaults.Any(element => !SameDefault(element, defaults[0])))
        {
            findings.AddRange(defaults.Select(static element => new DocumentationFinding("SPCT0008",
                Location.Create(element.Location.SourceTree!, new TextSpan(element.Location.SourceSpan.Start + 1, 4)),
                string.Join(" ", element.Attributes.Select(static attribute => attribute.Key + "=\"" + attribute.Value + "\"")))));
        }

        return new DocumentationAnalysis(elements.AsReadOnly(), relationships.AsReadOnly(), findings.AsReadOnly());
    }

    private static bool SameDefault(DocumentationBinding left, DocumentationBinding right) =>
        left.Attributes.Count == right.Attributes.Count && left.Attributes.All(attribute =>
            right.Attributes.TryGetValue(attribute.Key, out var value) && string.Equals(value, attribute.Value, StringComparison.Ordinal));

    private static bool Present(IReadOnlyDictionary<string, string> values, string key) =>
        values.TryGetValue(key, out var value) && !string.IsNullOrWhiteSpace(value);

    private static string? Value(IReadOnlyDictionary<string, string> values, string key) => values.TryGetValue(key, out var value) ? value : null;

    private static bool SameTarget(BindingTarget left, BindingTarget right) =>
        left.Kind == right.Kind && left.Epic == right.Epic && left.Feature == right.Feature && left.Id == right.Id;

    private static bool WellFormed(SyntaxNode node)
    {
        try
        {
            var clean = node.ReplaceTrivia(node.DescendantTrivia().Where(static trivia => trivia.IsKind(SyntaxKind.DocumentationCommentExteriorTrivia)),
                static (_, _) => SyntaxFactory.Whitespace(string.Empty));
            _ = XElement.Parse(clean.ToString());
            return true;
        }
        catch (XmlException)
        {
            return false;
        }
    }
}
