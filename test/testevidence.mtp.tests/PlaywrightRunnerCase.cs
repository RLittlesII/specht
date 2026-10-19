using Microsoft.Playwright.Xunit.v3;
using Xunit;

namespace Specht.TestEvidence.Mtp.Tests;

/// <summary>Provides a real Playwright .NET case for MTP evidence collection.</summary>
[Trait("Tier", "Integration")]
public sealed class PlaywrightRunnerCase : PageTest
{
    /// <verifies epic="0201" feature="F3" claim="B-003" />
    [Fact(Skip = "Evidence collection validates Playwright discovery without launching a browser.")]
    public Task PlaywrightDotNetCase() => Task.CompletedTask;
}
