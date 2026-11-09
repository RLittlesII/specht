namespace Specht.Tests.Identifiers;

/// <summary>Strategy names other than <c>nanoid</c> or <c>snowflake</c> (<c>0001-F19</c> B-048, C-3), the scenario's rows.</summary>
public sealed class UnknownStrategies : TheoryData<string>
{
    /// <summary>Initializes the <c>sequence</c> and <c>uuid</c> rows.</summary>
    public UnknownStrategies()
    {
        Add("sequence");
        Add("uuid");
    }
}
