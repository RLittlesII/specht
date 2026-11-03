namespace Specht.Tests.Manifest;

/// <summary>
/// The one version a set holds, a pin it does not hold, and a floating policy under which that held version shares the
/// pin's major and, for <c>latest-patch</c>, its minor (<c>0001-F7</c> B-145).
/// </summary>
public sealed class UnshippedFloatingPins : TheoryData<string, string, string>
{
    /// <summary>Initializes the scenario's two rows.</summary>
    public UnshippedFloatingPins()
    {
        Add("0.1.5", "0.1.9", "latest-patch");
        Add("1.2.0", "1.0.9", "latest-minor");
    }
}
