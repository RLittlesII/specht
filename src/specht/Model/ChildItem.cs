namespace Specht.Model;

/// <summary>A task, test, bug or spike file beside a specification.</summary>
public sealed class ChildItem
{
    /// <summary>An item from its parts.</summary>
    public ChildItem(string relativePath, string fileName, Frontmatter frontmatter, string parentDirectory)
    {
        RelativePath = relativePath;
        FileName = fileName;
        Frontmatter = frontmatter;
        ParentDirectory = parentDirectory;
    }

    /// <summary>Path relative to the repository root.</summary>
    public string RelativePath { get; }

    /// <summary>The file's own name.</summary>
    public string FileName { get; }

    /// <summary>Its frontmatter.</summary>
    public Frontmatter Frontmatter { get; }

    /// <summary>The directory holding it.</summary>
    public string ParentDirectory { get; }

    /// <summary>The <c>id</c> frontmatter value, or <c>null</c>.</summary>
    public string? Id => Frontmatter.Text("id");

    /// <summary>The <c>parent</c> frontmatter value, or <c>null</c>.</summary>
    public string? Parent => Frontmatter.Text("parent");

    /// <summary>The <c>type</c> frontmatter value, or <c>null</c>.</summary>
    public string? Type => Frontmatter.Text("type");

    /// <summary>The <c>&lt;epic&gt;-&lt;nn&gt;</c> prefix of the file name.</summary>
    public string FileNameId => FileName.Length >= 7 ? FileName[..7] : FileName;
}
