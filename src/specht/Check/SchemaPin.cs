using Specht.Cqrs;
using Specht.Manifest;
using Specht.Model;
using Specht.Versioning;

namespace Specht.Check;

/// <summary>Pins a shipped version without rewriting other manifest bytes or consumer files.</summary>
public static class SchemaPin
{
    /// <summary>The root and target version.</summary>
    public sealed record Command(string Root, string Version) : ICommand<SemanticVersion>;

    /// <summary>Validates the inputs, edits in memory, and replaces the manifest through the I/O edge.</summary>
    public sealed class Handler(SpecModelReader reader, ManifestPinEditor editor, SchemaVersions versions)
        : ICommandHandler<Command, SemanticVersion>
    {
        /// <inheritdoc />
        public async Task<SemanticVersion> Handle(Command command, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var bytes = reader.ReadManifest(command.Root, required: true)!;
            editor.Validate(bytes);
            if (!SemanticVersion.TryParse(command.Version, out var target) || versions.Find(target) is null)
            {
                throw new SchemaVersionNotFoundException(command.Version, versions.Versions.Select(static version => version.Number));
            }
            var changed = editor.Change(bytes, target);
            if (!bytes.AsSpan().SequenceEqual(changed))
            {
                await reader.WriteManifest(command.Root, changed, cancellationToken);
            }
            return target;
        }
    }
}
