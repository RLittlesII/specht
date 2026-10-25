using Xunit;

namespace Specht.Tests.Evidence;

public sealed class CarriedWithoutOwnership : TheoryData<int>
{
    public CarriedWithoutOwnership()
    {
        Add(1);
        Add(2);
    }
}
