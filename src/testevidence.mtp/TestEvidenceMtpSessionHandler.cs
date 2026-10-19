using System.Threading.Tasks;
using Microsoft.Testing.Platform.Extensions;
using Microsoft.Testing.Platform.Extensions.TestHost;
using Microsoft.Testing.Platform.Services;

namespace Specht.TestEvidence.Mtp;

public sealed class TestEvidenceMtpSessionHandler : ITestSessionLifetimeHandler
{
    public string Uid => "specht.test-evidence.session";
    public string Version => "1.0.0";
    public string DisplayName => "Specht test evidence session handler";
    public string Description => "Writes collected runner evidence at the end of an MTP test session.";

    public Task<bool> IsEnabledAsync() => Task.FromResult(true);

    public Task OnTestSessionStartingAsync(ITestSessionContext context)
    {
        TestEvidenceMtpCollector.StartSession();
        return Task.CompletedTask;
    }

    public Task OnTestSessionFinishingAsync(ITestSessionContext context) => TestEvidenceMtpCollector.FinishSessionAsync();
}
