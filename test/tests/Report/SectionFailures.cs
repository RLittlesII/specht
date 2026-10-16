using Specht.Model;
using Specht.Tests.Shared;

namespace Specht.Tests.Report;

/// <summary>
/// Synthetic Feature specifications that break a three-section manifest's contract (<c>Goal</c>, <c>Needs</c>,
/// <c>Design</c>) in one way each, for 0001-F3 B-010: one per <c>SPEC010</c> branch.
/// </summary>
public sealed class SectionFailures : TheoryData<string, FeatureSpec>
{
    public SectionFailures()
    {
        Add(
            "a missing section",
            new FeatureSpecFixture().WithDocument(SpecDocument.Parse(
                "## Goal\n\nText.\n\n## Design\n\nText.\n",
                "src/sample/.spec/README.md")));
        Add(
            "a section twice",
            new FeatureSpecFixture().WithDocument(SpecDocument.Parse(
                "## Goal\n\nText.\n\n## Needs\n\nText.\n\n## Needs\n\nText.\n\n## Design\n\nText.\n",
                "src/sample/.spec/README.md")));
        Add(
            "sections out of order",
            new FeatureSpecFixture().WithDocument(SpecDocument.Parse(
                "## Needs\n\nText.\n\n## Goal\n\nText.\n\n## Design\n\nText.\n",
                "src/sample/.spec/README.md")));
    }
}
