using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using AwesomeAssertions;
using Reqnroll;
using Specht.Tests.Shared;
using Specht.Tool.Features;
using Specht.Tool.Features.Init;
using Specht.Versioning;

namespace Specht.Acceptance.Versioning;

public sealed partial class VersioningSteps
{
    [Given("the manifest pins version {word} with the pin policy {word}")]
    public void GivenTheManifestPinsVersionWithThePinPolicy(string version, string policy)
    {
        GivenTheManifestPinsVersion(version);
        GivenItsManifestSetsThePinPolicy(policy);
    }

    [Given("its manifest sets the pin policy {word}")]
    public void GivenItsManifestSetsThePinPolicy(string policy) => EditManifest(manifest => manifest["pinPolicy"] = policy);

    [Then("version {word} is marked as the one in use")]
    public void ThenVersionIsMarkedAsTheOneInUse(string version) => _stdout.Should().Contain(version + " (in use)");

    [Then("no version is marked as the one in use")]
    public void ThenNoVersionIsMarkedAsTheOneInUse()
    {
        _exitCode.Should().Be(0, _stderr);
        _stdout.Should().NotContain("(in use)");
    }

    [Then("the standard output does not name version {word}")]
    public void ThenTheStandardOutputDoesNotNameVersion(string version) =>
        OutputLines().Should().NotContain(line => Regex.IsMatch(line, Naming(version)));

    [Then("the manifest is rejected for the pin policy key")]
    public void ThenThePinPolicyKeyIsRejected()
    {
        ThenTheManifestIsRejected();
        _stderr.Should().Contain("'pinPolicy'");
    }

    private static Dictionary<string, byte[]> ShippingCopyWith(SemanticVersion source, IEnumerable<SemanticVersion> numbers)
    {
        var shipping = InitWriter.ShippingCopy(typeof(InitWriter).Assembly);
        var copy = new Dictionary<string, byte[]>(shipping, StringComparer.Ordinal);
        var from = $"/v{source}/";
        foreach (var number in numbers)
        {
            foreach (var (name, bytes) in shipping.Where(entry => entry.Key.Contains(from, StringComparison.Ordinal)))
            {
                var text = Encoding.UTF8.GetString(bytes);
                copy[name.Replace(from, $"/v{number}/", StringComparison.Ordinal)] = Encoding.UTF8.GetBytes(
                    name.EndsWith(".json", StringComparison.Ordinal)
                        ? text.Replace(source.ToString(), number.ToString(), StringComparison.Ordinal)
                        : text + "\n");
            }
        }

        return copy;
    }

    private void ThenTheFilesAreTheSyntheticVersion(string version)
    {
        _exitCode.Should().Be(0, _stderr);
        var after = SnapshotFiles();
        var expected = new ShippedFiles(_shippingCopy!).At(Version(version))
            .Where(file => file.Relative != SpechtTree.ManifestPath && _filesBefore!.ContainsKey(file.Relative))
            .ToList();
        expected.Should().NotBeEmpty();
        foreach (var file in expected)
        {
            after[file.Relative].Should().Equal(file.Bytes, file.Relative);
        }
    }

    private Dictionary<string, byte[]>? _shippingCopy;
}
