namespace Specht.Tests.Mcp;

/// <summary>The exit codes with which the check answers: clean and violations.</summary>
public sealed class ExitCodesThatAreAnswers : TheoryData<int>
{
    public ExitCodesThatAreAnswers()
    {
        Add(0);
        Add(1);
    }
}
