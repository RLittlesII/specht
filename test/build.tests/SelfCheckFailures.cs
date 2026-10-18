namespace Specht.Build.Tests;

public sealed class SelfCheckFailures : TheoryData<int, int>
{
    public SelfCheckFailures()
    {
        Add(0, 17);
        Add(1, 18);
        Add(2, 3);
        Add(2, 0);
    }
}
