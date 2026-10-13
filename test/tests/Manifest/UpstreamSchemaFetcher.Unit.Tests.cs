using System.Net;
using System.Security.Cryptography;
using AwesomeAssertions;
using Specht.Manifest;

namespace Specht.Tests.Manifest;

/// <summary>The HTTP and raw SHA-256 boundary of upstream schemas (0001-F4 B-014 to B-016; 0001-F7 C-15).</summary>
[Trait("Tier", "Unit")]
public sealed class UpstreamSchemaFetcherUnitTests
{
    [Fact]
    public async Task MatchingRawBytes_WhenFetched_ShouldReturnTheBytesWithoutTransformation()
    {
        // Given
        byte[] content = [0xEF, 0xBB, 0xBF, 0x7B, 0x0D, 0x0A, 0x7D];
        using var client = new HttpClient(new SyntheticHandler(static _ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new ByteArrayContent([0xEF, 0xBB, 0xBF, 0x7B, 0x0D, 0x0A, 0x7D]),
        }));
        var file = new UpstreamSchemaFile("feature", "feature.json", "https://schemas.example/feature.json", Hash(content));

        // When
        var fetched = await new UpstreamSchemaFetcher(client).Fetch(file, CancellationToken.None);

        // Then
        fetched.Should().Equal(content);
    }

    [Fact]
    public async Task DifferentLineEndingsWithTheSameJsonValue_WhenFetched_ShouldRejectTheRawHashMismatch()
    {
        // Given
        using var client = new HttpClient(new SyntheticHandler(static _ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new ByteArrayContent("{\n}"u8.ToArray()),
        }));
        var file = new UpstreamSchemaFile("feature", "feature.json", "https://schemas.example/feature.json", Hash("{\r\n}"u8.ToArray()));

        // When
        var fetch = () => new UpstreamSchemaFetcher(client).Fetch(file, CancellationToken.None);

        // Then
        (await fetch.Should().ThrowExactlyAsync<SpechtUpstreamSchemaException>()).Which.Message.Should().Contain("schemas.example");
    }

    [Theory]
    [InlineData(HttpStatusCode.Unauthorized)]
    [InlineData(HttpStatusCode.NotFound)]
    [InlineData(HttpStatusCode.ServiceUnavailable)]
    public async Task AnUnsuccessfulHttpResponse_WhenFetched_ShouldRefuseTheSource(HttpStatusCode status)
    {
        // Given
        using var client = new HttpClient(new SyntheticHandler(_ => new HttpResponseMessage(status)));
        var file = new UpstreamSchemaFile("feature", "feature.json", "https://schemas.example/feature.json", Hash([]));

        // When
        var fetch = () => new UpstreamSchemaFetcher(client).Fetch(file, CancellationToken.None);

        // Then
        (await fetch.Should().ThrowExactlyAsync<SpechtUpstreamSchemaException>()).Which.Message.Should().Contain("schemas.example");
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task ASourceWithUserInfo_WhenFetched_ShouldSendAndReportNoCredential(bool unreachable)
    {
        // Given
        string? address = null;
        string? authorization = null;
        string? cookie = null;
        using var client = new HttpClient(new SyntheticHandler(request =>
        {
            address = request.RequestUri!.AbsoluteUri;
            authorization = request.Headers.Authorization?.ToString();
            cookie = request.Headers.TryGetValues("Cookie", out var values) ? string.Join(";", values) : null;
            if (unreachable)
            {
                throw new HttpRequestException("secret-user secret-password should never appear in a diagnostic");
            }

            return new HttpResponseMessage(HttpStatusCode.OK) { Content = new ByteArrayContent("wrong bytes"u8.ToArray()) };
        }));
        var file = new UpstreamSchemaFile(
            "feature", "feature.json", "https://secret-user:secret-password@schemas.example/feature.json", Hash([]));

        // When
        var fetch = () => new UpstreamSchemaFetcher(client).Fetch(file, CancellationToken.None);

        // Then
        var exception = (await fetch.Should().ThrowExactlyAsync<SpechtUpstreamSchemaException>()).Which;
        address.Should().Be("https://schemas.example/feature.json");
        authorization.Should().BeNull();
        cookie.Should().BeNull();
        exception.Message.Should().Contain("https://schemas.example/feature.json").And.NotContain("secret-user").And.NotContain("secret-password");
    }

    private static string Hash(byte[] content) => "sha256:" + Convert.ToHexStringLower(SHA256.HashData(content));

    private sealed class SyntheticHandler(Func<HttpRequestMessage, HttpResponseMessage> respond) : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken) =>
            Task.FromResult(respond(request));
    }
}
