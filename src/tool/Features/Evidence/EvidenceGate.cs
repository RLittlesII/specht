namespace Specht.Tool.Features.Evidence;

/// <summary>The independent required-link and exit decisions for the evidence gate.</summary>
internal static class EvidenceGate
{
    public static string LinkState(bool exempt, bool satisfied, bool inputsIncomplete) => exempt ? "exempt"
        : satisfied ? "satisfied" : inputsIncomplete ? "unavailable" : "missing";

    public static int ExitCode(bool configurationInvalid, bool selectionMissing, bool inputsIncomplete, bool policyFailure) =>
        configurationInvalid ? 3 : selectionMissing ? 4 : inputsIncomplete ? 2 : policyFailure ? 1 : 0;

    public static bool Passed(string? outcome) => outcome == "passed";
}
