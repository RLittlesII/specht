using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace Specht.Codefixes.Documentation;

internal sealed class DocumentationOrder : IComparer<string?>
{
    public IReadOnlyList<TextChange> Read(SourceText text, SyntaxNode root, IReadOnlyList<TextSpan> written)
    {
        var changes = new List<TextChange>();
        foreach (var comment in root.DescendantTrivia().Select(static trivia => trivia.GetStructure())
                     .OfType<DocumentationCommentTriviaSyntax>()
                     .Where(comment => written.Any(span => comment.FullSpan.OverlapsWith(span))))
        {
            foreach (var role in comment.Content.OfType<XmlEmptyElementSyntax>()
                         .Where(element => element.Name.ToString() is "verifies" or "implements" or "follows" && OwnsLine(text, element))
                         .GroupBy(static element => element.Name.ToString()))
            {
                var slots = role.ToArray();
                var sorted = slots.OrderBy(static element => Value(element, "epic"), this)
                    .ThenBy(static element => Value(element, "feature"), this)
                    .ThenBy(element => Value(element, role.Key == "follows" ? "adr" : "claim"), this).ToArray();
                for (var index = 0; index < slots.Length; index++)
                {
                    if (sorted[index] != slots[index])
                    {
                        changes.Add(new TextChange(slots[index].Span, text.ToString(sorted[index].Span)));
                    }
                }
            }
        }

        return changes;
    }

    public int Compare(string? left, string? right)
    {
        if (left is null || right is null)
        {
            return left is null ? right is null ? 0 : -1 : 1;
        }

        var leftIndex = 0;
        var rightIndex = 0;
        while (leftIndex < left.Length && rightIndex < right.Length)
        {
            if (IsDigit(left[leftIndex]) && IsDigit(right[rightIndex]))
            {
                var leftDigits = Digits(left, ref leftIndex);
                var rightDigits = Digits(right, ref rightIndex);
                var numeric = leftDigits.Length != rightDigits.Length ? leftDigits.Length.CompareTo(rightDigits.Length)
                    : string.CompareOrdinal(leftDigits, rightDigits);
                if (numeric != 0)
                {
                    return numeric;
                }
            }
            else if (left[leftIndex] != right[rightIndex])
            {
                return left[leftIndex].CompareTo(right[rightIndex]);
            }
            else
            {
                leftIndex++;
                rightIndex++;
            }
        }

        var remainder = (left.Length - leftIndex).CompareTo(right.Length - rightIndex);
        return remainder != 0 ? remainder : string.CompareOrdinal(left, right);
    }

    private static bool OwnsLine(SourceText text, XmlEmptyElementSyntax element)
    {
        var line = text.Lines.GetLineFromPosition(element.SpanStart);
        return text.ToString(TextSpan.FromBounds(line.Start, element.SpanStart)).Trim() == "///"
            && line.End >= element.Span.End
            && text.ToString(TextSpan.FromBounds(element.Span.End, line.End)).Trim().Length == 0;
    }

    private static string? Value(XmlEmptyElementSyntax element, string key) => element.Attributes.OfType<XmlTextAttributeSyntax>()
        .Where(attribute => attribute.Name.ToString() == key)
        .Select(static attribute => string.Concat(attribute.TextTokens.Select(static token => token.ValueText))).FirstOrDefault();

    private static string Digits(string value, ref int index)
    {
        var start = index;
        while (index < value.Length && IsDigit(value[index]))
        {
            index++;
        }

        return value.Substring(start, index - start).TrimStart('0');
    }

    private static bool IsDigit(char character) => character is >= '0' and <= '9';
}
