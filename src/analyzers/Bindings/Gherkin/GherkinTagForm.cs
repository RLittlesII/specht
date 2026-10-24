using System;
using System.Text.RegularExpressions;

namespace Specht.Analysis.Bindings.Gherkin;

/// <summary>One reading of a tag form: a literal with exactly one placeholder, tested the way the engine tests companion tags.</summary>
public sealed record GherkinTagForm(string Form, string Placeholder, bool IgnoreCase, string? Grammar)
{
    /// <summary>Returns the text the placeholder captures, or null when the tag does not match the form and the grammar.</summary>
    public string? Read(string tag)
    {
        if (tag.Length < _prefix.Length + _suffix.Length)
        {
            return null;
        }

        var comparison = IgnoreCase ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal;
        if (!tag.StartsWith(_prefix, comparison) || !tag.EndsWith(_suffix, comparison))
        {
            return null;
        }

        var captured = tag.Substring(_prefix.Length, tag.Length - _prefix.Length - _suffix.Length);
        return Grammar is null || Regex.IsMatch(captured, "\\A(?:" + Grammar + ")\\z",
            RegexOptions.CultureInvariant | (IgnoreCase ? RegexOptions.IgnoreCase : RegexOptions.None)) ? captured : null;
    }

    private static string Part(string form, string placeholder, bool before)
    {
        var marker = "{" + placeholder + "}";
        var index = form.IndexOf(marker, StringComparison.Ordinal);
        if (index < 0 || form.IndexOf(marker, index + marker.Length, StringComparison.Ordinal) >= 0)
        {
            throw new GherkinInputException(null, "Native claim input requires claim.tag, mapped as /claim.");
        }

        return before ? form.Substring(0, index) : form.Substring(index + marker.Length);
    }

    private readonly string _prefix = Part(Form, Placeholder, true);
    private readonly string _suffix = Part(Form, Placeholder, false);
}
