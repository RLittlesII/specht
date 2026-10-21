namespace Specht.Tool.Features.Evidence;

/// <summary>The independent required-link and exit decisions for the evidence gate.</summary>
/// <implements epic="0201" feature="F5" claim="B-017" />
internal static class EvidenceGate
{
    /// <implements epic="0201" feature="F5" claim="B-017" />
    public static string LinkState(bool exempt, bool satisfied, bool inputsIncomplete) => exempt ? "exempt"
        : satisfied ? "satisfied" : inputsIncomplete ? "unavailable" : "missing";

    /// <implements epic="0201" feature="F5" claim="B-017" />
    public static int ExitCode(bool configurationInvalid, bool selectionMissing, bool inputsIncomplete, bool policyFailure) =>
        configurationInvalid ? 3 : selectionMissing ? 4 : inputsIncomplete ? 2 : policyFailure ? 1 : 0;

    /// <implements epic="0201" feature="F5" claim="B-017" />
    public static bool Passed(string? outcome) => outcome == "passed";
}
