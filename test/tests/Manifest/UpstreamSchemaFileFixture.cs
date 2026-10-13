using System.Security.Cryptography;
using Rocket.Surgery.Extensions.Testing.AutoFixtures;
using Specht.Manifest;

namespace Specht.Tests.Manifest;

/// <summary>
/// Builds an <see cref="UpstreamSchemaFile"/>: the item kind under the default manifest's file name, at a synthetic
/// address, recording the hash of no bytes until a test says which bytes were fetched.
/// </summary>
[AutoFixture(typeof(UpstreamSchemaFile))]
internal sealed partial class UpstreamSchemaFileFixture
{
    public UpstreamSchemaFileFixture() => Named("task", "task.schema.json").Fetched([]);

    /// <summary>Sets the kind, its file name, and the address that file name gives it.</summary>
    /// <param name="kind">The kind.</param>
    /// <param name="fileName">The file name the manifest gives the kind.</param>
    /// <returns>The fixture.</returns>
    public UpstreamSchemaFileFixture Named(string kind, string fileName) =>
        WithKind(kind).WithFileName(fileName).WithAddress("https://schemas.example/specht/" + fileName);

    /// <summary>Sets the recorded hash to the SHA-256 of <paramref name="bytes"/>, as a manifest writes it.</summary>
    /// <param name="bytes">The bytes that were fetched.</param>
    /// <returns>The fixture.</returns>
    public UpstreamSchemaFileFixture Fetched(byte[] bytes) => WithHash("sha256:" + Convert.ToHexStringLower(SHA256.HashData(bytes)));
}
