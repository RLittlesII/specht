using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using AwesomeAssertions;
using Markdig;
using Markdig.Syntax;
using Markdig.Syntax.Inlines;
using Reqnroll;

namespace Specht.Acceptance.Check;

/// <summary>
/// Steps for the package scenarios of <c>src/tool/Features/Check/.spec/check.feature</c> (0001-F2 B-017 to B-029 and B-033
/// to B-036): what the packed tool says about itself, what it carries and what it does not. The program files in it are
/// <see cref="PackedProgramSteps"/>'s. "The tool is packed" is <see cref="PackedToolSteps"/>'s. Every
/// expected value is the claim's, written here, and none is read back from the project that packs. A step reads the
/// package's manifest as a feed does; no step opens the package by its id, so each claim fails on its own.
/// </summary>
[Binding]
[Scope(Feature = "The check command")]
public sealed partial class PackageSteps(PackedTool tool)
{
    [Then("the package is named specht.tool")]
    public async Task ThenThePackageIsNamedSpechtTool() => (await SaysAsync("id")).Should().Be("specht.tool");

    [Then(@"^the package names the git repository https://github\.com/RLittlesII/specht as its source$")]
    public async Task ThenThePackageNamesTheGitRepositoryAsItsSource()
    {
        var repository = await RepositoryAsync();
        repository.Type.Should().Be("git");
        repository.Url.Should().Be(Repository);
    }

    [Then("the package records the commit it was packed from")]
    public async Task ThenThePackageRecordsTheCommitItWasPackedFrom() =>
        (await RepositoryAsync()).Commit.Should().Be(await tool.CommitAsync());

    [Then(@"^the package names https://github\.com/RLittlesII/specht as its project page$")]
    public async Task ThenThePackageNamesItsProjectPage() => (await SaysAsync("projectUrl")).Should().Be(Repository);

    [Then("the package states its licence as MIT")]
    public async Task ThenThePackageStatesItsLicenceAsMit()
    {
        var licence = (await tool.MetadataAsync()).Elements().SingleOrDefault(static element => element.Name.LocalName == "license");
        licence.Should().NotBeNull("the package states a licence");
        licence!.Attribute("type")?.Value.Should().Be("expression");
        licence.Value.Should().Be("MIT");
    }

    [Then("the package names Rodney Littles, II as its author")]
    public async Task ThenThePackageNamesItsAuthor() => (await SaysAsync("authors")).Should().Be(Owner);

    [Then("the package's copyright notice names Rodney Littles, II")]
    public async Task ThenThePackagesCopyrightNoticeNamesItsOwner() => (await SaysAsync("copyright")).Should().Contain(Owner);

    [Then("the package describes the tool as one that checks a repository's specification documents against a versioned schema")]
    public async Task ThenThePackageDescribesTheTool() => (await SaysAsync("description")).Should().Be(Description);

    [Then("the package carries the readme it names")]
    public async Task ThenThePackageCarriesTheReadmeItNames() => await CarriedAsync("readme");

    [Then("every link in the readme the package carries is a full web address")]
    public async Task ThenEveryLinkInTheReadmeThePackageCarriesIsAFullWebAddress() =>
        Links(await ReadmeAsync()).Should().OnlyContain(static address => IsFullWebAddress(address));

    [Then("every image in it is a full web address")]
    public async Task ThenEveryImageInItIsAFullWebAddress()
    {
        var readme = await ReadmeAsync();

        readme.Descendants<LinkInline>().Where(static link => link.IsImage).Select(static link => link.Url)
            .Concat(Html(readme).SelectMany(static html => Image().Matches(html)).Select(static match => match.Groups[1].Value))
            .Should().OnlyContain(static address => IsFullWebAddress(address));
    }

    [Then("the package carries the icon it names")]
    public async Task ThenThePackageCarriesTheIconItNames()
    {
        var icon = await CarriedAsync("icon");

        (icon.AsSpan().StartsWith(Png) || icon.AsSpan().StartsWith(Jpeg)).Should().BeTrue("the icon is a PNG or a JPEG image");
    }

    [Then("the package carries the tags specification, spec-driven-development, traceability, gherkin and dotnet-tool")]
    public async Task ThenThePackageCarriesTheTags() =>
        ((await SaysAsync("tags")) ?? string.Empty).Split(' ', StringSplitOptions.RemoveEmptyEntries)
        .Should().Contain(["specification", "spec-driven-development", "traceability", "gherkin", "dotnet-tool"]);

    [Then("the package's release notes point at the repository's releases")]
    public async Task ThenThePackagesReleaseNotesPointAtTheRepositorysReleases() =>
        (await SaysAsync("releaseNotes")).Should().Be(Repository + "/releases");

