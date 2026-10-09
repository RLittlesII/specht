namespace specht.tool;

/// <summary>
/// The process exit codes, returned from a command and never thrown (brief § 5; <c>0001-F2</c> C-2). The pre-commit hook
/// and CI read them, so a code is never reused for a second meaning.
/// </summary>
public static class ExitCodes
{
    /// <summary>The check found nothing that fails it.</summary>
    public const int Success = 0;

    /// <summary>An error-severity violation, or any violation under <c>--strict</c>.</summary>
    public const int Violations = 1;

    /// <summary>The root is not a directory, or it has no manifest.</summary>
    public const int MissingInput = 2;

    /// <summary>The manifest is not well-formed JSON or not the manifest's shape.</summary>
    public const int InvalidManifest = 3;
}
