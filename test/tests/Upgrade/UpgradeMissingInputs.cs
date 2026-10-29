using Specht.Manifest;

namespace Specht.Tests.Upgrade;

/// <summary>
/// The missing inputs of <c>0001-F7</c> B-018 and B-019, as the root typed beneath a root holding files of another version,
/// whether that root holds its manifest, and the failure the loader throws: a typed path that does not exist and one that
/// names a file, each beside a manifest, and the root itself without one.
/// </summary>
public sealed class UpgradeMissingInputs : TheoryData<string, bool, Type>
{
    public UpgradeMissingInputs()
    {
        Add("no-such-root", true, typeof(SpechtRootNotFoundException));
        Add(".specht/templates/feature.md", true, typeof(SpechtRootNotFoundException));
        Add(".", false, typeof(SpechtManifestNotFoundException));
    }
}
