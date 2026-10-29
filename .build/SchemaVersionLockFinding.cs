namespace Specht.Build;

/// <summary>One fault <see cref="SchemaVersionLock.VerifyAsync"/> found, against one schema version.</summary>
public sealed record SchemaVersionLockFinding(string Version, SchemaVersionLockFault Fault);
