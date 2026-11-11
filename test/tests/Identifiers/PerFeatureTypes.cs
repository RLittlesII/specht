namespace Specht.Tests.Identifiers;

/// <summary>
/// The five per-Feature types <c>0001-F19</c> B-021 gives a default Nano ID size of 6, and B-057 and B-058 refuse a request
/// for without <c>--feature</c>.
/// </summary>
public sealed class PerFeatureTypes : TheoryData<string>
{
    /// <summary>Initializes the <c>decision</c>, <c>claim</c>, <c>constraint</c>, <c>openQuestion</c> and <c>scenario</c> rows.</summary>
    public PerFeatureTypes()
    {
        Add("decision");
        Add("claim");
        Add("constraint");
        Add("openQuestion");
        Add("scenario");
    }
}
