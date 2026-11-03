using System.Diagnostics.CodeAnalysis;
using Specht.Manifest;

namespace Specht.Model;

/// <summary>A task, test, bug or spike file beside a specification.</summary>
/// <implements epic="0001" feature="F1" claim="B-004" />
public sealed class ChildItem
{
    /// <summary>An item from its parts, read through the manifest's keys.</summary>
    /// <param name="relativePath">Path relative to the repository root.</param>
    /// <param name="fileName">The file's own name.</param>
    /// <param name="frontmatter">Its frontmatter.</param>
    /// <param name="parentDirectory">The root-relative directory holding it.</param>
    /// <param name="keys">The frontmatter key each role is read from, or null for the names every older pin reads.</param>
    /// <param name="task">The text the task file shape's <c>{task}</c> captured, or null for the file name's first seven characters.</param>
    /// <implements epic="0001" feature="F1" claim="B-004" />
    /// <implements epic="0001" feature="F5" claim="B-004" />
    /// <implements epic="0001" feature="F5" claim="B-006" />
    public ChildItem(
        string relativePath,
        string fileName,
        Frontmatter frontmatter,
        string parentDirectory,
        SpechtTaskKeys? keys = null,
        string? task = null)
    {
        RelativePath = relativePath;
        FileName = fileName;
        Frontmatter = frontmatter;
        ParentDirectory = parentDirectory;
        Keys = keys ?? new SpechtTaskKeys();
        FileNameId = task;
    }

    /// <summary>Path relative to the repository root.</summary>
    public string RelativePath { get; }

    /// <summary>The file's own name.</summary>
    public string FileName { get; }

    /// <summary>Its frontmatter.</summary>
    public Frontmatter Frontmatter { get; }

    /// <summary>The root-relative directory holding it, empty at the root.</summary>
    public string ParentDirectory { get; }

    /// <summary>The frontmatter key each role is read from.</summary>
    public SpechtTaskKeys Keys { get; }

    /// <summary>The id frontmatter value, or <c>null</c>.</summary>
    public string? Id => Frontmatter.Text(Keys.Id);

    /// <summary>The parent frontmatter value, or <c>null</c>.</summary>
    public string? Parent => Frontmatter.Text(Keys.Parent);

    /// <summary>The type frontmatter value, or <c>null</c>.</summary>
    public string? Type => Frontmatter.Text(Keys.Type);

    /// <summary>The task id its file name carries: the captured <c>{task}</c>, or else the <c>&lt;epic&gt;-&lt;nn&gt;</c> prefix.</summary>
    [AllowNull]
    public string FileNameId => field ?? (FileName.Length >= 7 ? FileName[..7] : FileName);
}
