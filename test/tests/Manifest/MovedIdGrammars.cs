namespace Specht.Tests.Manifest;

/// <summary>
/// A customised <c>identifiers</c> grammar of <c>0001-F7</c> B-155's shape for a <c>0001-F19</c> C-13 type, with the
/// <c>plain</c> and <c>id</c> its <c>ids</c> record gets: the six rows of the scenario, a head ending at a <c>{n}</c>
/// quantifier, which repeats the head's last character, and a head ending at a backslash, which no head holds.
/// </summary>
public sealed class MovedIdGrammars : TheoryData<string, string, string, string>
{
    public MovedIdGrammars()
    {
        Add("claim", "^B-[0-9]{4}$", "B-{id}", "[0-9]{4}");
        Add("constraint", "^CON-[0-9]+$", "CON-{id}", "[0-9]+");
        Add("scenario", "^S-(?:[0-9a-hjkmnp-tv-z]{6})$", "S-{id}", "(?:[0-9a-hjkmnp-tv-z]{6})");
        Add("decision", "^[0-9]{4}$", "{id}", "[0-9]{4}");
        Add("adr", "^ADR-0[0-9]{3}$", "ADR-0{id}", "[0-9]{3}");
        Add("claim", "^B-?[0-9]{3}$", "B{id}", "-?[0-9]{3}");
        Add("lesson", "^LX{2}[0-9]+$", "L{id}", "X{2}[0-9]+");
        Add("openQuestion", "^Q\\.[0-9]+$", "Q{id}", "\\.[0-9]+");
    }
}
