using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Specht.Analysis.Documentation;

/// <summary>Reads supplied method recognition for analysis and invoked repairs.</summary>
public sealed class DocumentationTestMethods
{
    public HashSet<string> Read(AnalyzerOptions options, CancellationToken cancellationToken)
    {
        var methods = new HashSet<string>(StringComparer.Ordinal);
        foreach (var file in options.AdditionalFiles)
        {
            if (!options.AnalyzerConfigOptionsProvider.GetOptions(file)
                    .TryGetValue("build_metadata.AdditionalFiles.SpechtInputKind", out var kind) || kind != "test-evidence")
            {
                continue;
            }

            try
            {
                var document = JsonNode.Parse(file.GetText(cancellationToken)?.ToString() ?? "null");
                if (document?["format"]?.GetValue<string>() == "specht.test-evidence" && document["formatVersion"]?.GetValue<int>() == 1)
                {
                    foreach (var method in document["discovery"]?["methods"]?.AsArray() ?? new JsonArray())
                    {
                        if (method?["symbolId"]?.GetValue<string>() is { } id)
                        {
                            methods.Add(id);
                        }
                    }
                }
            }
            catch (Exception exception) when (exception is JsonException or InvalidOperationException)
            {
            }
        }

        return methods;
    }
}
