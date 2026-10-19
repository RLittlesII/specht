using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Specht.Analysis.Bindings;

/// <summary>Validates explicit identities and resolves selected claim and ADR declarations without I/O.</summary>
/// <follows adr="ADR-0017" />
public sealed class BindingResolver
{
    public BindingResolver(BindingCatalog catalog, BindingPolicy policy)
    {
        _catalog = catalog;
        _policy = policy;
    }

    /// <implements epic="0201" feature="F1" claim="B-015" />
    public BindingResult Resolve(ClaimReference reference)
    {
        var fault = string.IsNullOrEmpty(reference.Claim) ? BindingResolution.Incomplete
            : OwnerFault(reference.Epic, reference.Feature);
        if (fault is not null)
        {
            return new BindingResult(fault.Value, null, null, []);
        }

        if (!Matches("claim", reference.Claim!))
        {
            return new BindingResult(BindingResolution.Invalid, null, null, []);
        }

        var owners = _catalog.Features.Where(feature => OwnerMatches(feature.Identity, reference.Epic, reference.Feature)).ToArray();
        var matches = owners.SelectMany(feature => feature.Claims.Where(claim => string.Equals(claim.Id, reference.Claim, StringComparison.Ordinal))
            .Select(claim => new BindingTarget(BindingTargetKind.Claim,
                _policy.DeclaresEpicGrammar ? feature.Identity.Epic : null, feature.Identity.Feature,
                claim.Id, claim.State, feature.State, feature.RelativePath, claim.Line)));
        return Result(matches, owners.Length > 1);
    }

    /// <implements epic="0201" feature="F1" claim="B-004" />
    public BindingResult Resolve(AdrReference reference)
    {
        var fault = string.IsNullOrEmpty(reference.Adr) ? BindingResolution.Incomplete
            : reference.Feature is not null ? OwnerFault(reference.Epic, reference.Feature)
            : reference.Epic is not null ? BindingResolution.Invalid : (BindingResolution?)null;
        if (fault is not null)
        {
            return new BindingResult(fault.Value, null, null, []);
        }

        if (string.IsNullOrWhiteSpace(reference.Adr) || _policy.Identifiers.ContainsKey("adr") && !Matches("adr", reference.Adr!))
        {
            return new BindingResult(BindingResolution.Invalid, null, null, []);
        }

        var owners = reference.Feature is null ? []
            : _catalog.Features.Where(feature => OwnerMatches(feature.Identity, reference.Epic, reference.Feature)).ToArray();
        var matches = _catalog.Adrs.Where(adr => string.Equals(adr.Id, reference.Adr, StringComparison.Ordinal)
                && (reference.Feature is null ? adr.Owner is null
                    : adr.Owner is not null && OwnerMatches(adr.Owner, reference.Epic, reference.Feature)))
            .Select(adr => new BindingTarget(BindingTargetKind.Adr,
                _policy.DeclaresEpicGrammar ? adr.Owner?.Epic : null, adr.Owner?.Feature,
                adr.Id, adr.State, owners.Length == 1 ? owners[0].State : null, adr.RelativePath, adr.Line));
        return Result(matches, owners.Length > 1);
    }

    public BindingTargetKind? ReadTargetKind(string kind) => kind switch
    {
        "claim" => BindingTargetKind.Claim,
        "adr" => BindingTargetKind.Adr,
        _ => null,
    };

    private BindingResolution? OwnerFault(string? epic, string? feature)
    {
        if (string.IsNullOrEmpty(feature) || _policy.DeclaresEpicGrammar && string.IsNullOrEmpty(epic))
        {
            return BindingResolution.Incomplete;
        }

        return !_policy.DeclaresEpicGrammar && epic is not null || !Matches("feature", feature!)
            || _policy.DeclaresEpicGrammar && !Matches("epic", epic!) ? BindingResolution.Invalid : null;
    }

    private bool Matches(string role, string value) => _policy.Identifiers.TryGetValue(role, out var grammar)
        && new Regex("\\A(?:" + grammar + ")\\z", RegexOptions.CultureInvariant).IsMatch(value);

    private bool OwnerMatches(FeatureIdentity owner, string? epic, string? feature) =>
        string.Equals(owner.Feature, feature, StringComparison.Ordinal)
        && (!_policy.DeclaresEpicGrammar || string.Equals(owner.Epic, epic, StringComparison.Ordinal));

    private BindingResult Result(IEnumerable<BindingTarget> matches, bool ambiguousOwner)
    {
        var ordered = matches.OrderBy(static target => target.RelativePath, StringComparer.Ordinal)
            .ThenBy(static target => target.Line).ThenBy(static target => target.Id, StringComparer.Ordinal)
            .ThenBy(static target => target.State, StringComparer.Ordinal).ThenBy(static target => target.FeatureState, StringComparer.Ordinal).ToArray();
        var status = ambiguousOwner || ordered.Length > 1 ? BindingResolution.Ambiguous
            : ordered.Length == 0 ? BindingResolution.Unresolved : BindingResolution.Resolved;
        var target = status == BindingResolution.Resolved ? ordered[0] : null;
        return new BindingResult(status, target, target is null ? null : Eligible(target), ordered);
    }

    private bool Eligible(BindingTarget target)
    {
        if (target.Kind == BindingTargetKind.Adr)
        {
            return target.State is null ? _policy.Omissions.AdrEligible : target.State == "accepted";
        }

        var claim = target.State is null ? _policy.Omissions.ClaimEligible
            : !Marker("withdrawn", target.State) && !Marker("superseded", target.State) && target.State is "Active" or "Amended";
        var feature = target.FeatureState is null ? _policy.Omissions.FeatureEligible
            : target.FeatureState is "draft" or "in-review" || Marker("approved", target.FeatureState);
        return claim && feature;
    }

    private bool Marker(string key, string state) =>
        _policy.Markers.TryGetValue(key, out var marker) && string.Equals(marker, state, StringComparison.Ordinal);

    private readonly BindingCatalog _catalog;
    private readonly BindingPolicy _policy;
}
