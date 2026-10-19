using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Globalization;
using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Specht.Analysis.Policy;

/// <summary>Reads the build-supplied AdditionalFiles metadata and current source texts.</summary>
public sealed class AnalysisInputAdapter
{
    public AnalysisSnapshot Read(
        ImmutableArray<AdditionalText> files, AnalyzerConfigOptionsProvider options, IEnumerable<SyntaxTree> sources,
        CancellationToken cancellationToken)
    {
        var inputs = new List<AnalysisInput>();
        foreach (var file in files)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var metadata = options.GetOptions(file);
            if (!metadata.TryGetValue("build_metadata.AdditionalFiles.SpechtInputKind", out var kind)
                || string.IsNullOrWhiteSpace(kind))
            {
                continue;
            }

            var valid = metadata.TryGetValue("build_metadata.AdditionalFiles.SpechtRelativePath", out var path);
            inputs.Add(new AnalysisInput(kind, valid ? path! : "analysis/invalid-input", valid ? file.GetText(cancellationToken)?.ToString() : null));
        }

        var index = 0;
        foreach (var source in sources.OrderBy(static source => source.FilePath, StringComparer.Ordinal))
        {
            inputs.Add(new AnalysisInput("source", "source/" + index.ToString(CultureInfo.InvariantCulture),
                source.GetText(cancellationToken).ToString()));
            index++;
        }

        return new AnalysisInputReader().Read(inputs, cancellationToken);
    }
}
