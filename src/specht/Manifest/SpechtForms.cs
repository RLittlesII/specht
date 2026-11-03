namespace Specht.Manifest;

/// <summary>
/// The identity form and the two edge forms, each literal text around the placeholders <c>{epic}</c> and <c>{id}</c>
/// (B-005, B-052, decision 0010).
/// </summary>
/// <param name="Identity">A Feature's identity, such as <c>{epic}-{id}</c>.</param>
/// <param name="LocalEdge">An edge to a Feature of the declaring epic, such as <c>{id}</c>.</param>
/// <param name="QualifiedEdge">An edge to a Feature of the epic it carries, such as <c>{epic}/{id}</c>.</param>
/// <implements epic="0001" feature="F5" claim="B-005" />
public sealed record SpechtForms(string Identity, string LocalEdge, string QualifiedEdge)
{
    /// <summary>The placeholder an epic's text stands in for.</summary>
    public const string EpicPlaceholder = "{epic}";

    /// <summary>The placeholder a Feature id's text stands in for.</summary>
    public const string IdPlaceholder = "{id}";

    /// <summary>Writes <paramref name="form"/> with each placeholder replaced by its value, in one pass.</summary>
    /// <param name="form">The form.</param>
    /// <param name="epic">The epic's text.</param>
    /// <param name="id">The Feature id's text.</param>
    /// <returns>The written form.</returns>
    /// <implements epic="0001" feature="F5" claim="B-005" />
    public static string Render(string form, string epic, string id) =>
        string.Join(id, form.Split(IdPlaceholder).Select(part => part.Replace(EpicPlaceholder, epic, StringComparison.Ordinal)));
}
