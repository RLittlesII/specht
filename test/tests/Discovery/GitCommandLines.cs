namespace Specht.Tests.Discovery;

/// <summary>
/// Why a row asks as it does, the root, under a work tree whose <c>.git</c> is at <c>repo</c>, the manifest it holds, the
/// finder that asks git, and the arguments git is run with: each glob a <c>:(glob)</c> pathspec with <c>\</c>, <c>?</c> and
/// <c>[</c> escaped, its first character a bracket expression matching that character alone unless it is <c>*</c> or
/// <c>\</c>, so no glob has a literal head git would read as a prefix and none spells a <c>\</c> it did not have (B-007;
/// item 0009), each bare exclusion an <c>--exclude</c> pattern at any depth below the root and an exclude glob pathspec, and
/// each root-relative exclusion an <c>--exclude</c> pattern at that place and an exclude literal pathspec (<c>0001-F6</c>
/// B-005, B-007, B-010, C-1, C-2; decisions 0002, 0010).
/// </summary>
public sealed class GitCommandLines : TheoryData<string, string, string, string, string[]>
{
    public GitCommandLines()
    {
        Add(
            "the root is the work tree's top, so the exclude patterns carry no prefix",
            "repo",
            """{ "exclusions": ["bin", "/.spec"] }""",
            "specifications",
            [
                .. LsFiles,
                "--exclude=/**/bin/",
                "--exclude=/.spec/",
                "--",
                ":(glob)[e]pics/**/spec.md",
                ":(glob)**/.spec/README.md",
                ":(exclude,glob)**/bin/**",
                ":(exclude,literal).spec",
            ]);
        Add(
            "the root is below the work tree's top, so each exclude pattern is anchored at the root by its escaped path",
            "repo/in[1]",
            """{ "exclusions": ["bin", "/.spec"] }""",
            "specifications",
            [
                .. LsFiles,
                @"--exclude=/in\[1]/**/bin/",
                @"--exclude=/in\[1]/.spec/",
                "--",
                ":(glob)[e]pics/**/spec.md",
                ":(glob)**/.spec/README.md",
                ":(exclude,glob)**/bin/**",
                ":(exclude,literal).spec",
            ]);
        Add(
            "a glob keeps its wildcards and escapes the rest, and an exclusion escapes every wildcard but in a literal pathspec",
            "repo",
            """{ "exclusions": ["b*n?", "/a?/[c]"], "epicFiles": ["epics/q?/[x]/e\\pic*.md"] }""",
            "epics",
            [
                .. LsFiles,
                @"--exclude=/**/b\*n\?/",
                @"--exclude=/a\?/\[c]/",
                "--",
                @":(glob)[e]pics/q\?/\[x]/e\\pic*.md",
                @":(exclude,glob)**/b\*n\?/**",
                ":(exclude,literal)a?/[c]",
            ]);
        Add(
            "the test files finder passes the globs it is given",
            "repo",
            """{ "exclusions": ["obj"] }""",
            "test files",
            [
                .. LsFiles,
                "--exclude=/**/obj/",
                "--",
                ":(glob)[t]est/**/*.cs",
                ":(glob)**/*.fs",
                ":(exclude,glob)**/obj/**",
            ]);
        Add(
            "a special first character is bracketed alone, a ! or ^ after a ] so it does not negate, and a \\ head is escaped",
            "repo",
            """{ "exclusions": [], "epicFiles": ["?x/a.md", "[x]/a.md", "\\x/a.md", "!x/a.md", "^x/a.md", "]x/a.md"] }""",
            "epics",
            [
                .. LsFiles,
                "--",
                ":(glob)[?]x/a.md",
                ":(glob)[[]x]/a.md",
                @":(glob)\\x/a.md",
                ":(glob)[]!]x/a.md",
                ":(glob)[]^]x/a.md",
                ":(glob)[]]x/a.md",
            ]);
    }

    private static readonly string[] LsFiles = ["ls-files", "-z", "--cached", "--others", "--exclude-standard"];
}
