namespace Specht.Tests.Manifest;

/// <summary>
/// An <c>identifiers</c> member whose grammar, decoded, equals that member's grammar in a shipped manifest below schema
/// <c>0.4.0</c> (<c>0001-F7</c> B-161), as the manifest's JSON writes it: each of the three every such version ships, and the
/// claim grammar with a character written as a JSON escape, which decodes to the same string.
/// </summary>
public sealed class DefaultIdGrammars : TheoryData<string, string>
{
    public DefaultIdGrammars()
    {
        Add("claim", "\"^B-[0-9]{3}[a-z]?$\"");
        Add("constraint", "\"^C-[0-9]+$\"");
        Add("openQuestion", "\"^OQ-[0-9]+$\"");
        Add("claim", "\"^\\u0042-[0-9]{3}[a-z]?$\"");
    }
}
