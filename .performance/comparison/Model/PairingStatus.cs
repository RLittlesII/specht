namespace Specht.Comparison.Model;

/// <summary>Evidence availability and equivalence, without a regression judgment.</summary>
public enum PairingStatus
{
    Comparable,
    NotComparable,
    Added,
    Removed,
    InsufficientEvidence,
}
