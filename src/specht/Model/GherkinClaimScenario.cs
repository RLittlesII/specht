
namespace Specht.Model;

/// <summary>One Scenario or Scenario Outline the reader opens: its keyword line and its trimmed title.</summary>
/// <implements epic="0001" feature="F16" claim="B-023" />
public sealed record GherkinClaimScenario(int Line, string Title);
