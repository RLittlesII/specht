using System.Collections.Generic;
using Specht.Versioning;

namespace Specht.Tool.Features.Upgrade;

/// <summary>What an upgrade did: the version it read and the files it rewrote.</summary>
/// <param name="Pinned">The version the manifest pins, which the upgrade never changes.</param>
/// <param name="Rewritten">Each rewritten file, relative to the root with <c>/</c> separators in ordinal order.</param>
/// <implements epic="0001" feature="F7" claim="B-005" />
public sealed record UpgradeResult(SemanticVersion Pinned, IReadOnlyList<string> Rewritten);
