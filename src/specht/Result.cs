using Dunet;

namespace Specht;

/// <summary>What a step that reads a check's input returns: the value it read, or the <see cref="InputFailure"/> that stopped it.</summary>
/// <typeparam name="T">What the step reads.</typeparam>
[Union]
public partial record Result<T>
{
    /// <summary>The step read its input.</summary>
    /// <param name="Value">What it read.</param>
    public partial record Ok(T Value);

    /// <summary>The input stopped the step.</summary>
    /// <param name="Failure">Why.</param>
    public partial record Failed(InputFailure Failure);
}
