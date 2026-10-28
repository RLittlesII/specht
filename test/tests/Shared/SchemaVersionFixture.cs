using Rocket.Surgery.Extensions.Testing.AutoFixtures;
using Specht.Versioning;

namespace Specht.Tests.Shared;

/// <summary>
/// Builds a <see cref="SchemaVersion"/>: version 0.1.0 with three schemas that accept anything, an empty rule vocabulary and
/// no template, until a test overrides what it asserts on.
/// </summary>
/// <remarks>
/// Written on <see cref="AutoFixtureBase{TFixture}"/> rather than declared with <c>[AutoFixture]</c>: for the
/// <see cref="IReadOnlySet{T}"/> vocabulary parameter the generator emits an NSubstitute default, and this repository
/// references no mocking library.
/// </remarks>
internal sealed class SchemaVersionFixture : AutoFixtureBase<SchemaVersionFixture>
{
    /// <summary>Sets the version's number.</summary>
    /// <param name="number">The number.</param>
    /// <returns>The fixture.</returns>
    public SchemaVersionFixture WithNumber(SemanticVersion number) => With(ref _number, number);

    /// <summary>Sets the version's rule vocabulary.</summary>
    /// <param name="ruleIds">The rule ids.</param>
    /// <returns>The fixture.</returns>
    public SchemaVersionFixture WithRuleIds(IReadOnlySet<string> ruleIds) => With(ref _ruleIds, ruleIds);

    /// <summary>Sets the version's omission defaults.</summary>
    /// <param name="manifest">The manifest text.</param>
    /// <returns>The fixture.</returns>
    public SchemaVersionFixture WithManifest(string manifest) => With(ref _manifest, manifest);

    /// <summary>Sets the file name of each template the version ships.</summary>
    /// <param name="templates">The template file names.</param>
    /// <returns>The fixture.</returns>
    public SchemaVersionFixture WithTemplates(IReadOnlySet<string> templates) => With(ref _templates, templates);

    public static implicit operator SchemaVersion(SchemaVersionFixture fixture) => fixture.Build();

    private SchemaVersion Build() => new(_number, "{}", "{}", "{}", _ruleIds,
        _manifest ?? EngineServices.Versions().Find(_number)?.Manifest ?? EngineServices.Versions().Find(new SemanticVersion(0, 1, 0))!.Manifest)
    {
        Templates = _templates,
    };

    private SemanticVersion _number = new(0, 1, 0);

    private IReadOnlySet<string> _ruleIds = new HashSet<string>(StringComparer.Ordinal);

    private string? _manifest;

    private IReadOnlySet<string> _templates = new HashSet<string>(StringComparer.Ordinal);
}
