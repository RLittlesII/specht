using System.Collections.Generic;
using System.Text.Json;

namespace Specht.Analysis.TestEvidence;

public sealed record TestEvidenceArtifact(
    string ProjectPath,
    string TargetFramework,
    string ConfigurationName,
    string ConfigurationFingerprint,
    string AssemblyName,
    string AssemblySha256,
    string? AssemblyPath,
    string SourceFingerprint,
    string PolicyFingerprint,
    string AdapterId,
    string AdapterVersion,
    string AdapterContractVersion,
    TestEvidenceDiscovery Discovery,
    IReadOnlyList<TestEvidenceBinding> Bindings,
    TestEvidenceResults Results,
    IReadOnlyList<TestEvidenceInput> Inputs,
    IReadOnlyList<JsonElement> Scenarios,
    IReadOnlyList<TestEvidenceCatalogFeature>? Catalog = null);
