using Specht.Tests.Shared;

namespace Specht.Tests.Manifest.CodeAnalysis;

/// <summary>
/// Every schema version the tool ships, as the pin a consumer's manifest names (<c>codeanalysis</c> B-021, B-024): a
/// version cut later joins the rows without an edit here.
/// </summary>
public sealed class ShippedSchemaPins : TheoryData<string>
{
    public ShippedSchemaPins()
    {
        foreach (var version in EngineServices.Versions().Versions)
        {
            Add(version.Number.ToString());
        }
    }
}
