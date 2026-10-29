using System.Net.Http;
using System.Security.Cryptography;

namespace Specht.Manifest;

/// <summary>Fetches an upstream schema or template without a credential and checks its SHA-256 (<c>0001-F4</c> B-014 to B-016, B-041 to B-043).</summary>
/// <param name="client">The credential-free HTTP client; a test substitutes its message handler.</param>
/// <implements epic="0001" feature="F7" claim="B-068" />
public sealed class UpstreamSchemaFetcher(HttpClient client)
{
    /// <summary>Returns the served bytes only after their SHA-256 matches the manifest's hash.</summary>
    /// <param name="file">The schema address and recorded hash from the manifest.</param>
    /// <param name="cancellationToken">Cancels the fetch.</param>
    /// <returns>The verified bytes without decoding or transformation.</returns>
    /// <exception cref="SpechtUpstreamSchemaException">The source is unreachable or its bytes do not match the hash.</exception>
    /// <implements epic="0001" feature="F7" claim="B-068" />
    public Task<byte[]> Fetch(UpstreamSchemaFile file, CancellationToken cancellationToken) =>
        Fetch(file.Address, file.Matches, cancellationToken);

    /// <summary>Returns the served template bytes only after their SHA-256 matches the manifest's hash.</summary>
    /// <param name="file">The template address and recorded hash from the manifest.</param>
    /// <param name="cancellationToken">Cancels the fetch.</param>
    /// <returns>The verified bytes without decoding or transformation.</returns>
    /// <exception cref="SpechtUpstreamSchemaException">The source is unreachable or its bytes do not match the hash.</exception>
    /// <implements epic="0001" feature="F4" claim="B-041" />
    public Task<byte[]> Fetch(UpstreamTemplateFile file, CancellationToken cancellationToken) =>
        Fetch(
            file.Address,
            bytes => string.Equals("sha256:" + Convert.ToHexStringLower(SHA256.HashData(bytes)), file.Hash, StringComparison.Ordinal),
            cancellationToken);

    /// <implements epic="0001" feature="F7" claim="B-068" />
    /// <implements epic="0001" feature="F4" claim="B-041" />
    private async Task<byte[]> Fetch(string recordedAddress, Func<byte[], bool> matches, CancellationToken cancellationToken)
    {
        var address = new UriBuilder(recordedAddress) { UserName = string.Empty, Password = string.Empty }.Uri;
        byte[] bytes;

        try
        {
            bytes = await client.GetByteArrayAsync(address, cancellationToken);
        }
        catch (Exception exception) when (
            exception is HttpRequestException or IOException
            || (exception is OperationCanceledException && !cancellationToken.IsCancellationRequested))
        {
            throw new SpechtUpstreamSchemaException($"upstream schema source '{address}' could not be reached.");
        }

        if (!matches(bytes))
        {
            throw new SpechtUpstreamSchemaException($"upstream schema source '{address}' does not match its recorded SHA-256.");
        }

        return bytes;
    }
}
