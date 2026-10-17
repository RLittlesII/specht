namespace Specht.Tests.FrontmatterCommand;

/// <summary>The engine's default and overridden rule levels determine the command's verdict.</summary>
public sealed class FrontmatterCommandLevels : TheoryData<string, int, string>
{
    public FrontmatterCommandLevels()
    {
        Add("error", 1, "errors: 2, warnings: 0");
        Add("warning", 0, "errors: 0, warnings: 2");
        Add("off", 0, "errors: 0, warnings: 0");
    }
}
