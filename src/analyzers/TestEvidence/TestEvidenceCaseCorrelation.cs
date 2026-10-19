using System;
using System.Collections.Generic;
using System.Linq;

namespace Specht.Analysis.TestEvidence;

public sealed class TestEvidenceCaseCorrelation
{
    public TestEvidenceCaseCorrelationResult Correlate(TestEvidenceArtifact discovery, TestEvidenceArtifact execution)
    {
        if (!SameInputs(discovery, execution))
        {
            return new TestEvidenceCaseCorrelationResult(false, Array.Empty<TestEvidenceCaseMatch>());
        }

        if (execution.Results.State == "absent")
        {
            return new TestEvidenceCaseCorrelationResult(true, Array.Empty<TestEvidenceCaseMatch>());
        }

        var cases = discovery.Discovery.Cases.ToDictionary(static item => item.CaseId, StringComparer.Ordinal);
        foreach (var item in execution.Discovery.Cases)
        {
            if (cases.TryGetValue(item.CaseId, out var previous) && previous.MethodSymbolId != item.MethodSymbolId)
            {
                return new TestEvidenceCaseCorrelationResult(false, Array.Empty<TestEvidenceCaseMatch>());
            }

            cases[item.CaseId] = cases.ContainsKey(item.CaseId) ? cases[item.CaseId] : item;
        }

        var outcomes = execution.Results.Cases.GroupBy(static outcome => outcome.CaseId, StringComparer.Ordinal)
            .ToDictionary(static group => group.Key, static group => group.First(), StringComparer.Ordinal);
        var matches = cases.Values.Where(item => outcomes.ContainsKey(item.CaseId))
            .OrderBy(static item => item.CaseId, StringComparer.Ordinal)
            .Select(item => new TestEvidenceCaseMatch(item, outcomes[item.CaseId])).ToArray();
        return new TestEvidenceCaseCorrelationResult(true, matches);
    }

    private static bool SameInputs(TestEvidenceArtifact discovery, TestEvidenceArtifact execution) =>
        discovery.ProjectPath == execution.ProjectPath
        && discovery.TargetFramework == execution.TargetFramework
        && discovery.AssemblySha256 == execution.AssemblySha256
        && discovery.AdapterId == execution.AdapterId
        && discovery.SourceFingerprint == execution.SourceFingerprint
        && discovery.ConfigurationFingerprint == execution.ConfigurationFingerprint
        && discovery.PolicyFingerprint == execution.PolicyFingerprint;
}
