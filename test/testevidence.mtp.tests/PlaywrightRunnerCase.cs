using Microsoft.Playwright.Xunit.v3;
using Xunit;

namespace Specht.TestEvidence.Mtp.Tests;

public sealed class PlaywrightRunnerCase : PageTest
{
    /// <verifies epic="0201" feature="F3" claim="B-003" />
    [Fact(Skip = "Evidence collection validates Playwright discovery without launching a browser.")]
    public Task PlaywrightDotNetCase() => Task.CompletedTask;
}
