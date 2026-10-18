using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading;
using Json.Schema;
using Specht.Analysis.Bindings;
using Specht.Model;

namespace Specht.Analysis.Policy;

/// <summary>Reads actual consumer inputs into a fresh catalog, with no shipped-schema lookup or ambient I/O.</summary>
public sealed class AnalysisInputReader
{
    public AnalysisSnapshot Read(IReadOnlyList<AnalysisInput> inputs, CancellationToken cancellationToken = default)
    {
        var path = "analysis/inputs";
        var contentKey = Key(inputs, cancellationToken);
        try
        {
            foreach (var input in inputs)
            {
                cancellationToken.ThrowIfCancellationRequested();
                path = _paths.Normalize(input.RelativePath);
                if (input.Text is null)
                {
                    throw new ArgumentException("A required consumer input could not be read.");
                }
            }

            if (inputs.GroupBy(input => _paths.Normalize(input.RelativePath), StringComparer.Ordinal).Any(static group => group.Count() != 1))
            {
                throw new ArgumentException("Input provenance is duplicated.");
            }

            path = "analysis/inputs";
            var configuration = One(inputs, "configuration");
            path = configuration.RelativePath;
            var raw = JsonNode.Parse(configuration.Text!) as JsonObject ?? throw new ArgumentException("Consumer configuration is not an object.");
            var schemas = new Dictionary<string, JsonSchema>(StringComparer.Ordinal);
            var registry = new SchemaRegistry { Fetch = static (_, _) => null };
            var options = new BuildOptions { SchemaRegistry = registry };
            foreach (var input in inputs.Where(static input => input.Kind.StartsWith("schema:", StringComparison.Ordinal)))
            {
                cancellationToken.ThrowIfCancellationRequested();
                path = input.RelativePath;
                schemas.Add(input.Kind.Substring("schema:".Length), JsonSchema.FromText(input.Text!, options));
            }

            path = configuration.RelativePath;
            if (schemas.ContainsKey("configuration"))
            {
                Validate(schemas, "configuration", raw);
            }

            var suppliedMappings = inputs.Where(static input => input.Kind == "mappings").ToArray();
            if (suppliedMappings.Length > 1)
            {
                throw new ArgumentException("Analysis mappings are duplicated.");
            }

            path = suppliedMappings.Length == 1 ? suppliedMappings[0].RelativePath : configuration.RelativePath;
            var mapped = suppliedMappings.Length == 1 ? JsonNode.Parse(suppliedMappings[0].Text!)
                : raw["codeAnalysis"]?["mappings"];
            var fields = (mapped as JsonObject ?? throw new ArgumentException("Explicit analysis mappings are missing.")).ToDictionary(
                static entry => entry.Key,
                static entry => entry.Value?.GetValue<string>() ?? throw new ArgumentException("An explicit mapping is empty."), StringComparer.Ordinal);
            var projection = new AnalysisConfigurationReader().Read(raw, fields);
            projection["policy"] ??= raw["codeAnalysis"]?.DeepClone();
            var policy = new AnalysisPolicyReader().Read(projection);
            var settings = projection["policy"]?["records"];
            var records = new AnalysisRecordReader();
            var features = new List<BindingFeature>();
            foreach (var input in inputs.Where(static input => input.Kind == "feature"))
            {
                cancellationToken.ThrowIfCancellationRequested();
                path = input.RelativePath;
                var frontmatter = new FrontmatterTextReader().Read(input.Text!);
                Validate(schemas, "feature", frontmatter.Node);
                features.Add(records.Feature(input, policy, settings));
            }

            path = configuration.RelativePath;
            foreach (var selected in policy.Features)
            {
                if (!features.Any(feature => feature.Identity == selected))
                {
                    throw new ArgumentException("A selected consumer Feature input is missing.");
                }
            }

            var adrs = new List<BindingAdr>();
            foreach (var input in inputs.Where(static input => input.Kind == "adr"))
            {
                cancellationToken.ThrowIfCancellationRequested();
                path = input.RelativePath;
                var frontmatter = new FrontmatterTextReader().Read(input.Text!);
                if (schemas.ContainsKey("adr"))
                {
                    Validate(schemas, "adr", (JsonNode?)frontmatter.Node ?? JsonValue.Create(input.Text));
                }
                adrs.Add(records.Adr(input, policy, settings, features));
            }

            return new AnalysisSnapshot(contentKey, policy, new BindingCatalog(features, adrs), Array.Empty<AnalysisInputFailure>());
        }
        catch (Exception exception) when (exception is ArgumentException or InvalidOperationException or JsonException
            or JsonSchemaException or RefResolutionException or KeyNotFoundException)
        {
            var safe = "analysis/inputs";
            try
            {
                safe = _paths.Normalize(path);
            }
            catch (ArgumentException)
            {
            }

            return new AnalysisSnapshot(contentKey, null, null,
                new[] { new AnalysisInputFailure(safe, "Required consumer input is missing, invalid or incompatible with its explicit mappings.") });
        }
    }

    private static AnalysisInput One(IReadOnlyList<AnalysisInput> inputs, string kind)
    {
        var selected = inputs.Where(input => input.Kind == kind).ToArray();
        return selected.Length == 1 ? selected[0] : throw new ArgumentException("A required analysis input is absent or duplicated.");
    }

    private static void Validate(IReadOnlyDictionary<string, JsonSchema> schemas, string kind, JsonNode? value)
    {
        if (!schemas.TryGetValue(kind, out var schema)
            || !schema.Evaluate(JsonSerializer.SerializeToElement(value), new EvaluationOptions { RequireFormatValidation = true }).IsValid)
        {
            throw new ArgumentException("The supplied consumer schema rejects an input.");
        }
    }

    private static string Key(IReadOnlyList<AnalysisInput> inputs, CancellationToken cancellationToken)
    {
        using var bytes = new MemoryStream();
        using (var writer = new BinaryWriter(bytes, Encoding.UTF8, true))
        {
            foreach (var input in inputs.OrderBy(static input => input.RelativePath, StringComparer.Ordinal).ThenBy(static input => input.Kind,
                StringComparer.Ordinal).ThenBy(static input => input.Text, StringComparer.Ordinal))
            {
                cancellationToken.ThrowIfCancellationRequested();
                writer.Write(input.Kind);
                writer.Write(input.RelativePath);
                writer.Write(input.Text is not null);
                writer.Write(input.Text ?? string.Empty);
            }
        }

        using var sha = SHA256.Create();
        return BitConverter.ToString(sha.ComputeHash(bytes.ToArray())).Replace("-", string.Empty).ToLowerInvariant();
    }

    private readonly AnalysisPaths _paths = new();
}
