namespace Specht.Analysis.Tests.Documentation;

public sealed class InvalidDefaultCases : TheoryData<string>
{
    public InvalidDefaultCases()
    {
        Add("<spec />");
        Add("<spec feature=\"cap:read\" extra=\"unknown\" />");
        Add("<spec feature=\"cap:read\">body</spec>");
        Add("<spec feature=\"cap:read\"><other /></spec>");
        Add("<spec feature=\"cap:read\"");
    }
}
