namespace Specht.Tests.Model;

/// <summary>
/// A record's text and the first line the parse gives under its <c>## Status</c> heading (<c>0132-F2</c> B-011, OQ-3): the
/// first paragraph after the first level-2 heading whose text is exactly <c>Status</c> and before the next level-2
/// heading, a list's or a quote's included, up to its first line break, its literals and code-span contents flattened and
/// trimmed; <see langword="null"/> with no such heading or no paragraph under it.
/// </summary>
public sealed class StatusSectionTexts : TheoryData<string, string?>
{
    public StatusSectionTexts()
    {
        Add(
            "---\ntitle: \"ADR-0001: Read each record as written\"\ntype: adr\n---\n\n# ADR-0001: Read each record as written\n\n"
                + "## Status\n\nAccepted, 2026-10-12, by the owner.\n\n## Context\n\nProse.\n",
            "Accepted, 2026-10-12, by the owner.");
        Add("## Status\n\naccepted - 2026-10-08\nby the owner, on review.\n", "accepted - 2026-10-08");
        Add("## Status\n\n- Proposed, awaiting the owner.\n", "Proposed, awaiting the owner.");
        Add("## Status\n\n> Superseded by ADR-0002.\n", "Superseded by ADR-0002.");
        Add("## Status\n\n**Accepted** on `2026-10-12`.\n", "Accepted on 2026-10-12.");
        Add("## Context\n\nProse.\n\n## Status\n\n### History\n\nRejected — withdrawn by the owner.\n", "Rejected — withdrawn by the owner.");
        Add("## Status\n\nProposed.\n\n## Status\n\nAccepted.\n", "Proposed.");
        Add("## Context\n\nAccepted.\n", null);
        Add("## Status\n\n## Context\n\nAccepted.\n", null);
        Add("### Status\n\nAccepted.\n", null);
        Add("## status\n\nAccepted.\n", null);
        Add("## Status of the record\n\nAccepted.\n", null);
    }
}
