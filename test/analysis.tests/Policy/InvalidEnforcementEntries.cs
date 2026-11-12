namespace Specht.Analysis.Tests.Policy;

public sealed class InvalidEnforcementEntries : TheoryData<string, string>
{
    public InvalidEnforcementEntries()
    {
        Add("""{"project":"test/tests.csproj","include":["test/**/*.cs"],"role":"invented","declarationKinds":["method"]}""",
            "Enforcement role must be verifies, implements or follows.");
        Add("""{"project":"test/tests.csproj","include":["test/**/*.cs"],"role":"implements","declarationKinds":[]}""",
            "Enforcement declarationKinds must name at least one declaration kind.");
        Add("""{"project":"test/tests.csproj","include":["test/**/*.cs"],"role":"implements"}""",
            "Enforcement declarationKinds must name at least one declaration kind.");
        Add("""{"project":"test/tests.csproj","include":["test/**/*.cs"],"role":"implements","declarationKinds":["invented"]}""",
            "Enforcement declarationKinds names an unsupported declaration kind.");
        Add("""{"project":"test/tests.csproj","include":["test/**/*.cs"],"role":"verifies","declarationKinds":["constructor"]}""",
            "Enforcement declarationKinds may name only testMethod when role is verifies.");
        Add("""{"project":"test/tests.csproj","include":[],"role":"verifies","declarationKinds":["testMethod"]}""",
            "Enforcement include must select at least one source glob.");
        Add("""{"project":"test/tests.csproj","role":"verifies","declarationKinds":["testMethod"]}""",
            "Enforcement include must select at least one source glob.");
    }
}
