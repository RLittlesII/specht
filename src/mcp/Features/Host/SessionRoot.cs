namespace Specht.Mcp.Features.Host;

/// <summary>
/// The repository root the session answers for (<c>0132-F3</c> B-009). The command line names it after the application is composed,
/// so the command states it once, before the session starts, and every part reads it from here.
/// </summary>
public sealed class SessionRoot
{
    /// <summary>Gets or sets the root, as typed on the command line.</summary>
    public string Value { get; set; } = ".";
}
