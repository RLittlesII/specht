using System;
using System.Collections.Generic;
using System.Text;

namespace Specht.Analysis.Policy;

/// <summary>A project's required-case scope: a tag expression over case categories, or every case when none is declared.</summary>
public sealed class CaseScope
{
    public CaseScope(string? expression)
    {
        Expression = expression;
        if (expression is null)
        {
            _owns = static _ => true;
            return;
        }

        var tokens = Tokens(expression);
        var index = 0;
        _owns = Or(tokens, ref index);
        if (index != tokens.Count)
        {
            throw new FormatException(Invalid);
        }
    }

    internal const string Invalid = "Project required-case scope is not a valid tag expression.";

    public string? Expression { get; }

    public bool Owns(IEnumerable<string> categories) => _owns.Invoke(new HashSet<string>(categories, StringComparer.Ordinal));

    private static Func<ISet<string>, bool> Or(IReadOnlyList<string> tokens, ref int index)
    {
        var result = And(tokens, ref index);
        while (Next(tokens, index) == "or")
        {
            index++;
            var left = result;
            var right = And(tokens, ref index);
            result = categories => left.Invoke(categories) || right.Invoke(categories);
        }

        return result;
    }

    private static Func<ISet<string>, bool> And(IReadOnlyList<string> tokens, ref int index)
    {
        var result = Not(tokens, ref index);
        while (Next(tokens, index) == "and")
        {
            index++;
            var left = result;
            var right = Not(tokens, ref index);
            result = categories => left.Invoke(categories) && right.Invoke(categories);
        }

        return result;
    }

    private static Func<ISet<string>, bool> Not(IReadOnlyList<string> tokens, ref int index)
    {
        if (Next(tokens, index) != "not")
        {
            return Primary(tokens, ref index);
        }

        index++;
        var operand = Not(tokens, ref index);
        return categories => !operand.Invoke(categories);
    }

    private static Func<ISet<string>, bool> Primary(IReadOnlyList<string> tokens, ref int index)
    {
        var token = Next(tokens, index) ?? throw new FormatException(Invalid);
        index++;
        if (token == "(")
        {
            var inner = Or(tokens, ref index);
            if (Next(tokens, index) != ")")
            {
                throw new FormatException(Invalid);
            }

            index++;
            return inner;
        }

        if (token.Length < 2 || token[0] != '@')
        {
            throw new FormatException(Invalid);
        }

        var tag = token.Substring(1);
        return categories => categories.Contains(tag);
    }

    private static string? Next(IReadOnlyList<string> tokens, int index) => index < tokens.Count ? tokens[index] : null;

    private static List<string> Tokens(string expression)
    {
        var tokens = new List<string>();
        var current = new StringBuilder();
        foreach (var character in expression)
        {
            if (char.IsWhiteSpace(character) || character is '(' or ')')
            {
                if (current.Length > 0)
                {
                    tokens.Add(current.ToString());
                    current.Clear();
                }

                if (character is '(' or ')')
                {
                    tokens.Add(character.ToString());
                }
            }
            else
            {
                current.Append(character);
            }
        }

        if (current.Length > 0)
        {
            tokens.Add(current.ToString());
        }

        return tokens;
    }

    private readonly Func<ISet<string>, bool> _owns;
}
