using Specht.Analysis.Policy;
using Specht.Analysis.Tests.Policy;

namespace Specht.Analysis.Tests.Documentation;

internal sealed class OrderedRecordInputs
{
    public List<AnalysisInput> Inputs { get; } =
    [
        new AnalysisInputFixture().WithKind("configuration").WithRelativePath("config/consumer.json")
            .WithText( """
            {
              "revision":"0.1.0",
              "idPatterns":{"epic":"^[0-9]{4}$","feature":"^F[0-9]+$","claim":"^B-[0-9]{3}$","adr":"^ADR-[0-9]{4}$"},
              "claimHeading":"3. Acceptance Criteria",
              "lifecycleMarkers":{},
              "codeAnalysis":{
                "projects":[{"path":"test/tests.csproj","targetFrameworks":["net10.0"]}],
                "features":[{"epic":"0001","feature":"F2"}],
                "enforcement":[{"project":"test/tests.csproj","include":["test/**/*.cs"],"exclude":[],
                  "role":"verifies","declarationKinds":["testMethod"]}],
                "records":{"adrProfiles":[
                  {"glob":".spec/adr/*.md","owner":"repository",
                    "identity":{"headingLevel":1,"separator":": "},"lifecycle":{"section":"Status","firstWord":true}},
                  {"glob":"src/*/.spec/adr/*.md","owner":"containingFeature",
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
        new AnalysisInputFixture().WithKind("feature").WithRelativePath("src/a/.spec/README.md")
            .WithText( """
            ---
            id: F2
            epic: "0201"
            spec_status: draft
            ---
            ## 3. Acceptance Criteria
            | ID | Claim | Source | Status |
            | --- | --- | --- | --- |
            | B-001 | A synthetic need | Agreement | Active |
            """),
        new AnalysisInputFixture().WithKind("feature").WithRelativePath("src/b/.spec/README.md")
            .WithText( """
            ---
            id: F10
            epic: "0001"
            spec_status: draft
            ---
            ## 3. Acceptance Criteria
            | ID | Claim | Source | Status |
            | --- | --- | --- | --- |
            | B-001 | A synthetic need | Agreement | Active |
            """),
        new AnalysisInputFixture().WithKind("feature").WithRelativePath("src/c/.spec/README.md")
            .WithText( """
            ---
            id: F2
            epic: "0001"
            spec_status: draft
            ---
            ## 3. Acceptance Criteria
            | ID | Claim | Source | Status |
            | --- | --- | --- | --- |
            | B-010 | A later synthetic need | Agreement | Active |
            | B-002 | An earlier synthetic need | Agreement | Active |
            | B-001 | A first synthetic need | Agreement | Active |
            """),
        new AnalysisInputFixture().WithKind("adr").WithRelativePath(".spec/adr/0012.md")
            .WithText( """
            # ADR-0012: A synthetic repository decision
            ## Status
            Accepted, on the agreed date.
            """),
        new AnalysisInputFixture().WithKind("adr").WithRelativePath("src/a/.spec/adr/0001.md")
            .WithText( """
            # ADR-0001: A synthetic decision of epic 0201
            ## Status
            Accepted, on the agreed date.
            """),
        new AnalysisInputFixture().WithKind("adr").WithRelativePath("src/c/.spec/adr/0001.md")
            .WithText( """
            # ADR-0001: A synthetic decision of epic 0001
            ## Status
            Accepted, on the agreed date.
            """),
        new AnalysisInputFixture().WithKind("schema:feature").WithRelativePath("contracts/feature.json")
            .WithText( """
            {"type":"object","required":["id","spec_status"],"properties":{"id":{"type":"string"},"spec_status":{"type":"string"}}}
            """),
    ];
}
