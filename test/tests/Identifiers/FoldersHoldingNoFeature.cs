namespace Specht.Tests.Identifiers;

/// <summary>
/// A folder, relative to the root, that holds no Feature's <c>.spec/</c> in a tree whose only co-located Feature is
/// <c>features/one</c> (<c>0001-F19</c> B-059, B-060; A-7): the folder above that Feature, a folder that holds a file and no
/// specification, a folder that does not exist, and a Feature folder in the legacy <c>epics/</c> layout.
/// </summary>
public sealed class FoldersHoldingNoFeature : TheoryData<string>
{
    /// <summary>Initializes one row per folder.</summary>
    public FoldersHoldingNoFeature()
    {
        Add("features");
        Add("features/two");
        Add("absent");
        Add("epics/0002-epic/F1-feature");
    }
}
