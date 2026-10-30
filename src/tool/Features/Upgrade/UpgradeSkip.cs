namespace Specht.Tool.Features.Upgrade;

/// <summary>A file an upgrade neither compares nor writes, and why.</summary>
/// <param name="Relative">The file, relative to the root with <c>/</c> separators.</param>
/// <param name="Reason">Why the upgrade skipped it, printed after its path.</param>
/// <implements epic="0001" feature="F7" claim="B-030" />
/// <implements epic="0001" feature="F7" claim="B-061" />
public sealed record UpgradeSkip(string Relative, string Reason);
