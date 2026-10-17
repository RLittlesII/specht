using Reqnroll;

namespace BindingSpike.Acceptance;

[Binding]
public sealed class Steps
{
    private int value;

    [Given("the value is {int}")]
    public void GivenValue(int input) => value = input;

    [When("it is incremented")]
    public void Increment() => value++;

    [Then("the value is {int}")]
    public void Verify(int expected)
    {
        if (value != expected) throw new InvalidOperationException("The incremented value is incorrect.");
    }
}
