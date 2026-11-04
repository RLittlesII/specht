namespace Specht.Build.Tests;

public sealed class MirrorActionInvalidInputs : TheoryData<string, string>
{
    public MirrorActionInvalidInputs()
    {
        Add("MIRROR_BACKFILL", "yes");
        Add("MIRROR_DRY_RUN", "yes");
        Add("MIRROR_ROOT", string.Empty);
        Add("MIRROR_REPOSITORY", string.Empty);
        Add("GH_TOKEN", string.Empty);
    }
}
