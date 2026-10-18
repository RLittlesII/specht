namespace Specht.Analysis.Tests.Policy;

public sealed class AuthoringSelections : TheoryData<string, string, string, bool, bool, bool, bool>
{
    public AuthoringSelections()
    {
        Add("test/tests.csproj", "net10.0", "test/Read.cs", false, true, true, true);
        Add("test/tests.csproj", "net10.0", "test/Read.cs", true, true, true, false);
        Add("test/tests.csproj", "net10.0", "test/Read.cs", false, false, true, false);
        Add("test/tests.csproj", "net10.0", "test/Shared/Read.cs", false, true, true, false);
        Add("test/tests.csproj", "net10.0", "src/Read.cs", false, true, true, false);
        Add("test\\tests.csproj", "net10.0", "test\\Read.cs", false, true, true, true);
        Add("test/tests.csproj", "net11.0", "test/Read.cs", false, true, false, false);
        Add("other/tests.csproj", "net10.0", "test/Read.cs", false, true, false, false);
    }
}
