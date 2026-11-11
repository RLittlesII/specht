namespace Specht.Tests.Mint;

/// <summary>
/// A manifest pinned to <c>0.3.1</c> that opts in an <c>epic</c>, <c>feature</c> or <c>task</c> type, and a request for
/// that type (<c>0001-F19</c> B-018, B-061; C-13). Each entry names a Nano ID size over the one-character alphabet <c>z</c>,
/// and <c>identifiers</c> declares a grammar its draw matches, so neither the strategy's missing default size nor the type's
/// shipped grammar refuses it, and only B-061 does. The <c>task</c> grammar names the <c>epic</c> and <c>sequence</c> groups
/// the check requires of it (<c>0001-F5</c> B-050), so the manifest is not refused when it is read.
/// </summary>
public sealed class NotMintedTypesOptedIn : TheoryData<string, string>
{
    /// <summary>Initializes the <c>epic</c>, <c>feature</c> and <c>task</c> rows.</summary>
    public NotMintedTypesOptedIn()
    {
        Add(
            """
            {
              "schemaVersion": "0.3.1",
              "identifiers": { "epic": "^z{3}$" },
              "mint": { "epic": { "strategy": "nanoid", "nanoid": { "alphabet": "z", "size": 3 } } }
            }
            """,
            "epic");
        Add(
            """
            {
              "schemaVersion": "0.3.1",
              "identifiers": { "feature": "^z{3}$" },
              "mint": { "feature": { "strategy": "nanoid", "nanoid": { "alphabet": "z", "size": 3 } } }
            }
            """,
            "feature");
        Add(
            """
            {
              "schemaVersion": "0.3.1",
              "identifiers": { "task": "^(?<epic>z)(?<sequence>zz)$" },
              "mint": { "task": { "strategy": "nanoid", "nanoid": { "alphabet": "z", "size": 3 } } }
            }
            """,
            "task");
    }
}
