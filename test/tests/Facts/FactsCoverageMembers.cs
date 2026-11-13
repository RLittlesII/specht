namespace Specht.Tests.Facts;

/// <summary>
/// A traceability row, a scenario or a test added to a claim in a facts document, each a member the published facts
/// schema refuses (<c>0132-F2</c> B-025): its name and its JSON value.
/// </summary>
public sealed class FactsCoverageMembers : TheoryData<string, string>
{
    public FactsCoverageMembers()
    {
        Add("traceability", """{ "scenario": "Crossing the zebra", "test": "ZebraTests.Crossing", "status": "Covered" }""");
        Add("scenario", "\"Crossing the zebra\"");
        Add("test", "\"ZebraTests.Crossing\"");
    }
}
