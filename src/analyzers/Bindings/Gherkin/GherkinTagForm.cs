using Specht.Model;

namespace Specht.Analysis.Bindings.Gherkin;

/// <summary>One reading of a tag form: a literal with exactly one placeholder, tested the way the engine tests companion tags.</summary>
public sealed record GherkinTagForm(string Form, string Placeholder, bool IgnoreCase, string? Grammar)
{
    internal GherkinClaimTagForm TagForm => Shape with { IgnoreCase = IgnoreCase, Grammar = Grammar };

    /// <summary>Returns the text the placeholder captures, or null when the tag does not match the form and the grammar.</summary>
    public string? Read(string tag) => TagForm.Read(tag);

    private GherkinClaimTagForm Shape { get; } = Validated(Form, Placeholder);

    private static GherkinClaimTagForm Validated(string form, string placeholder)
    {
        try
        {
            return new GherkinClaimTagForm(form, placeholder, false, null);
        }
        catch (GherkinClaimTextException failure)
        {
            throw new GherkinInputException(failure.RelativePath, failure.Message);
        }
    }
}
