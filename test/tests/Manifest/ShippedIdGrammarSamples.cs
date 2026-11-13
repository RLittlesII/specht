namespace Specht.Tests.Manifest;

/// <summary>
/// A type, an id, and whether the shipped schema 0.4.0 grammar for that type admits it for a Feature without a claim
/// prefix: the minted forms of six or thirteen base32 characters, eight for <c>adr</c> and <c>lesson</c>
/// (<c>0001-F5</c> B-089); every id the 0.3.1 claim, constraint and open-question grammars admit (B-090); and the
/// numbered scenario, decision, ADR and lesson ids in the tree (B-099).
/// </summary>
public sealed class ShippedIdGrammarSamples : TheoryData<string, string, bool>
{
    /// <summary>Initializes the rows.</summary>
    public ShippedIdGrammarSamples()
    {
        Add("claim", "B-k3x9ab", true);
        Add("claim", "B-0d2kq8v1m3n4p", true);
        Add("claim", "B-k3x9a", false);
        Add("claim", "B-K3X9AB", false);
        Add("constraint", "C-k3x9ab", true);
        Add("constraint", "C-0d2kq8v1m3n4p", true);
        Add("constraint", "C-k3x9abc", false);
        Add("constraint", "k3x9ab", false);
        Add("openQuestion", "OQ-k3x9ab", true);
        Add("openQuestion", "OQ-0d2kq8v1m3n4p", true);
        Add("openQuestion", "OQ-k3x9ai", false);
        Add("openQuestion", "Q-k3x9ab", false);
        Add("scenario", "S-k3x9ab", true);
        Add("scenario", "S-0d2kq8v1m3n4p", true);
        Add("scenario", "S-k3x9a", false);
        Add("scenario", "s-k3x9ab", false);
        Add("decision", "k3x9ab", true);
        Add("decision", "0d2kq8v1m3n4p", true);
        Add("decision", "D-k3x9ab", false);
        Add("decision", "k3x9abcd", false);
        Add("adr", "k3x9abmn", true);
        Add("adr", "0d2kq8v1m3n4p", true);
        Add("adr", "k3x9ab", false);
        Add("adr", "ADR-k3x9abmn", false);
        Add("lesson", "k3x9abmn", true);
        Add("lesson", "0d2kq8v1m3n4p", true);
        Add("lesson", "k3x9abmno", false);
        Add("lesson", "k3x9abmnu", false);
        Add("claim", "B-001", true);
        Add("claim", "B-042a", true);
        Add("constraint", "C-1", true);
        Add("constraint", "C-120", true);
        Add("openQuestion", "OQ-7", true);
        Add("openQuestion", "OQ-78", true);
        Add("scenario", "S-1", true);
        Add("scenario", "S-120", true);
        Add("scenario", "S-", false);
        Add("scenario", "S-12a", false);
        Add("decision", "0005", true);
        Add("decision", "005", false);
        Add("adr", "0036", true);
        Add("adr", "ADR-0036", false);
        Add("lesson", "0001", true);
        Add("lesson", "00001", false);
    }
}
