using System.Collections.Generic;
using System.Linq;

namespace Specht.Analysis.TestEvidence;

public sealed class TestEvidenceRunnerCollector
{
    public TestEvidenceRunnerCollectionResult Collect(
        string adapterId,
        string adapterVersion,
        IReadOnlyList<TestEvidenceMethod> methods,
        IReadOnlyList<TestEvidenceRunnerCaseRecord> cases,
        string? sessionId,
        bool enumerationComplete,
        string? runId,
        IReadOnlyList<TestEvidenceRunnerResultRecord> results)
    {
        var methodsById = methods
            .Where(static method => !string.IsNullOrEmpty(method.SymbolId))
            .GroupBy(static method => method.SymbolId, System.StringComparer.Ordinal)
            .ToDictionary(static group => group.Key, static group => group.Count(), System.StringComparer.Ordinal);
        var methodsValid = methodsById.Count == methods.Count
            && methodsById.Values.All(static count => count == 1);
        var resolvedCases = cases.Select(item => ResolveMethod(item, methods)).ToArray();
        var casesValid = methodsValid
            && !string.IsNullOrEmpty(sessionId)
            && !string.IsNullOrEmpty(adapterId)
            && !string.IsNullOrEmpty(adapterVersion)
            && cases.Select((item, index) => item.CaseId is { Length: > 0 }
                && resolvedCases[index] is not null).All(static valid => valid);
        var normalizedCases = casesValid
            ? cases.Select((item, index) => new TestEvidenceCase(item.CaseId!, resolvedCases[index]!, item.RowId, item.DisplayName)).ToArray()
            : System.Array.Empty<TestEvidenceCase>();
        casesValid &= normalizedCases.Select(static item => item.CaseId)
            .Distinct(System.StringComparer.Ordinal).Count() == normalizedCases.Length;

        var discovery = casesValid
            ? new TestEvidenceDiscovery(enumerationComplete ? TestEvidenceDiscoveryState.Complete : TestEvidenceDiscoveryState.Deferred,
                sessionId, methods, normalizedCases)
            : new TestEvidenceDiscovery(TestEvidenceDiscoveryState.Unavailable, null, methods, System.Array.Empty<TestEvidenceCase>());

        var normalizedResults = NormalizeResults(runId, results, normalizedCases);
        var isValid = !string.IsNullOrEmpty(adapterId)
            && !string.IsNullOrEmpty(adapterVersion)
            && normalizedResults.IsValid;
        return new TestEvidenceRunnerCollectionResult(isValid, adapterId, adapterVersion, discovery, normalizedResults.Results);
    }

    private static string? ResolveMethod(TestEvidenceRunnerCaseRecord runnerCase, IReadOnlyList<TestEvidenceMethod> methods)
    {
        if (runnerCase.MethodSymbolId is { Length: > 0 } symbolId)
        {
            if (!methods.Any(method => method.SymbolId == symbolId))
            {
                return null;
            }

            return runnerCase.Method is null || ResolveMethodMetadata(runnerCase.Method, methods) == symbolId
                ? symbolId
                : null;
        }

        return runnerCase.Method is { } identity ? ResolveMethodMetadata(identity, methods) : null;
    }

    private static string? ResolveMethodMetadata(TestEvidenceRunnerMethodRecord identity, IReadOnlyList<TestEvidenceMethod> methods)
    {
        var matches = methods.Where(method => method.DeclaringType == identity.DeclaringType
            && method.Name == identity.Name
            && method.GenericArity == identity.GenericArity
            && method.ParameterTypes.SequenceEqual(identity.ParameterTypes, System.StringComparer.Ordinal)).ToArray();
        return matches.Length == 1 ? matches[0].SymbolId : null;
    }

    private static (bool IsValid, TestEvidenceResults Results) NormalizeResults(
        string? runId,
        IReadOnlyList<TestEvidenceRunnerResultRecord> results,
        IReadOnlyList<TestEvidenceCase> cases)
    {
        if (runId is null && results.Count == 0)
        {
            return (true, new TestEvidenceResults("absent", null, System.Array.Empty<TestEvidenceOutcome>()));
        }

        if (string.IsNullOrEmpty(runId))
        {
            return (false, new TestEvidenceResults("absent", null, System.Array.Empty<TestEvidenceOutcome>()));
        }

        var caseIds = new System.Collections.Generic.HashSet<string>(cases.Select(static item => item.CaseId), System.StringComparer.Ordinal);
        var normalized = new List<TestEvidenceOutcome>();
        foreach (var result in results)
        {
            if (result.CaseId is not { Length: > 0 } caseId
                || !caseIds.Contains(caseId)
                || result.Outcome is not ("passed" or "failed" or "skipped" or "notRun" or "aborted"))
            {
                return (false, new TestEvidenceResults("absent", null, System.Array.Empty<TestEvidenceOutcome>()));
            }

            var outcome = new TestEvidenceOutcome(caseId, result.Outcome, result.Detail);
            if (normalized.Any(previous => previous.CaseId == outcome.CaseId && previous != outcome))
            {
                return (false, new TestEvidenceResults("absent", null, System.Array.Empty<TestEvidenceOutcome>()));
            }

            if (!normalized.Contains(outcome))
            {
                normalized.Add(outcome);
            }
        }

        return (true, new TestEvidenceResults("available", runId, normalized));
    }
}
