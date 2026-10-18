using Specht.Analysis.Policy;

namespace Specht.Analysis.Tests.Policy;

internal sealed class AnalysisInputs
{
    public List<AnalysisInput> Inputs { get; } =
    [
        new AnalysisInputFixture().WithKind("configuration").WithRelativePath("config/consumer.json")
            .WithText( """
            {
              "revision":"0.1.0",

              "idPatterns":{"feature":"^cap:[a-z]+$","claim":"^need/[0-9]+$"},
              "claimHeading":"3. Acceptance Criteria",
              "lifecycleMarkers":{"approved":"approved","withdrawn":"Withdrawn","superseded":"Superseded"},
              "codeAnalysis":{
                "projects":[{"path":"test/tests.csproj","targetFrameworks":["net10.0"]}],
                "features":[{"feature":"cap:read"}],
                "enforcement":[{"project":"test/tests.csproj","include":["test/**/*.cs"],"exclude":["test/Shared/**"],
                  "role":"verifies","declarationKinds":["testMethod"]}],
                "records":{"adrProfiles":[{"glob":".spec/adr/*.md","owner":"repository",
                  "identity":{"headingLevel":1,"separator":": "},"lifecycle":{"section":"Status","firstWord":true}}]}
              }
            }
            """),
        new AnalysisInputFixture().WithKind("mappings").WithRelativePath("config/analysis-mappings.json")
            .WithText( """
            {"/schemaVersion":"/revision","/identifiers":"/idPatterns","/policy":"/codeAnalysis",
             "/claimsSection":"/claimHeading","/markers":"/lifecycleMarkers"}
            """),
        new AnalysisInputFixture().WithKind("schema:configuration").WithRelativePath("contracts/configuration.json")
            .WithText( "{\"type\":\"object\",\"required\":[\"revision\"]}"),
        new AnalysisInputFixture().WithKind("feature").WithRelativePath("src/read/.spec/README.md")
            .WithText( """
            ---
            id: cap:read
            spec_status: draft
            ---
            ## 3. Acceptance Criteria
            | ID | Claim | Source | Status |
            | --- | --- | --- | --- |
            | need/7 | A synthetic need | Agreement | Active |
            """),
        new AnalysisInputFixture().WithKind("adr").WithRelativePath(".spec/adr/0012.md")
            .WithText( """
            # ADR-0012: A synthetic decision
            ## Status
            Accepted, on the agreed date.
            """),
        new AnalysisInputFixture().WithKind("schema:feature").WithRelativePath("contracts/feature.json")
            .WithText( """
            {"type":"object","required":["id","spec_status"],"properties":{"id":{"type":"string"},"spec_status":{"type":"string"}}}
            """),
        new AnalysisInputFixture().WithKind("schema:adr").WithRelativePath("contracts/adr.json")
            .WithText( "{\"type\":\"string\"}"),
        new AnalysisInputFixture().WithKind("source").WithRelativePath("test/Read.cs")
            .WithText( "class Read {}"),
    ];
}
