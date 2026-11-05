using System.Collections.Generic;

namespace Specht.Mcp.Features.PinnedTool;

/// <summary>One program the host asks the system to run (<c>0132-F3</c> § 7).</summary>
/// <param name="FileName">The program, as the system finds it: a name looked up on the path, never a location the host chose.</param>
/// <param name="Arguments">The arguments, one element each.</param>
/// <param name="WorkingDirectory">The directory it runs in, the session's root.</param>
public sealed record ProcessRequest(string FileName, IReadOnlyList<string> Arguments, string WorkingDirectory);
