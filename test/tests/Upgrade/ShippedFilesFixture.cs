using System.Text;
using Rocket.Surgery.Extensions.Testing.AutoFixtures;
using Specht.Tool.Features;
using Specht.Tool.Features.Init;
using Specht.Versioning;

namespace Specht.Tests.Upgrade;

/// <summary>
/// Builds the shipped files over the tool assembly's shipping copy until a test supplies its own, or adds to it a version
/// the tool does not ship: every file of a shipped version under the new number, each JSON file with the shipped number
/// replaced by the new one and every other file with one line feed appended, so each file differs from its source.
/// </summary>
internal sealed class ShippedFilesFixture : AutoFixtureBase<ShippedFilesFixture>
{
    public ShippedFilesFixture WithShippingCopy(IReadOnlyDictionary<string, byte[]> shippingCopy) => With(ref _shippingCopy, shippingCopy);

    public ShippedFilesFixture WithVersionCopied(SemanticVersion source, SemanticVersion number)
    {
        var from = $"/v{source}/";
        var copy = new Dictionary<string, byte[]>(_shippingCopy, StringComparer.Ordinal);
        foreach (var (name, bytes) in _shippingCopy.Where(entry => entry.Key.Contains(from, StringComparison.Ordinal)))
        {
            var text = Encoding.UTF8.GetString(bytes);
            copy[name.Replace(from, $"/v{number}/", StringComparison.Ordinal)] = Encoding.UTF8.GetBytes(
                name.EndsWith(".json", StringComparison.Ordinal)
                    ? text.Replace(source.ToString(), number.ToString(), StringComparison.Ordinal)
                    : text + "\n");
        }

        return WithShippingCopy(copy);
    }

    public static implicit operator ShippedFiles(ShippedFilesFixture fixture) => new(fixture._shippingCopy);

    private IReadOnlyDictionary<string, byte[]> _shippingCopy = InitWriter.ShippingCopy(typeof(ShippedFiles).Assembly);
}
