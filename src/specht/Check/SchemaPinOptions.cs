using Specht.Cqrs;
using Specht.Manifest;
using Specht.Model;
using Specht.Versioning;

namespace Specht.Check;

/// <summary>Validates the manifest before the host offers a choice, including repairable current pins.</summary>
public static class SchemaPinOptions
{
    /// <summary>The root whose manifest must be valid for pinning.</summary>
    public sealed record Query(string Root) : IQuery<IReadOnlyList<SemanticVersion>>;

    /// <summary>Returns shipped choices after root and non-version settings validation.</summary>
    public sealed class Handler(SpecModelReader reader, ManifestPinEditor editor, SchemaVersions versions)
        : IQueryHandler<Query, IReadOnlyList<SemanticVersion>>
    {
        /// <inheritdoc />
        public Task<IReadOnlyList<SemanticVersion>> Handle(Query query, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            editor.Validate(reader.ReadManifest(query.Root, required: true)!);
            return Task.FromResult<IReadOnlyList<SemanticVersion>>(versions.Versions.Select(static version => version.Number).ToArray());
        }
    }
}
