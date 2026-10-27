namespace Specht.Tests.Manifest;

/// <summary>Declaration-sensitive capabilities and nested nulls that must retain the check's interpretation.</summary>
public sealed class EffectiveManifestDeclarations : TheoryData<string>
{
    public EffectiveManifestDeclarations()
    {
        Add("""{"schemaVersion":"0.2.0","claim":{"tag":null,"tagIgnoreCase":null,"extension":[]}}""");
        Add("""{"schemaVersion":"0.2.0","identifiers":{"epic":"^[0-9]{4}$"},"claim":{}}""");
        Add("""
            {"schemaVersion":"0.2.0", "tests":{"files":[],"declarations":[]},
             "matrix":{"scenarioTitles":true}, "owners":{}, "requiredSpecifications":[], "consumerTemplates":[]}
            """);
    }
}
