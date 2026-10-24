using System.Collections.ObjectModel;
using Microsoft.VisualStudio.TestPlatform.ObjectModel;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Client;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Logging;
using VsTestResult = Microsoft.VisualStudio.TestPlatform.ObjectModel.TestResult;

namespace Specht.TestEvidence.VsTest.Tests;

internal sealed class SyntheticLoggerEvents : TestLoggerEvents
{
    public override event EventHandler<TestRunMessageEventArgs>? TestRunMessage;
    public override event EventHandler<TestRunStartEventArgs>? TestRunStart;
    public override event EventHandler<TestResultEventArgs>? TestResult;
    public override event EventHandler<TestRunCompleteEventArgs>? TestRunComplete;
    public override event EventHandler<DiscoveryStartEventArgs>? DiscoveryStart;
    public override event EventHandler<TestRunMessageEventArgs>? DiscoveryMessage;
    public override event EventHandler<DiscoveredTestsEventArgs>? DiscoveredTests;
    public override event EventHandler<DiscoveryCompleteEventArgs>? DiscoveryComplete;

    public void Discover(string source, params TestCase[] cases) =>
        Discover(source, new DiscoveryCompleteEventArgs(cases.Length, false) { FullyDiscoveredSources = [source] }, cases);

    public void Discover(string source, DiscoveryCompleteEventArgs completion, params TestCase[] cases)
    {
        DiscoveryStart?.Invoke(this, new DiscoveryStartEventArgs(new DiscoveryCriteria([source], 1, null)));
        DiscoveryMessage?.Invoke(this, new TestRunMessageEventArgs(TestMessageLevel.Informational, "discovering"));
        DiscoveredTests?.Invoke(this, new DiscoveredTestsEventArgs(cases));
        DiscoveryComplete?.Invoke(this, completion);
    }

    public void Run(string source, IReadOnlyList<TestCase>? named, params VsTestResult[] results)
    {
        var criteria = named is null ? new TestRunCriteria([source], 1) : new TestRunCriteria(named, 1);
        TestRunStart?.Invoke(this, new TestRunStartEventArgs(criteria));
        TestRunMessage?.Invoke(this, new TestRunMessageEventArgs(TestMessageLevel.Informational, "running"));
        foreach (var result in results)
        {
            TestResult?.Invoke(this, new TestResultEventArgs(result));
        }

        TestRunComplete?.Invoke(this, new TestRunCompleteEventArgs(new TestRunStatistics(), false, false, null,
            new Collection<AttachmentSet>(), TimeSpan.Zero));
    }
}
