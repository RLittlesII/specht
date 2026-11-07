namespace Specht.Analysis.Tests.TestEvidence;

internal sealed class WrittenFormatVersions : TheoryData<string, int>
{
    public WrittenFormatVersions()
    {
        Add(TestEvidenceJson.Valid, 1);
        Add(TestEvidenceJson.VersionThree, 3);
    }
}
