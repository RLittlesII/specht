using System.Threading.Tasks;
using Reqnroll;
using Reqnroll.BoDi;

namespace Specht.Acceptance;

/// <summary>
/// "The tool is packed", for every feature that says it: <c>.build/Versioning/.spec/versioning.feature</c> (0055-F5) and
/// <c>src/tool/Features/Check/.spec/check.feature</c> (0001-F2). The step is not scoped to a feature, and the
/// <see cref="PackedTool"/> it packs is registered for the test run, so the features share one pack.
/// </summary>
[Binding]
public sealed class PackedToolSteps(PackedTool tool)
{
    [BeforeTestRun]
    public static void ShareOnePackedTool(IObjectContainer testRun) => testRun.RegisterInstanceAs(new PackedTool(), dispose: true);

    [When("the tool is packed")]
    public async Task WhenTheToolIsPacked() => await tool.PackageAsync();
}
