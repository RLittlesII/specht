using Reqnroll;

namespace Specht.Acceptance.Manifest;

/// <summary>Limits the shared print step to the four engine claims until the command is delivered.</summary>
[Binding]
[Scope(Feature = "The manifest command", Tag = "B-003")]
[Scope(Feature = "The manifest command", Tag = "B-004")]
[Scope(Feature = "The manifest command", Tag = "B-005")]
[Scope(Feature = "The manifest command", Tag = "B-006")]
public sealed class EffectiveManifestProjectionSteps(EffectiveManifestSteps steps)
{
    [When("the effective manifest is printed")]
    public void WhenProjected() => steps.WhenProjected();
}
