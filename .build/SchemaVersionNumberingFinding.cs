namespace Specht.Build;

/// <summary>One fault <see cref="SchemaVersionNumbering.VerifyAsync"/> found, between a shipped version and the next higher one.</summary>
public sealed record SchemaVersionNumberingFinding(string Lower, string Higher, SchemaVersionNumberingFault Fault);
