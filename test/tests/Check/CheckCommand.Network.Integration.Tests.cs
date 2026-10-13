using System.Net;
using System.Net.Http;
using AwesomeAssertions;
using Specht.Tests.Shared;

namespace Specht.Tests.Check;

/// <summary>
/// The check has nothing to reach the network with (<c>0001-F7</c> B-025; OQ-39, the owner's ruling of 2026-10-12): the
/// engine's library and the tool's own assembly, read from the files the build wrote, reference no network member
/// outside the fetch adapter and its host configuration
/// (ADR-0013), and the same read over an assembly that does reference one finds it, so the guard is
/// not vacuous. Item <c>0095</c> supplies the fetch adapter; only its fetch and client configuration are exempt.
/// </summary>
[Trait("Tier", "Integration")]
public sealed class CheckCommandNetworkIntegrationTests
{
    /// <summary>Gets each assembly the check runs in, and where the build wrote it.</summary>
    public static TheoryData<string, Func<string>> CheckAssemblies =>
        new()
        {
            { "the engine's library", static () => EngineAssembly.Built },
            { "the tool's own assembly", static () => EngineAssembly.BuiltTool },
        };

    [Theory]
    [MemberData(nameof(CheckAssemblies))]
    public void AnAssemblyTheCheckRunsIn_WhenItsReferencesAreRead_ShouldReferenceNoNetworkTypeOrMember(string because, Func<string> built)
    {
        // Given
        var assembly = built();

        // When
        var references = EngineAssembly.CheckNetworkReferencesIn(assembly);

        // Then
        references.Should().BeEmpty(because);
    }

    [Fact]
    public void AnAssemblyThatReferencesANetworkTypeAndMember_WhenItsReferencesAreRead_ShouldFindBoth()
    {
        // Given
        var assembly = typeof(NetworkReader).Assembly.Location;

        // When
        var references = EngineAssembly.NetworkReferencesIn(assembly).Select(static reference => reference.Member);

        // Then
        references.Should().Contain(["System.Net.Http.HttpClient", "System.Net.IPAddress.Loopback"]);
    }

    [Fact]
    public void AFetchOutsideTheApprovedAdapter_WhenItsReferencesAreRead_ShouldRemainAForbiddenNetworkReference()
    {
        // Given
        var assembly = typeof(NetworkReader).Assembly.Location;

        // When
        var references = EngineAssembly.CheckNetworkReferencesIn(assembly);

        // Then
        references.Should().Contain(static reference => reference.Member == "System.Net.Http.HttpClient.GetByteArrayAsync"
            && reference.Site.EndsWith("NetworkReader.Fetch", StringComparison.Ordinal));
    }

    /// <summary>References a network type and a network member, so this test assembly carries each; it is never called.</summary>
    private static class NetworkReader
    {
        public static object[] Read() => [typeof(HttpClient), IPAddress.Loopback];

        public static Task<byte[]> Fetch(HttpClient client) => client.GetByteArrayAsync("https://schemas.example/task.schema.json");
    }
}
