namespace Specht.Tests.Identifiers;

/// <summary>
/// Snowflake sub-objects that name a datacenter or a worker (<c>0001-F19</c> B-027, B-028; C-16), the bits below that
/// field, its width and the number named: the scenarios' datacenter 3 and worker 9 under the default widths, and each under
/// a width of its own.
/// </summary>
public sealed class SnowflakeFieldsNamed : TheoryData<string, int, int, int>
{
    /// <summary>Initializes the two default-width rows and the two own-width rows.</summary>
    public SnowflakeFieldsNamed()
    {
        Add("""{ "datacenter": 3 }""", 17, 5, 3);
        Add("""{ "worker": 9 }""", 12, 5, 9);
        Add("""{ "datacenterBits": 3, "datacenter": 7 }""", 17, 3, 7);
        Add("""{ "workerBits": 8, "worker": 200 }""", 12, 8, 200);
    }
}
