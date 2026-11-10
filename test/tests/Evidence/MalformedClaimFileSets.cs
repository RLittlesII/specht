using Xunit;

namespace Specht.Tests.Evidence;

public sealed class MalformedClaimFileSets : TheoryData<string, string?>
{
    public MalformedClaimFileSets()
    {
        Add("[{\"feature\":\"cap:read\"}]", null);
        Add("[{\"glob\":7,\"feature\":\"cap:read\"}]", null);
        Add("[{\"glob\":\" \",\"feature\":\"cap:read\"}]", null);
        Add("[{\"glob\":\"src/read/.spec/*.feature\"}]", null);
        Add("[{\"glob\":\"src/read/.spec/*.feature\",\"feature\":7}]", null);
        Add("[{\"glob\":\"src/read/.spec/*.feature\",\"feature\":\" \"}]", null);
        Add("[{\"glob\":\"src/read/.spec/*.feature\",\"feature\":\"cap:read\",\"record\":7}]", null);
        Add("[{\"glob\":\"src/read/.spec/*.feature\",\"feature\":\"Cap:Read\"}]", null);
        Add("[{\"glob\":\"src/read/.spec/*.feature\",\"feature\":\"cap:read\",\"epic\":\"0201\"}]", null);
        Add("[{\"glob\":\"src/read/.spec/*.feature\",\"feature\":\"cap:read\"},{\"glob\":\"src/list/.spec/*.feature\",\"feature\":\"cap:read\"}]", null);
        Add("[{\"glob\":\"src/read/.spec/*.feature\",\"feature\":\"cap:read\"}]", "^[0-9]{4}$");
        Add("[{\"glob\":\"src/read/.spec/*.feature\",\"feature\":\"cap:read\",\"epic\":\"x\"}]", "^[0-9]{4}$");
    }
}
