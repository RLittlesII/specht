namespace Specht.Manifest;

/// <summary>The grammar one Feature's claim ids are read with, chosen once per Feature (B-077, B-079, B-086; decisions 0022 and 0024).</summary>
/// <param name="Pattern">The regular expression a complete claim id matches.</param>
/// <param name="Prefix">The literal prefix the Feature declares, or null when the tree's claim grammar applies.</param>
/// <implements epic="0001" feature="F5" claim="B-079" />
public sealed record SpechtClaimGrammar(string Pattern, string? Prefix)
{
    /// <summary>The claim head from schema 0.4.0 (A-8); null below it.</summary>
    public string? Head { get; init; }

    /// <summary>The scenario head for the same Feature from schema 0.4.0 (A-8); null below it.</summary>
    public string? ScenarioHead { get; init; }

    /// <summary>The one head a declared prefix gives claims and scenarios, which reads the Feature as declaring none (B-100); else null.</summary>
    public string? SharedHead { get; init; }

    /// <summary>Whether the scenario head is longer than the claim head, so a tag beginning with it is a scenario's (B-095, B-101).</summary>
    public bool LongerScenarioHead => Head is not null && ScenarioHead is not null && ScenarioHead.Length > Head.Length;
}
