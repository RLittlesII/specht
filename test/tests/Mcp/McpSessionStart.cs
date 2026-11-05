namespace Specht.Tests.Mcp;

/// <summary>What starts the built MCP: the directory it starts in, the environment it carries over what this process hands it, and its arguments.</summary>
/// <param name="Root">The directory the process starts in.</param>
/// <param name="Environment">The environment variables set for the process, by name.</param>
/// <param name="Arguments">The arguments, each passed as one.</param>
internal sealed record McpSessionStart(string Root, IReadOnlyDictionary<string, string> Environment, string[] Arguments);
