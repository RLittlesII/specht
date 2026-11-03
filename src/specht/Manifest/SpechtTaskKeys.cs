namespace Specht.Manifest;

/// <summary>
/// The frontmatter key each role a rule reads in an item is written as (B-004, decision 0010); each defaults to the name
/// the engine read before schema 0.3.0, which every older pin keeps (B-049).
/// </summary>
/// <param name="Id">The item id.</param>
/// <param name="Parent">The Feature the item belongs to.</param>
/// <param name="Type">The item's type.</param>
/// <implements epic="0001" feature="F5" claim="B-004" />
public sealed record SpechtTaskKeys(string Id = "id", string Parent = "parent", string Type = "type");
