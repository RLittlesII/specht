using System.Text.RegularExpressions;

namespace Specht.Discovery;

/// <summary>
/// The manifest's task file shapes, each read as a file name pattern with <c>{task}</c> standing for the task grammar
/// (<c>0001-F6</c> B-003, B-012), and the text a file name's <c>{task}</c> captured (<c>0001-F5</c> B-006).
/// </summary>
/// <implements epic="0001" feature="F6" claim="B-001" />
public sealed class SpechtTaskShapes
{
    /// <summary>The shapes, <c>{task}</c> in each standing for <paramref name="taskGrammar"/> without its anchors.</summary>
    /// <param name="shapes">The manifest's <c>taskFiles</c>.</param>
    /// <param name="taskGrammar">The manifest's <c>identifiers.task</c> grammar.</param>
    /// <implements epic="0001" feature="F6" claim="B-001" />
    public SpechtTaskShapes(IReadOnlyList<string> shapes, string taskGrammar)
    {
        var task = $"(?<{TaskGroup}>{taskGrammar.TrimStart('^').TrimEnd('$')})";
        _shapes = shapes
            .Select(shape => new Regex(SpechtDiscovery.Pattern(shape).Replace(@"\{task}", task, StringComparison.Ordinal)))
            .ToList();
    }

    /// <summary>Whether <paramref name="fileName"/> has one of the shapes.</summary>
    /// <param name="fileName">A file's own name.</param>
    /// <returns>True when a shape matches it.</returns>
    /// <implements epic="0001" feature="F6" claim="B-001" />
    public bool IsMatch(string fileName) => _shapes.Any(shape => shape.IsMatch(fileName));

    /// <summary>The text the first matching shape's <c>{task}</c> captured in <paramref name="fileName"/>.</summary>
    /// <param name="fileName">A file's own name.</param>
    /// <returns>The captured task id, or null when no shape captured one.</returns>
    /// <implements epic="0001" feature="F5" claim="B-006" />
    public string? Task(string fileName) =>
        _shapes.Select(shape => shape.Match(fileName).Groups[TaskGroup]).FirstOrDefault(static group => group.Success)?.Value;

    private const string TaskGroup = "task";

    private readonly List<Regex> _shapes;
}
