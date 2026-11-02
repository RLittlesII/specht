namespace Specht.Model;

/// <summary>A companion's <c>Scenario:</c> or <c>Scenario Outline:</c> and the claim tags Gherkin gives it.</summary>
/// <param name="Title">The text after the keyword, trimmed.</param>
/// <param name="Line">The keyword's one-based line.</param>
/// <param name="Tags">Its own claim tags, its Feature's, its Rule's and, for an outline, its Examples'.</param>
/// <implements epic="0001" feature="F12" claim="B-008" />
public sealed record FeatureScenario(string Title, int Line, IReadOnlyList<FeatureTag> Tags);
