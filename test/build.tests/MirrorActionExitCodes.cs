namespace Specht.Build.Tests;

public sealed class MirrorActionExitCodes : TheoryData<int>
{
    public MirrorActionExitCodes()
    {
        Add(0);
        Add(17);
    }
}
