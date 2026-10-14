namespace Specht.Model;

/// <summary>A claim's original id and line, with its grammar verdict.</summary>
/// <param name="Id">The trimmed first cell.</param>
/// <param name="Line">The one-based line.</param>
/// <param name="IsValid">Whether the claim grammar accepts the id.</param>
/// <param name="FirstDeclarationLine">The first line of a duplicated valid id, null on its first declaration.</param>
public sealed record SpecClaim(string Id, int Line, bool IsValid, int? FirstDeclarationLine);
