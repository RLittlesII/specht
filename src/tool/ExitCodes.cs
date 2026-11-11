namespace Specht.Tool;

/// <summary>
/// The process exit codes, returned from a command and never thrown (brief § 5; <c>0001-F2</c> C-2). The pre-commit hook
/// and CI read them, so a code is never reused for a second meaning.
/// </summary>
/// <implements epic="0001" feature="F2" claim="B-003" />
public static class ExitCodes
{
    /// <summary>The check found nothing that fails it.</summary>
    public const int Success = 0;

    /// <summary>An error-severity violation, or any violation under <c>--strict</c>.</summary>
    public const int Violations = 1;

    /// <summary>
    /// The root or manifest is missing, a version is required without an interactive terminal, or <c>mint</c> is given
    /// <c>--feature</c> for a repository-wide type or none for a per-Feature type the manifest opts in.
    /// </summary>
    public const int MissingInput = 2;

    /// <summary>
    /// The manifest is not well-formed JSON, not the manifest's shape, or a manifest the engine rejects, a <c>mint</c> entry
    /// that opts in <c>epic</c>, <c>feature</c> or <c>task</c> and one that names a strategy and gives no id for the type
    /// requested included.
    /// </summary>
    public const int InvalidManifest = 3;

    /// <summary>
    /// A target named on the command line is not found in the shipped catalog, the manifest's <c>mint</c> key holds no
    /// entry for the type requested or its entry names no strategy, or the folder <c>mint</c>'s <c>--feature</c> names holds
    /// no Feature's <c>.spec/</c>.
    /// </summary>
    public const int NotFound = 4;
}
