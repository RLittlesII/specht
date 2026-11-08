namespace Specht.Manifest;

/// <summary>The grammar one Feature's claim ids are read with, chosen once per Feature (B-077, B-079; decision 0022).</summary>
/// <param name="Pattern">The regular expression a complete claim id matches.</param>
/// <param name="Prefix">The literal prefix the Feature declares, or null when the manifest's <c>identifiers.claim</c> applies.</param>
/// <implements epic="0001" feature="F5" claim="B-079" />
public sealed record SpechtClaimGrammar(string Pattern, string? Prefix);
