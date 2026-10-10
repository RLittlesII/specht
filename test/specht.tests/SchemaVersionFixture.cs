using Rocket.Surgery.Extensions.Testing.AutoFixtures;

namespace specht.tests;

/// <summary>
/// Builds a <see cref="SchemaVersion"/>: version 1 with three schemas that accept anything and an empty rule vocabulary,
/// until a test overrides what it asserts on.
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
    public SchemaVersionFixture WithNumber(int number) => With(ref _number, number);

    /// <summary>Sets the version's rule vocabulary.</summary>
    /// <param name="ruleIds">The rule ids.</param>
    /// <returns>The fixture.</returns>
    public SchemaVersionFixture WithRuleIds(IReadOnlySet<string> ruleIds) => With(ref _ruleIds, ruleIds);

    public static implicit operator SchemaVersion(SchemaVersionFixture fixture) => fixture.Build();

    private SchemaVersion Build() => new(_number, "{}", "{}", "{}", _ruleIds);

    private int _number = 1;

    private IReadOnlySet<string> _ruleIds = new HashSet<string>(StringComparer.Ordinal);
}
