using Microsoft.Testing.Platform.Builder;

namespace Specht.TestEvidence.Mtp;

public static class TestEvidenceBuilderHook
{
    public static void AddExtensions(ITestApplicationBuilder builder, string[] arguments)
    {
#pragma warning disable RSA1007 // MTP receives extension factories as callbacks.
        builder.TestHost.AddDataConsumer(static _ => Collector);
        builder.TestHost.AddTestSessionLifetimeHandler(static _ => SessionHandler);
#pragma warning restore RSA1007
    }

    private static readonly TestEvidenceMtpCollector Collector = new();
    private static readonly TestEvidenceMtpSessionHandler SessionHandler = new();
}
