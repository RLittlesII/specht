namespace Specht.Tests.Upgrade;

/// <summary>Fails any request, so an upgrade test that supplies no synthetic source never reaches the network (ADR-0013).</summary>
internal sealed class NoNetworkHandler : HttpMessageHandler
{
    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken) =>
        throw new InvalidOperationException($"An upgrade test asked for {request.RequestUri} without a synthetic source.");
}
