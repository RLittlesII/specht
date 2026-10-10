namespace specht;

/// <summary>
/// What discovery finds and what it skips, as the manifest declares it (<c>0001-F6</c> B-001, B-002, B-003; decision 0004):
/// the one argument discovery takes.
/// </summary>
/// <param name="Layouts">The specification layouts, in the manifest's order (C-7).</param>
/// <param name="Exclusions">A bare directory name excluded at any depth, or a root-relative path beginning with <c>/</c> (decision 0003).</param>
/// <param name="TaskFiles">The file-name shape of an item beside a specification; <c>{task}</c> stands for the task grammar.</param>
/// <param name="EpicFiles">The glob an epic file's root-relative path matches.</param>
/// <param name="CompanionFiles">The file-name glob of a companion file beside a specification.</param>
public sealed record SpecDiscoveryInputs(
    IReadOnlyList<SpecLayout> Layouts,
    IReadOnlyList<string> Exclusions,
    string TaskFiles,
    string EpicFiles,
    string CompanionFiles);
