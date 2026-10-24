using Reqnroll;

namespace Fixture.Shared;

[Binding]
public sealed class SharedClaimSteps
{
    [Given("the claim holds")]
    public void GivenTheClaimHolds() { }

    [Given("the claim holds for {word}")]
    public void GivenTheClaimHoldsFor(string row) => _ = row;
}
