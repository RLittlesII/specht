namespace Specht.Tests.Manifest;

/// <summary>
/// A manifest pinned to schema <c>0.4.0</c> whose claim and scenario grammars move into heads differing only in case, which
/// are not one head where the tag case setting compares tags ordinally (<c>0001-F7</c> B-155, B-159; <c>0001-F5</c> B-094):
/// <c>claim.tagIgnoreCase</c> set to <c>false</c>, and absent, so the shipped default, <c>false</c>, applies.
/// </summary>
public sealed class CaseDistinctClaimAndScenarioGrammars : TheoryData<string>
{
    public CaseDistinctClaimAndScenarioGrammars()
    {
        Add("""{"schemaVersion":"0.4.0","claim":{"tagIgnoreCase":false},"identifiers":{"claim":"^X-[0-9]+$","scenario":"^x-[0-9]+$"}}""");
        Add("""{"schemaVersion":"0.4.0","identifiers":{"claim":"^X-[0-9]+$","scenario":"^x-[0-9]+$"}}""");
    }
}
