using System.Text.RegularExpressions;

namespace Specht.Manifest;

/// <summary>One type's effective <c>ids</c> record from schema 0.4.0, its omitted members the shipped record's (B-097, A-8).</summary>
/// <param name="Plain">The id outside a Feature that declares a claim prefix, ending with its only <c>{id}</c>.</param>
/// <param name="Form">The id in a Feature that declares a claim prefix, holding <c>{prefix}</c> once, or null.</param>
/// <param name="Id">The regular expression the text in the <c>{id}</c> place matches whole.</param>
/// <implements epic="0001" feature="F5" claim="B-086" />
public sealed record SpechtIdRecord(string Plain, string? Form, string Id)
{
    /// <summary>The placeholder a <c>form</c> holds once for the Feature's claim prefix.</summary>
    public const string PrefixPlaceholder = "{prefix}";

    /// <summary>The placeholder a <c>plain</c> and a <c>form</c> end with.</summary>
    public const string IdPlaceholder = "{id}";

    /// <summary>The rendered form without its closing <c>{id}</c>: the <c>form</c> with the prefix, where both exist, else the <c>plain</c>.</summary>
    /// <param name="prefix">The Feature's claim prefix, or null when it declares none.</param>
    /// <returns>The head.</returns>
    /// <implements epic="0001" feature="F5" claim="B-086" />
    public string Head(string? prefix) =>
        (Form is not null && prefix is not null ? Form.Replace(PrefixPlaceholder, prefix, StringComparison.Ordinal) : Plain)[..^IdPlaceholder.Length];

    /// <summary>The head as literal text, then a whole match of <see cref="Id"/>, case-sensitive.</summary>
    /// <param name="prefix">The Feature's claim prefix, or null when it declares none.</param>
    /// <returns>The grammar's pattern.</returns>
    /// <implements epic="0001" feature="F5" claim="B-086" />
    /// <implements epic="0001" feature="F5" claim="B-098" />
    public string Grammar(string? prefix) => $"^{Regex.Escape(Head(prefix))}(?:{Id})$";
}
