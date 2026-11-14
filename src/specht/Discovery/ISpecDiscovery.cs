namespace Specht.Discovery;

/// <summary>
/// Lists the repository's specifications, items, epics, companion files and declared test files where the manifest says
/// they are, each by its root-relative path (<c>0001-F6</c> B-001; ADR-0001 stage E).
/// </summary>
/// <implements epic="0001" feature="F6" claim="B-001" />
public interface ISpecDiscovery
{
    /// <summary>
    /// Discovers every specification under <paramref name="root"/> in the layouts <paramref name="inputs"/> declares,
    /// skipping what it excludes (<c>0001-F6</c> B-001, B-002, C-6, C-7).
    /// </summary>
    /// <param name="root">The repository root.</param>
    /// <param name="inputs">The manifest's discovery inputs.</param>
    /// <returns>Each specification, carrying its root-relative path and directory and the layout it was found in.</returns>
    /// <implements epic="0001" feature="F6" claim="B-001" />
    IReadOnlyList<SpechtLocation> FindSpecifications(string root, SpechtDiscoveryInputs inputs);

    /// <summary>
    /// Discovers the item files beside each specification: every file whose name matches one of the manifest's task file
    /// shapes, <c>{task}</c> standing for <paramref name="taskGrammar"/> (<c>0001-F6</c> B-003, B-012).
    /// </summary>
    /// <param name="root">The repository root.</param>
    /// <param name="specifications">The discovered specifications.</param>
    /// <param name="inputs">The manifest's discovery inputs.</param>
    /// <param name="taskGrammar">The manifest's <c>identifiers.task</c> grammar.</param>
    /// <returns>The root-relative path of each item file, in specification order, each one's in ordinal order of file name.</returns>
    /// <implements epic="0001" feature="F6" claim="B-001" />
    IReadOnlyList<string> FindChildItems(
        string root,
        IEnumerable<SpechtLocation> specifications,
        SpechtDiscoveryInputs inputs,
        string taskGrammar);

    /// <summary>
    /// Discovers every epic file under <paramref name="root"/>: each file one of the manifest's epic globs matches and
    /// its exclusions do not skip (<c>0001-F6</c> B-002, B-003, B-012).
    /// </summary>
    /// <param name="root">The repository root.</param>
    /// <param name="inputs">The manifest's discovery inputs.</param>
    /// <returns>The root-relative path of each epic file, in ordinal order.</returns>
    /// <implements epic="0001" feature="F6" claim="B-001" />
    IReadOnlyList<string> FindEpics(string root, SpechtDiscoveryInputs inputs);

    /// <summary>
    /// Discovers the companion files beside <paramref name="specification"/>: every file whose name matches one of the
    /// manifest's companion globs (<c>0001-F6</c> B-003, B-012).
    /// </summary>
    /// <param name="root">The repository root.</param>
    /// <param name="specification">The specification.</param>
    /// <param name="inputs">The manifest's discovery inputs.</param>
    /// <returns>The root-relative path of each companion file, in ordinal order of file name.</returns>
    /// <implements epic="0001" feature="F6" claim="B-001" />
    IReadOnlyList<string> FindCompanions(string root, SpechtLocation specification, SpechtDiscoveryInputs inputs);

    /// <summary>
    /// Discovers the records in one folder beside <paramref name="specification"/>: every Markdown file directly in the
    /// child folder named <paramref name="folder"/>, compared ordinally, of the folder holding the specification file
    /// (<c>0132-F2</c> B-011).
    /// </summary>
    /// <param name="root">The repository root.</param>
    /// <param name="specification">The specification.</param>
    /// <param name="folder">The record folder's name, such as <c>adr</c>.</param>
    /// <returns>The root-relative path of each record, in ordinal order of file name; empty without that folder.</returns>
    /// <implements epic="0132" feature="F2" claim="B-011" />
    IReadOnlyList<string> FindRecords(string root, SpechtLocation specification, string folder);

    /// <summary>
    /// Lists each declared test file once, in root-relative ordinal order, outside the manifest's exclusions; with no glob
    /// declared it lists nothing.
    /// </summary>
    /// <param name="root">The repository root.</param>
    /// <param name="globs">The test file globs.</param>
    /// <param name="exclusions">The selected manifest's directory exclusions.</param>
    /// <returns>The root-relative path of each selected file.</returns>
    /// <implements epic="0001" feature="F6" claim="B-001" />
    IReadOnlyList<string> FindTestFiles(string root, IReadOnlyList<string> globs, IReadOnlyList<string> exclusions);
}
