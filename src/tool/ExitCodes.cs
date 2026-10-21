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

    /// <summary>The root or manifest is missing, or a version is required without an interactive terminal.</summary>
    public const int MissingInput = 2;

    /// <summary>The manifest is not well-formed JSON, not the manifest's shape, or a manifest the engine rejects.</summary>
    public const int InvalidManifest = 3;

    /// <summary>A target named on the command line is not found in the shipped catalog.</summary>
    public const int NotFound = 4;
}
