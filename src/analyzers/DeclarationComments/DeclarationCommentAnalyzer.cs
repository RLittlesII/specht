using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;

namespace Specht.Analysis.DeclarationComments;

/// <summary>Rejects ordinary line comments on class and method declaration surfaces.</summary>
#pragma warning disable RS1004 // 0201-F8 C-2 selects CSharp declaration syntax.
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class DeclarationCommentAnalyzer : DiagnosticAnalyzer
{
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [Rule];

    public override void Initialize(AnalysisContext context)
    {
        context.EnableConcurrentExecution();
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
#pragma warning disable RSA1007 // Roslyn registration receives a callback rather than invoking a delegate.
        context.RegisterSyntaxTreeAction(Analyze);
#pragma warning restore RSA1007
    }

    private static void Analyze(SyntaxTreeAnalysisContext context)
    {
        var root = context.Tree.GetRoot(context.CancellationToken);
        var text = context.Tree.GetText(context.CancellationToken);
        var headerEnd = FileHeaderEnd(root, text);
        foreach (var declaration in root.DescendantNodes(static node =>
                     node is CompilationUnitSyntax or BaseNamespaceDeclarationSyntax or TypeDeclarationSyntax))
        {
            context.CancellationToken.ThrowIfCancellationRequested();
            var end = declaration switch
            {
                ClassDeclarationSyntax type => type.OpenBraceToken.IsKind(SyntaxKind.OpenBraceToken)
                    ? type.OpenBraceToken.SpanStart : type.SemicolonToken.SpanStart,
                MethodDeclarationSyntax { Body: { } body } => body.OpenBraceToken.SpanStart,
                MethodDeclarationSyntax { ExpressionBody: { } body } => body.ArrowToken.SpanStart,
                MethodDeclarationSyntax method => method.SemicolonToken.SpanStart,
                _ => -1,
            };
            if (end < declaration.FullSpan.Start)
            {
                continue;
            }

            var surface = TextSpan.FromBounds(declaration.FullSpan.Start, end);
            foreach (var trivia in declaration.DescendantTrivia(surface))
            {
                if (trivia.IsKind(SyntaxKind.SingleLineCommentTrivia) && trivia.SpanStart >= headerEnd && trivia.Span.End <= end)
                {
                    context.ReportDiagnostic(Diagnostic.Create(Rule, trivia.GetLocation()));
                }
            }
        }
    }

    private static int FileHeaderEnd(SyntaxNode root, SourceText text)
    {
        var commentEnd = 0;
        foreach (var trivia in root.GetLeadingTrivia())
        {
            if (trivia.IsKind(SyntaxKind.SingleLineCommentTrivia))
            {
                commentEnd = trivia.Span.End;
            }
            else if (trivia.IsKind(SyntaxKind.EndOfLineTrivia))
            {
                if (commentEnd > 0 && string.IsNullOrWhiteSpace(text.Lines.GetLineFromPosition(trivia.SpanStart).ToString()))
                {
                    return commentEnd;
                }
            }
            else if (!trivia.IsKind(SyntaxKind.WhitespaceTrivia))
            {
                break;
            }
        }

        return 0;
    }

#pragma warning disable RS2008 // The diagnostic contract is 0201-F8 § 7; package release tracking belongs to 0201-F6.
    private static readonly DiagnosticDescriptor Rule = new("SPCT0013", "Ordinary comment on a declaration surface",
        "Remove the ordinary line comment from the declaration surface; XML documentation (///) is allowed",
        "Style", DiagnosticSeverity.Warning, true,
        description: "Class and method declaration surfaces permit XML documentation but reject ordinary line comments.",
        helpLinkUri: "https://github.com/RLittlesII/specht/blob/main/src/codeanalysis/docs/SPCT0013.md");
}
