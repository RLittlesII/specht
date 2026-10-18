using System.Collections.Generic;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Specht.Acceptance.Adoption;

internal sealed class AdoptionFileOptions(IReadOnlyDictionary<string, string> values) : AnalyzerConfigOptions
{
    public override bool TryGetValue(string key, out string value) => values.TryGetValue(key, out value!);
}
