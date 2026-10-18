namespace Specht.Analysis.Tests.Policy;

public sealed class InvalidPolicyValues : TheoryData<string, string>
{
    public InvalidPolicyValues()
    {
        Add("requiredLinks", "{\"test\":false}");
        Add("exemptions", """
            [{"target":{"feature":"cap:read","claim":"need/7"},"links":["test"],"reason":" "}]
            """);
        Add("projects", """[{"path":"../escape.csproj","targetFrameworks":["net10.0"]}]""");
        Add("projects", """[{"path":"test/tests.csproj","targetFrameworks":[]}]""");
        Add("projects", """[{"path":"test/tests.csproj","targetFrameworks":["net10.0","net10.0"]}]""");
        Add("features", """[{"feature":"invented"}]""");
        Add("features", """[{"epic":"0042","feature":"cap:read"}]""");
        Add("enforcement", """
            [{"project":"other/tests.csproj","include":["test/**/*.cs"],"exclude":[],"role":"verifies","declarationKinds":["testMethod"]}]
            """);
        Add("enforcement", """
            [{"project":"test/tests.csproj","include":["test/**/*.cs"],"exclude":[],"role":"verifies","declarationKinds":["constructor"]}]
            """);
        Add("enforcement", """
            [{"project":"test/tests.csproj","include":["test/**/*.cs"],"exclude":[],"role":"invented","declarationKinds":["method"]}]
            """);
        Add("scenarioScopes", """{"featureTag":"@{feature}{feature}"}""");
        Add("records", """
            {"adrProfiles":[{"glob":"**/*.md","owner":"repository",
                "identity":{"frontmatterKey":"id","headingLevel":1,"separator":":"},"lifecycle":{"frontmatterKey":"status"}}]}
            """);
    }
}
