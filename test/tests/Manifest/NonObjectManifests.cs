namespace Specht.Tests.Manifest;

/// <summary>Manifest texts whose root is not a JSON object.</summary>
public sealed class NonObjectManifests : TheoryData<string>
{
    public NonObjectManifests()
    {
        Add("[]");
        Add("[{\"rules\":{}}]");
        Add("42");
        Add("\"synthetic\"");
        Add("null");
    }
}
