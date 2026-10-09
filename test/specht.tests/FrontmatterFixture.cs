using System.Text.Json.Nodes;
using Rocket.Surgery.Extensions.Testing.AutoFixtures;

namespace specht.tests;

/// <summary>
/// Builds a <see cref="Frontmatter"/>: a document with no frontmatter, starting on line 1 and naming no key, until a test
/// overrides what it asserts on.
/// </summary>
/// <remarks>
/// Written on <see cref="AutoFixtureBase{TFixture}"/> rather than declared with <c>[AutoFixture]</c>: for the
/// <see cref="IReadOnlyDictionary{TKey,TValue}"/> key-line parameter the generator emits an NSubstitute default, and this
/// repository references no mocking library.
/// </remarks>
internal sealed class FrontmatterFixture : AutoFixtureBase<FrontmatterFixture>
{
    /// <summary>Sets the frontmatter mapping.</summary>
    /// <param name="node">The mapping, or <see langword="null"/> for a document with none.</param>
    /// <returns>The fixture.</returns>
    public FrontmatterFixture WithNode(JsonObject? node) => With(ref _node, node);

    /// <summary>Sets the one-based line of the opening delimiter.</summary>
    /// <param name="startLine">The line.</param>
    /// <returns>The fixture.</returns>
    public FrontmatterFixture WithStartLine(int startLine) => With(ref _startLine, startLine);

    /// <summary>Sets the one-based line of each top-level key.</summary>
    /// <param name="keyLines">The lines, by key.</param>
    /// <returns>The fixture.</returns>
    public FrontmatterFixture WithKeyLines(IReadOnlyDictionary<string, int> keyLines) => With(ref _keyLines, keyLines);

    public static implicit operator Frontmatter(FrontmatterFixture fixture) => fixture.Build();

    private Frontmatter Build() => new(_node, _startLine, _keyLines);

    private JsonObject? _node;

    private int _startLine = 1;

    private IReadOnlyDictionary<string, int> _keyLines = new Dictionary<string, int>(StringComparer.Ordinal);
}
