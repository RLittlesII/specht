namespace Specht.Analysis.Tests.TestEvidence;

internal sealed class TestEvidenceFreshnessCases : TheoryData<string, string, string, string, string, string, bool>
{
    public TestEvidenceFreshnessCases()
    {
        Add("test/Example.Tests.csproj", "net10.0", TestEvidenceJson.SourceFingerprint,
            TestEvidenceJson.AssemblyHash, TestEvidenceJson.ConfigurationFingerprint, TestEvidenceJson.PolicyFingerprint, true);
        Add("test/Other.Tests.csproj", "net10.0", TestEvidenceJson.SourceFingerprint,
            TestEvidenceJson.AssemblyHash, TestEvidenceJson.ConfigurationFingerprint, TestEvidenceJson.PolicyFingerprint, false);
        Add("test/Example.Tests.csproj", "net11.0", TestEvidenceJson.SourceFingerprint,
            TestEvidenceJson.AssemblyHash, TestEvidenceJson.ConfigurationFingerprint, TestEvidenceJson.PolicyFingerprint, false);
        Add("test/Example.Tests.csproj", "net10.0", new string('e', 64),
            TestEvidenceJson.AssemblyHash, TestEvidenceJson.ConfigurationFingerprint, TestEvidenceJson.PolicyFingerprint, false);
        Add("test/Example.Tests.csproj", "net10.0", TestEvidenceJson.SourceFingerprint,
            new string('f', 64), TestEvidenceJson.ConfigurationFingerprint, TestEvidenceJson.PolicyFingerprint, false);
        Add("test/Example.Tests.csproj", "net10.0", TestEvidenceJson.SourceFingerprint,
            TestEvidenceJson.AssemblyHash, new string('1', 64), TestEvidenceJson.PolicyFingerprint, false);
        Add("test/Example.Tests.csproj", "net10.0", TestEvidenceJson.SourceFingerprint,
            TestEvidenceJson.AssemblyHash, TestEvidenceJson.ConfigurationFingerprint, new string('2', 64), false);
    }
}
