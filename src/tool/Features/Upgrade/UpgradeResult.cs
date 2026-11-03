using System.Collections.Generic;
using Specht.Versioning;

namespace Specht.Tool.Features.Upgrade;

/// <summary>What an upgrade did: the version it read, the files it rewrote and the files it skipped.</summary>
/// <param name="Pinned">The version the manifest pins, which the upgrade never changes.</param>
/// <param name="InUse">The version the check uses, whose files the upgrade wrote: the pin unless a floating pin policy selects a later one.</param>
/// <param name="Rewritten">Each rewritten file, relative to the root with <c>/</c> separators in ordinal order.</param>
/// <param name="Skipped">Each skipped file and its reason, in ordinal order of its relative path.</param>
/// <implements epic="0001" feature="F7" claim="B-005" />
/// <implements epic="0001" feature="F7" claim="B-030" />
/// <implements epic="0001" feature="F7" claim="B-061" />
/// <implements epic="0001" feature="F7" claim="B-017" />
/// <implements epic="0001" feature="F7" claim="B-146" />
public sealed record UpgradeResult(
    SemanticVersion Pinned, SemanticVersion InUse, IReadOnlyList<string> Rewritten, IReadOnlyList<UpgradeSkip> Skipped);
