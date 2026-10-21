using System.Net.Http;
using System.Security.Cryptography;

namespace Specht.Manifest;

/// <summary>Fetches the bytes of one public upstream schema and verifies its recorded SHA-256 (<c>0001-F4</c> B-014 to B-016).</summary>
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
    public async Task<byte[]> Fetch(UpstreamSchemaFile file, CancellationToken cancellationToken)
    {
        var address = new UriBuilder(file.Address) { UserName = string.Empty, Password = string.Empty }.Uri;
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

        var hash = "sha256:" + Convert.ToHexStringLower(SHA256.HashData(bytes));

        if (!string.Equals(hash, file.Hash, StringComparison.Ordinal))
        {
            throw new SpechtUpstreamSchemaException($"upstream schema source '{address}' does not match its recorded SHA-256.");
        }

        return bytes;
    }
}
