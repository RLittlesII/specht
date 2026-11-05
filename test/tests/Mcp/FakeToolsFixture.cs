using Rocket.Surgery.Extensions.Testing.AutoFixtures;
using Specht.Acceptance;

namespace Specht.Tests.Mcp;

/// <summary>
/// Builds the fake <c>specht</c> tools over nothing installed until a test states what is: a local tool manifest pinning a restored or
/// an unrestored tool, a manifest without the pin or removed, a global tool and a tool on the path. They are applied in that order.
/// </summary>
internal sealed class FakeToolsFixture : AutoFixtureBase<FakeToolsFixture>
{
    public FakeToolsFixture WithPin(string directory, string version = AcceptanceTool.FakeTools.PinnedVersion, bool restored = true) =>
        With(ref _pin, new PinState(directory, version, restored));

    public FakeToolsFixture WithUnpinned(string directory, bool manifestRemoved = false) =>
        With(ref _unpinned, new UnpinState(directory, manifestRemoved));

    public FakeToolsFixture WithUnrestored() => With(ref _unrestored, true);

    public FakeToolsFixture WithGlobal(string version = AcceptanceTool.FakeTools.GlobalVersion) => With(ref _global, version);

    public FakeToolsFixture WithOnPath(string version = AcceptanceTool.FakeTools.PathVersion) => With(ref _onPath, version);

    public FakeToolsFixture WithPinnedOrGlobal(bool pinned, string directory, string? version = null) =>
        pinned
            ? WithPin(directory, version ?? AcceptanceTool.FakeTools.PinnedVersion)
            : WithUnpinned(directory).WithGlobal(version ?? AcceptanceTool.FakeTools.GlobalVersion);

    public static implicit operator AcceptanceTool.FakeTools(FakeToolsFixture fixture)
    {
        var tools = new AcceptanceTool.FakeTools();
        if (fixture._pin is { } pin)
        {
            tools.Pin(pin.Directory, pin.Version, pin.Restored);
        }

        if (fixture._unpinned is { } unpinned)
        {
            tools.Unpin(unpinned.Directory, unpinned.ManifestRemoved);
        }

        if (fixture._unrestored)
        {
            tools.Unrestore();
        }

        if (fixture._global is { } global)
        {
            tools.InstallGlobal(global);
        }

        if (fixture._onPath is { } onPath)
        {
            tools.InstallOnPath(onPath);
        }

        return tools;
    }

    private sealed record PinState(string Directory, string Version, bool Restored);

    private sealed record UnpinState(string Directory, bool ManifestRemoved);

    private PinState? _pin;

    private UnpinState? _unpinned;

    private bool _unrestored;

    private string? _global;

    private string? _onPath;
}
