namespace Specht.Tests.Upgrade;

/// <summary>
/// The listings of <c>0001-F7</c> B-030 and B-142 for a root pinned to version 0.2.0 under the on-disk source with no upstream
/// schema source recorded, holding schemas of the consumer's under the three shipped names and version 0.2.0's templates:
/// the file name of the one template edited by hand, or none, and the standard output's lines. With no template edited
/// nothing is rewritten, so the root is current and the skip lines follow that line (B-142); with one edited, the skip
/// lines follow the line naming it (B-030).
/// </summary>
public sealed class UpgradeSkipListings : TheoryData<string, string[]>
{
    public UpgradeSkipListings()
    {
        string[] skips =
        [
            "skipped .specht/schema/frontmatter/epic.schema.json: the on-disk source is selected",
            "skipped .specht/schema/frontmatter/feature.schema.json: the on-disk source is selected",
            "skipped .specht/schema/frontmatter/task.schema.json: the on-disk source is selected",
        ];
        Add(string.Empty, [".specht/ is current at schemaVersion 0.2.0", .. skips]);
        Add("feature.md", [".specht/ rewritten to schemaVersion 0.2.0", "rewritten .specht/templates/feature.md", .. skips]);
    }
}
