using Specht.Model;

namespace Specht.Tests.DocumentFrontmatter;

/// <summary>Source selections supported by the check's existing schema loader.</summary>
public sealed class FrontmatterSources : TheoryData<string, SpecSchemaSource>
{
    public FrontmatterSources()
    {
        Add("embedded", SpecSchemaSource.Embedded);
        Add("disk", SpecSchemaSource.Disk);
        Add("upstream", SpecSchemaSource.Upstream);
    }
}
