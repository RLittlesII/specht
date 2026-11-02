namespace Specht.Tests.Tree;

/// <summary>Each size a tree is generated at, in specifications (<c>0109-F1</c> B-015; decision 0004).</summary>
public sealed class TreeSizes : TheoryData<int>
{
    /// <summary>Initializes a new instance of the <see cref="TreeSizes"/> class.</summary>
    public TreeSizes()
    {
        Add(1);
        Add(10);
        Add(100);
        Add(1000);
    }
}
