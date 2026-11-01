using Specht.Model;

namespace Specht.Tests.DocumentFrontmatter;

/// <summary>Source selections supported by the check's existing schema loader.</summary>
public sealed class FrontmatterSources : TheoryData<string, SpechtSchemaSource>
{
    public FrontmatterSources()
    {
        Add("embedded", SpechtSchemaSource.Embedded);
        Add("disk", SpechtSchemaSource.Disk);
        Add("upstream", SpechtSchemaSource.Upstream);
    }
}
