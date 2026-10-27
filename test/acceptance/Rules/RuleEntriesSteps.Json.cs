using System.Text.Json;
using AwesomeAssertions;
using Reqnroll;

namespace Specht.Acceptance.Rules;

public sealed partial class RuleEntriesSteps
{
    [When("the rules are listed as JSON")]
    public void WhenRulesAreListedAsJson() => _result = AcceptanceTool.Launch(Tree.Root, "rules", "--json");

    [When("the rules are listed as text and as JSON")]
    public void WhenRulesAreListedInBothForms()
    {
        WhenRulesAreListed();
        _textResult = _result;
        WhenRulesAreListedAsJson();
    }

    [Then("the standard output parses as exactly one JSON document")]
    public void ThenStdoutIsOneJsonDocument()
    {
        _result.ExitCode.Should().Be(0, _result.Stderr);
        _result.Stderr.Should().BeEmpty();
        using var document = JsonDocument.Parse(_result.Stdout);
        document.RootElement.ValueKind.Should().Be(JsonValueKind.Array);
    }

    [Then("both forms carry the same rules, families, default severities and effective levels in the same order")]
    public void ThenJsonMatchesText()
    {
        _textResult.ExitCode.Should().Be(0, _textResult.Stderr);
        _textResult.Stderr.Should().BeEmpty();
        _result.ExitCode.Should().Be(0, _result.Stderr);
        _result.Stderr.Should().BeEmpty();
        using var document = JsonDocument.Parse(_result.Stdout);
        var rows = document.RootElement.EnumerateArray().Select(static entry => string.Join('\t',
            entry.GetProperty("id").GetString(), entry.GetProperty("family").GetString(),
            entry.GetProperty("defaultSeverity").GetString(), entry.GetProperty("effectiveLevel").GetString()));
        rows.Should().Equal(_textResult.Stdout.Split('\n', StringSplitOptions.RemoveEmptyEntries).Select(static line => line.TrimEnd('\r')));
    }

    private (string Stdout, string Stderr, int ExitCode) _textResult;
}
