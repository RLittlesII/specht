using System.IO.Abstractions;
using Nuke.Common;
using Specht.Build;

internal partial class SpechtBuild
{
    private Target LockSchemaVersion => static definition => definition
        .Executes(static async () => await new SchemaVersionLock(new FileSystem(), RootDirectory).LockNewestAsync());
}
