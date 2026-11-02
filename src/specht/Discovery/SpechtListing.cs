namespace Specht.Discovery;

/// <summary>What discovery listed under one root, each file by its root-relative path, in the order discovery gave it.</summary>
/// <param name="Specifications">Each specification, in layout order and each layout's in ordinal order of relative path.</param>
/// <param name="Companions">The companion files beside each specification, at the same index as <paramref name="Specifications"/>.</param>
/// <param name="Items">The item files beside the specifications, in specification order.</param>
/// <param name="Epics">The epic files, in ordinal order of relative path.</param>
/// <param name="TestFiles">The declared test files, in ordinal order of relative path.</param>
/// <implements epic="0001" feature="F6" claim="B-001" />
public sealed record SpechtListing(
    IReadOnlyList<SpechtLocation> Specifications,
    IReadOnlyList<IReadOnlyList<string>> Companions,
    IReadOnlyList<string> Items,
    IReadOnlyList<string> Epics,
    IReadOnlyList<string> TestFiles);
