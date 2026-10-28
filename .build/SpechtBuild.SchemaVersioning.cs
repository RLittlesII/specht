using System.IO.Abstractions;
using Nuke.Common;
using Specht.Build;

internal partial class SpechtBuild
{
    private Target LockSchemaVersion => static definition => definition
        .Executes(static async () => await new SchemaVersionLock(new FileSystem(), RootDirectory).LockNewestAsync());

    private Target CutSchemaVersion => definition => definition
        .Executes(async () => await new SchemaVersionCut(new FileSystem(), RootDirectory).CutAsync(CutVersion ?? string.Empty));

    [Parameter("The schema version to cut from the newest shipped one, major.minor.patch - Default is none, which is refused")]
    private readonly string? CutVersion;
}
