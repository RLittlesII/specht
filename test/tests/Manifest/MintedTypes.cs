namespace Specht.Tests.Manifest;

/// <summary>
/// Each type <c>0001-F19</c> C-13 opts in, a grammar for it in <c>identifiers</c> that admits the ids its entry mints
/// (A-6), and its <c>mint</c> entry in C-12's shape, written as JSON. The entries alternate the two strategies, so each
/// strategy's own sub-object appears with every member C-12 names.
/// </summary>
public sealed class MintedTypes : TheoryData<string, string, string>
{
    /// <summary>Initializes one row for each of the seven types.</summary>
    public MintedTypes()
    {
        Add(
            "adr",
            "^[0-9a-z]{8}$",
            """
            {
              "strategy": "nanoid",
              "nanoid": { "alphabet": "0123456789abcdefghjkmnpqrstvwxyz", "size": 8 },
              "prefix": "",
              "encoding": "none",
              "attempts": 8
            }
            """);
        Add(
            "lesson",
            "^[0-9a-z]{13}$",
            """
            {
              "strategy": "snowflake",
              "snowflake": {
                "epoch": "2026-01-01",
                "datacenterBits": 5,
                "workerBits": 5,
                "sequenceBits": 12,
                "datacenter": 1,
                "worker": 2,
                "sequenceStart": 0
              },
              "prefix": "",
              "encoding": "base32",
              "attempts": 8
            }
            """);
        Add(
            "decision",
            "^[0-9a-z]{6}$",
            """
            {
              "strategy": "nanoid",
              "nanoid": { "alphabet": "0123456789abcdefghjkmnpqrstvwxyz", "size": 6 },
              "prefix": "",
              "encoding": "none",
              "attempts": 8
            }
            """);
        Add(
            "claim",
            "^B-([0-9]{3}[a-z]?|[0-9a-z]{13})$",
            """
            {
              "strategy": "snowflake",
              "snowflake": {
                "epoch": "2026-01-01",
                "datacenterBits": 5,
                "workerBits": 5,
                "sequenceBits": 12,
                "datacenter": 1,
                "worker": 2,
                "sequenceStart": 0
              },
              "prefix": "B-",
              "encoding": "base32",
              "attempts": 8
            }
            """);
        Add(
            "constraint",
            "^C-([0-9]+|[0-9a-z]{6})$",
            """
            {
              "strategy": "nanoid",
              "nanoid": { "alphabet": "0123456789abcdefghjkmnpqrstvwxyz", "size": 6 },
              "prefix": "C-",
              "encoding": "none",
              "attempts": 8
            }
            """);
        Add(
            "openQuestion",
            "^OQ-[0-9]+$",
            """
            {
              "strategy": "snowflake",
              "snowflake": {
                "epoch": "2026-01-01",
                "datacenterBits": 5,
                "workerBits": 5,
                "sequenceBits": 12,
                "datacenter": 1,
                "worker": 2,
                "sequenceStart": 0
              },
              "prefix": "OQ-",
              "encoding": "none",
              "attempts": 8
            }
            """);
        Add(
            "scenario",
            "^S-[0-9a-z]{6}$",
            """
            {
              "strategy": "nanoid",
              "nanoid": { "alphabet": "0123456789abcdefghjkmnpqrstvwxyz", "size": 6 },
              "prefix": "S-",
              "encoding": "none",
              "attempts": 8
            }
            """);
    }
}
