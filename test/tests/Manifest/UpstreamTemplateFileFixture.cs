using System.Security.Cryptography;
using Rocket.Surgery.Extensions.Testing.AutoFixtures;
using Specht.Manifest;

namespace Specht.Tests.Manifest;

/// <summary>
/// Builds an <see cref="UpstreamTemplateFile"/>: the feature template at a synthetic address, recording the hash of no bytes
/// until a test says which bytes were fetched.
/// </summary>
[AutoFixture(typeof(UpstreamTemplateFile))]
internal sealed partial class UpstreamTemplateFileFixture
{
    public UpstreamTemplateFileFixture() =>
        WithFileName("feature.md").WithAddress("https://schemas.example/specht/feature.md").Fetched([]);

    /// <summary>Sets the recorded hash to the SHA-256 of <paramref name="bytes"/>, as a manifest writes it.</summary>
    /// <param name="bytes">The bytes that were fetched.</param>
    /// <returns>The fixture.</returns>
    public UpstreamTemplateFileFixture Fetched(byte[] bytes) => WithHash("sha256:" + Convert.ToHexStringLower(SHA256.HashData(bytes)));
}
