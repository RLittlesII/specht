using System;

namespace Specht.Analysis.Bindings.Gherkin;

public sealed class GherkinInputException : Exception
{
    public GherkinInputException(string? relativePath, string message)
        : base(message) => RelativePath = relativePath;

    public string? RelativePath { get; }
}
