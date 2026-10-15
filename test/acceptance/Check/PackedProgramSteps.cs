using System.IO.Compression;
using System.Reflection.Metadata;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using AwesomeAssertions;
using Reqnroll;

namespace Specht.Acceptance.Check;

/// <summary>
/// Steps for the scenarios of <c>src/tool/Features/Check/.spec/check.feature</c> about the program files in the package
/// (0001-F2 B-030 to B-032): the two assemblies this repository builds, the engine's and the tool's, and the debug
/// information the package carries beside each (C-11). Each is read from inside the package with the base class
/// library's metadata readers. The commit is the one the <c>nbgv</c> tool gives for the checkout, the repository is the
/// claim's, and the directory B-032 looks for is the one the build was made in.
/// </summary>
[Binding]
[Scope(Feature = "The check command")]
public sealed class PackedProgramSteps(PackedTool tool)
{
    [Then("every program file of the tool in the package states a version naming the commit it was built from")]
    public async Task ThenEveryProgramFileOfTheToolInThePackageStatesAVersionNamingTheCommitItWasBuiltFrom()
    {
        var package = await tool.PackageAsync();
        var commit = await tool.CommitAsync();

        foreach (var (program, file) in Carried(package, ".dll"))
        {
            var version = AcceptanceTool.InformationalVersion(file);
            version.Should().Contain("+", $"{program} states a version that names a commit");
            var named = new string([.. version![(version.IndexOf('+') + 1)..].TakeWhile(char.IsAsciiHexDigit)]);
            named.Length.Should().BeGreaterThanOrEqualTo(7, $"{program} names a commit by at least git's shortest abbreviation, in {version}");
            commit.Should().StartWith(named, $"{program} names the commit it was built from, in {version}");
        }
    }

    [Then(@"^the debug information in the package points each program file's sources at https://github\.com/RLittlesII/specht$")]
    public async Task ThenTheDebugInformationInThePackagePointsEachProgramFilesSourcesAtTheRepository()
    {
        var package = await tool.PackageAsync();

        foreach (var (program, debug) in Carried(package, ".pdb"))
        {
            var (roots, documents) = Sources(debug);
            documents.Should().OnlyContain(
                document => roots.Keys.Any(root => document.StartsWith(root.TrimEnd('*'), StringComparison.Ordinal)),
                $"every source file of {program} is mapped");
            roots.Values.Should().NotBeEmpty($"{program}'s debug information maps its sources").And.OnlyContain(
                static address => address.Host == "raw.githubusercontent.com"
                                  && address.AbsolutePath.StartsWith("/RLittlesII/specht/", StringComparison.OrdinalIgnoreCase),
                $"{program}'s sources are in the repository");
        }
    }

    [Then("it points them at the commit the tool was built from")]
    public async Task ThenItPointsThemAtTheCommitTheToolWasBuiltFrom()
    {
        var package = await tool.PackageAsync();
        var commit = await tool.CommitAsync();

        foreach (var (program, debug) in Carried(package, ".pdb"))
        {
            var roots = Sources(debug).Roots;
            roots.Values.Should().NotBeEmpty($"{program}'s debug information maps its sources").And.OnlyContain(
                address => address.Segments.Length > 3 && address.Segments[3].TrimEnd('/') == commit,
                $"{program}'s sources are at the commit it was built from");
        }
    }

    [Given("the tool is built the way a continuous-integration run builds it")]
    public async Task GivenTheToolIsBuiltTheWayAContinuousIntegrationRunBuildsIt() => await tool.BuildAsContinuousIntegrationAsync();

    [When("that build is packed")]
    public async Task WhenThatBuildIsPacked() => _built = await tool.PackThatBuildAsync();

    [Then("no program file of the tool in the package names a directory of the machine that built it")]
    public void ThenNoProgramFileOfTheToolInThePackageNamesADirectoryOfTheMachineThatBuiltIt()
    {
        foreach (var (program, file) in Carried(Built, ".dll"))
        {
            Names(file, tool.Repository).Should().BeFalse(
                $"{program}.dll names no directory of the machine that built it");
        }
    }

    [Then("no debug information in the package names one")]
    public void ThenNoDebugInformationInThePackageNamesOne()
    {
        foreach (var (program, debug) in Carried(Built, ".pdb"))
        {
            Sources(debug).Documents.Should().NotContain(
                document => document.StartsWith(tool.Repository, StringComparison.Ordinal),
                $"{program}.pdb names no source file by a directory of the machine that built it");
            Names(debug, tool.Repository).Should().BeFalse($"{program}.pdb names no directory of the machine that built it");
        }
    }

    private static string[] Programs { get; } = ["specht", "specht.tool"];

    private static Guid SourceLink { get; } = new("CC110556-A091-4D38-9FEC-25AB9A351A6A");

    private string Built => _built ?? throw new InvalidOperationException("No build was packed.");

    private static IEnumerable<(string Program, byte[] File)> Carried(string package, string extension) =>
        Programs.SelectMany(program => Carried(package, program, extension).Select(file => (program, file)));

    private static List<byte[]> Carried(string package, string program, string extension)
    {
        using var archive = ZipFile.OpenRead(package);
        var entries = archive.Entries.Where(carried => carried.Name == program + extension).ToList();
        entries.Should().NotBeEmpty($"the package carries {program}{extension}");

        return [.. entries.Select(static entry =>
        {
            using var stream = entry.Open();
            using var bytes = new MemoryStream();
            stream.CopyTo(bytes);

            return bytes.ToArray();
        })];
    }

    private static (Dictionary<string, Uri> Roots, List<string> Documents) Sources(byte[] debug)
    {
        using var provider = MetadataReaderProvider.FromPortablePdbStream(new MemoryStream(debug));
        var reader = provider.GetMetadataReader();
        Dictionary<string, Uri> roots = [];

        foreach (var handle in reader.GetCustomDebugInformation(EntityHandle.ModuleDefinition))
        {
            var information = reader.GetCustomDebugInformation(handle);
            if (reader.GetGuid(information.Kind) == SourceLink)
            {
                using var map = JsonDocument.Parse(reader.GetBlobBytes(information.Value));
                foreach (var root in map.RootElement.GetProperty("documents").EnumerateObject())
                {
                    roots[root.Name] = new Uri(root.Value.GetString()!);
                }
            }
        }

        return (roots, [.. reader.Documents.Select(document => reader.GetString(reader.GetDocument(document).Name))]);
    }

    private static bool Names(byte[] file, string directory) =>
        file.AsSpan().IndexOf(Encoding.UTF8.GetBytes(directory)) >= 0 || file.AsSpan().IndexOf(Encoding.Unicode.GetBytes(directory)) >= 0;

    private string? _built;
}
