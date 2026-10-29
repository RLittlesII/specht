namespace Specht.Build;

/// <summary>What <see cref="SchemaVersionLock.VerifyAsync"/> found wrong with one version (0001-F7 B-004, B-089 to B-091).</summary>
public enum SchemaVersionLockFault
{
    /// <summary>A locked shipped version's files no longer give the digest its lock entry records (B-089).</summary>
    DigestMismatch,

    /// <summary>A shipped version is unlocked while another one is too (B-090).</summary>
    MoreThanOneUnlocked,

    /// <summary>A shipped version is unlocked and a higher one ships (B-091).</summary>
    UnlockedNotNewest,

    /// <summary>A lock entry 1.0.0 or higher has no shipped folder (B-004).</summary>
    LockedVersionNotShipped,
}
