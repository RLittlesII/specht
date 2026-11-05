namespace Specht.Build.Tests;

public sealed class MirrorActionUnstartableScripts : TheoryData<string>
{
    public MirrorActionUnstartableScripts()
    {
        Add("missing.sh");
        Add("not-executable.sh");
    }
}
