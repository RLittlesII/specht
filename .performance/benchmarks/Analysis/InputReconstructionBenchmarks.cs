using System;
using System.Collections.Generic;
using System.Globalization;
using BenchmarkDotNet.Attributes;
using Specht.Analysis.Policy;

namespace Specht.Benchmarks.Analysis;

/// <summary>Measures fresh consumer input reconstruction as valid Feature records grow.</summary>
public class InputReconstructionBenchmarks
{
    /// <summary>Gets or sets the number of distinct Feature records supplied to analysis.</summary>
    [Params(1, 40, 160)]
    public int Records { get; set; }

    /// <summary>Builds and validates synthetic consumer inputs outside the timed operation.</summary>
    [GlobalSetup]
    public void PrepareInputs()
    {
        _inputs =
        [
            new AnalysisInput("configuration", "config/consumer.json", Configuration),
            new AnalysisInput("mappings", "config/analysis-mappings.json", Mappings),
            new AnalysisInput("schema:configuration", "contracts/configuration.json", ConfigurationSchema),
            new AnalysisInput("schema:feature", "contracts/feature.json", FeatureSchema),
            new AnalysisInput("schema:adr", "contracts/adr.json", AdrSchema),
            new AnalysisInput("adr", ".spec/adr/0001.md", "# ADR-0001: A synthetic decision\n## Status\nAccepted, on the agreed date."),
            new AnalysisInput("scenario", "src/feature0000/.spec/feature.feature", "Feature: Synthetic behavior"),
            new AnalysisInput("source", "src/Consumer.cs", "class Consumer {}"),
        ];

        for (var index = 0; index < Records; index++)
        {
            var identity = index.ToString("D4", CultureInfo.InvariantCulture);
            _inputs.Add(new AnalysisInput("feature", $"src/feature{identity}/.spec/README.md", Feature(identity)));
        }

        var snapshot = _reader.Read(_inputs);
        if (!snapshot.Available || snapshot.Catalog?.Features.Count != Records)
        {
            throw new InvalidOperationException("The synthetic input catalog is unavailable or incomplete.");
        }
    }

    /// <summary>Builds a fresh content key, policy and catalog from the supplied texts.</summary>
    /// <returns>The fresh analysis snapshot.</returns>
    [Benchmark]
    public AnalysisSnapshot Reconstruct() => _reader.Read(_inputs);

    private static string Feature(string identity) => $"""
        ---
        id: cap:feature{identity}
        spec_status: draft
        ---
        ## 3. Acceptance Criteria
        | ID | Claim | Source | Status |
        | --- | --- | --- | --- |
        | need/7 | A synthetic need | Agreement | Active |
        """;

    private const string Configuration = """
        {
          "revision":"0.1.0",
          "idPatterns":{"feature":"^cap:feature[0-9]+$","claim":"^need/[0-9]+$","adr":"^ADR-[0-9]+$"},
          "claimHeading":"3. Acceptance Criteria",
          "lifecycleMarkers":{"approved":"approved","withdrawn":"Withdrawn","superseded":"Superseded"},
          "codeAnalysis":{
            "projects":[{"path":"src/consumer.csproj","targetFrameworks":["net10.0"]}],
            "features":[{"feature":"cap:feature0000"}],
            "records":{"adrProfiles":[{"glob":".spec/adr/*.md","owner":"repository",
              "identity":{"headingLevel":1,"separator":": "},"lifecycle":{"section":"Status","firstWord":true}}]}
          }
        }
        """;

    private const string Mappings = """
        {"/schemaVersion":"/revision","/identifiers":"/idPatterns","/policy":"/codeAnalysis",
         "/claimsSection":"/claimHeading","/markers":"/lifecycleMarkers"}
        """;

    private const string ConfigurationSchema = "{\"type\":\"object\",\"required\":[\"revision\"]}";
    private const string FeatureSchema = "{\"type\":\"object\",\"required\":[\"id\",\"spec_status\"]}";
    private const string AdrSchema = "{\"type\":\"string\"}";

    private readonly AnalysisInputReader _reader = new();
    private List<AnalysisInput> _inputs = null!;
}