    [Then("the package carries no specification file")]
    public async Task ThenThePackageCarriesNoSpecificationFile() =>
        (await tool.FilesAsync()).Where(static file => Under(file, ".spec") && !IsScenario(file)).Should().BeEmpty();

    [Then("it carries no scenario file")]
    public async Task ThenItCarriesNoScenarioFile() => (await tool.FilesAsync()).Where(IsScenario).Should().BeEmpty();

    [Then("it carries no work-item file")]
    public async Task ThenItCarriesNoWorkItemFile() =>
        (await tool.FilesAsync()).Where(static file => Under(file, ".issue")).Should().BeEmpty();

    [Then("the readme the package carries opens with the package's description")]
    public async Task ThenTheReadmeThePackageCarriesOpensWithThePackagesDescription() =>
        (await ReadmeTextAsync()).TrimStart().Should().StartWith(Description);

    [Then("the readme the package carries links to the install document in the repository")]
    public async Task ThenTheReadmeThePackageCarriesLinksToTheInstallDocumentInTheRepository() =>
        Links(await ReadmeAsync()).Should().Contain(static address => IsUnderTheRepository(address));

    [Then("the readme the package carries holds no package feed's address")]
    public async Task ThenTheReadmeThePackageCarriesHoldsNoPackageFeedsAddress() =>
        (await ReadmeTextAsync()).Should().NotContainEquivalentOf("nuget.pkg.github.com");

    private static ReadOnlySpan<byte> Png => [0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A];

    private static ReadOnlySpan<byte> Jpeg => [0xFF, 0xD8, 0xFF];

    private static bool IsFullWebAddress(string? address) =>
        Uri.TryCreate(address, UriKind.Absolute, out var uri) && uri.Scheme == Uri.UriSchemeHttps;

    private static bool IsUnderTheRepository(string? address) =>
        address is not null
        && address.StartsWith(Repository, StringComparison.OrdinalIgnoreCase)
        && (address.Length == Repository.Length || address[Repository.Length] is '/' or '#' or '?');

    private static bool Under(string file, string folder) => file.Split('/')[..^1].Contains(folder);

    private static bool IsScenario(string file) => file.EndsWith(".feature", StringComparison.OrdinalIgnoreCase);

    private static IEnumerable<string?> Links(MarkdownDocument readme) =>
        readme.Descendants<LinkInline>().Where(static link => !link.IsImage).Select(static link => link.Url)
            .Concat(readme.Descendants<AutolinkInline>().Select(static link => link.Url))
            .Concat(Html(readme).SelectMany(static html => Anchor().Matches(html)).Select(static match => match.Groups[1].Value));

    private static IEnumerable<string> Html(MarkdownDocument readme) =>
        readme.Descendants<HtmlInline>().Select(static html => html.Tag)
            .Concat(readme.Descendants<HtmlBlock>().Select(static html => html.Lines.ToString()));

    [GeneratedRegex("""<a\b[^>]*?\bhref\s*=\s*["']([^"']*)["']""", RegexOptions.IgnoreCase)]
    private static partial Regex Anchor();

    [GeneratedRegex("""<img\b[^>]*?\bsrc\s*=\s*["']([^"']*)["']""", RegexOptions.IgnoreCase)]
    private static partial Regex Image();

    private async Task<string?> SaysAsync(string element) =>
        (await tool.MetadataAsync()).Elements().SingleOrDefault(named => named.Name.LocalName == element)?.Value;

    private async Task<(string? Type, string? Url, string? Commit)> RepositoryAsync()
    {
        var repository = (await tool.MetadataAsync()).Elements().SingleOrDefault(static element => element.Name.LocalName == "repository");

        return (repository?.Attribute("type")?.Value, repository?.Attribute("url")?.Value, repository?.Attribute("commit")?.Value);
    }

    private async Task<byte[]> CarriedAsync(string element)
    {
        var named = await SaysAsync(element);
        named.Should().NotBeNull($"the package names its {element}");
        var file = await tool.FileAsync(named!);
        file.Should().NotBeNull($"the package carries {named}, the {element} it names");

        return file!;
    }

    private async Task<string> ReadmeTextAsync() => Encoding.UTF8.GetString(await CarriedAsync("readme")).TrimStart('\uFEFF');

    private async Task<MarkdownDocument> ReadmeAsync() => Markdown.Parse(await ReadmeTextAsync());

    private const string Repository = "https://github.com/RLittlesII/specht";

    private const string Owner = "Rodney Littles, II";

    private const string Description = "A dotnet tool that checks a repository's specification documents against a versioned schema.";
}
