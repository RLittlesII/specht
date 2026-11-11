using System;
using Specht.Identifiers;

namespace Specht.Tool.Features.Mint;

/// <summary>The tool's clock, the machine's time in universal time, which the engine reads only through <see cref="IClock"/>.</summary>
/// <implements epic="0001" feature="F19" claim="B-007" />
public sealed class SystemClock : IClock
{
    /// <inheritdoc />
    /// <implements epic="0001" feature="F19" claim="B-007" />
    public DateTimeOffset GetUtcNow() => TimeProvider.System.GetUtcNow();
}
