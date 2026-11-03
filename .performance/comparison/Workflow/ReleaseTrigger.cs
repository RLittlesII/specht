using System;

namespace Specht.Comparison;

/// <summary>Decides whether a run published a release and whether a comparison request belongs to that publication.</summary>
public static class ReleaseTrigger
{
    public static bool IsReleasePublication(RunContext context) =>
        context.EventName == "push" && context.Ref.StartsWith(TagPrefix, StringComparison.Ordinal) && context.Ref.Length > TagPrefix.Length;

    public static string? Validate(ComparisonReceipt receipt, RunContext context, ReleasePair pair)
    {
        if (receipt.FormatVersion != 1)
        {
            return "Comparison request has an unsupported format version.";
        }

        if (receipt.Repository != context.Repository || pair.Current.Repository != context.Repository)
        {
            return "Comparison request names another repository.";
        }

        if (receipt.RunId != context.RunId)
        {
            return "Comparison request belongs to another workflow run.";
        }

        if (receipt.Attempt != context.Attempt)
        {
            return "Comparison request belongs to another run attempt.";
        }

        if (receipt.Commit != context.Sha || pair.Current.Commit != context.Sha)
        {
            return "Comparison request names another commit.";
        }

        if (receipt.ReleaseId != pair.Current.Id)
        {
            return "Comparison request names another release.";
        }

        return receipt.PairDigest != ComparisonJson.Digest(pair) ? "Comparison request does not identify its saved pair." : null;
    }

    private const string TagPrefix = "refs/tags/v";
}
