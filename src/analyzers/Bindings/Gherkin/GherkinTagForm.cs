using System;

namespace Specht.Analysis.Bindings.Gherkin;

public sealed record GherkinTagForm(string Form, string Placeholder, bool IgnoreCase, string? Grammar)
{
    public string? Read(string tag) => throw new NotImplementedException();
}
