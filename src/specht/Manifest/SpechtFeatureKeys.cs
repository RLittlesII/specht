namespace Specht.Manifest;

/// <summary>
/// The frontmatter key each role a rule reads in a Feature specification is written as (B-004, decisions 0010 and 0017);
/// each defaults to the name the engine read before schema 0.3.0, which every older pin keeps (B-049).
/// </summary>
/// <param name="Id">The Feature id.</param>
/// <param name="Epic">The epic the Feature belongs to.</param>
/// <param name="SpecStatus">The specification's maturity.</param>
/// <param name="Status">The Feature's delivery status.</param>
/// <param name="Children">The task ids the Feature declares.</param>
/// <param name="Spikes">The spike ids the Feature declares.</param>
/// <param name="DependsOn">The Features this one depends on.</param>
/// <param name="Blocks">The Features this one blocks.</param>
/// <implements epic="0001" feature="F5" claim="B-004" />
public sealed record SpechtFeatureKeys(
    string Id = "id",
    string Epic = "epic",
    string SpecStatus = "spec_status",
    string Status = "status",
    string Children = "children",
    string Spikes = "spikes",
    string DependsOn = "depends_on",
    string Blocks = "blocks");
