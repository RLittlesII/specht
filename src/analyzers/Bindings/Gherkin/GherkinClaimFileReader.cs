using System;

namespace Specht.Analysis.Bindings.Gherkin;

public sealed class GherkinClaimFileReader
{
    public GherkinClaimFileReader(GherkinClaimPolicy policy) => Policy = policy;

    public GherkinClaimPolicy Policy { get; }

    public GherkinClaimFile Read(string relativePath, string text) => throw new NotImplementedException();
}
