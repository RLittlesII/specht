using System.Linq;
using System.Xml;
using System.Xml.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Specht.Analysis.Documentation;

/// <summary>Checks reserved XML well-formedness for analysis and invoked repairs.</summary>
public sealed class DocumentationXml
{
    public bool IsWellFormed(SyntaxNode node)
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
