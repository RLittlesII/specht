using System.Text;
using System.Text.Json;
using Specht.Cqrs;
using Specht.Manifest;
using Specht.Model;
using Specht.Versioning;

namespace Specht.Check;

/// <summary>Lists the shipped catalog with the optional repository pin (0001-F7 B-041, B-084, B-085).</summary>
public static class SchemaList
{
    /// <summary>The root whose pin is marked when its manifest exists.</summary>
    public sealed record Query(string Root) : IQuery<Result>;

    /// <summary>The shipped versions and the optional pin.</summary>
    public sealed record Result(IReadOnlyList<SemanticVersion> Versions, SemanticVersion? Pinned);

    /// <summary>Validates the pin without loading schemas, documents or upstream sources.</summary>
    public sealed class Handler(SpecModelReader reader, SpecManifestParser parser, SchemaVersions versions) : IQueryHandler<Query, Result>
    {
        /// <inheritdoc />
        public Task<Result> Handle(Query query, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var bytes = reader.ReadManifest(query.Root, required: false);
            SemanticVersion? pin = null;
            if (bytes is not null)
            {
                try
                {
                    pin = parser.PinnedVersion(parser.Parse(Encoding.UTF8.GetString(bytes).TrimStart('\uFEFF'))).Number;
                }
                catch (Exception exception) when (exception is JsonException or InvalidOperationException or FormatException or ArgumentException)
                {
                    throw new SpechtManifestUnreadableException(exception);
                }
            }
            return Task.FromResult(new Result(versions.Versions.Select(static version => version.Number).ToArray(), pin));
        }
    }
}
