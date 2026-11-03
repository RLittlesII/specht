using System.Collections.ObjectModel;
using System.Text.RegularExpressions;
using Specht.Manifest;

namespace Specht.Model;

/// <summary>The qualified dependency edges of the Features with an identity, resolved once before rules run.</summary>
/// <implements epic="0001" feature="F1" claim="B-004" />
public sealed class SpechtDependencyGraph
{
    /// <summary>
    /// Resolves both edge directions, preserving the existing duplicate-identity overwrite and edge order: through the
    /// manifest's edge forms when it reads them, and otherwise as the engine at <c>d73b7a51</c> did (decision 0018).
    /// </summary>
    /// <param name="features">Features with both epic and id, in discovery order.</param>
    /// <param name="structure">The manifest's forms and grammars.</param>
    /// <implements epic="0001" feature="F1" claim="B-004" />
    /// <implements epic="0001" feature="F5" claim="B-005" />
    /// <implements epic="0001" feature="F5" claim="B-054" />
    public SpechtDependencyGraph(IReadOnlyList<FeatureSpec> features, SpechtStructure structure)
    {
        var forms = structure.Forms is { } declared ? new EdgeForms(declared, structure.Identifiers) : null;
        Identities = new ReadOnlySet<string>(features.Select(static feature => feature.Identity).ToHashSet(StringComparer.Ordinal));
        var dependsOn = new Dictionary<string, IReadOnlySet<string>>(StringComparer.Ordinal);
        var blocks = new Dictionary<string, IReadOnlySet<string>>(StringComparer.Ordinal);
        var unresolvedDependsOn = new Dictionary<string, IReadOnlySet<string>>(StringComparer.Ordinal);
        var unresolvedBlocks = new Dictionary<string, IReadOnlySet<string>>(StringComparer.Ordinal);
        foreach (var feature in features)
        {
            (dependsOn[feature.Identity], unresolvedDependsOn[feature.Identity]) = Resolve(feature, feature.Keys.DependsOn, forms);
            (blocks[feature.Identity], unresolvedBlocks[feature.Identity]) = Resolve(feature, feature.Keys.Blocks, forms);
        }

        DependsOn = new ReadOnlyDictionary<string, IReadOnlySet<string>>(dependsOn);
        Blocks = new ReadOnlyDictionary<string, IReadOnlySet<string>>(blocks);
        UnresolvedDependsOn = new ReadOnlyDictionary<string, IReadOnlySet<string>>(unresolvedDependsOn);
        UnresolvedBlocks = new ReadOnlyDictionary<string, IReadOnlySet<string>>(unresolvedBlocks);
    }

    /// <summary>The qualified identities.</summary>
    public IReadOnlySet<string> Identities { get; }

    /// <summary>Qualified depends_on edges by identity.</summary>
    public IReadOnlyDictionary<string, IReadOnlySet<string>> DependsOn { get; }

    /// <summary>Qualified blocks edges by identity.</summary>
    public IReadOnlyDictionary<string, IReadOnlySet<string>> Blocks { get; }

    /// <summary>The depends_on edges written in neither edge form, by identity, each as written (B-054).</summary>
    public IReadOnlyDictionary<string, IReadOnlySet<string>> UnresolvedDependsOn { get; }

    /// <summary>The blocks edges written in neither edge form, by identity, each as written (B-054).</summary>
    public IReadOnlyDictionary<string, IReadOnlySet<string>> UnresolvedBlocks { get; }

    /// <implements epic="0001" feature="F1" claim="B-004" />
    /// <implements epic="0001" feature="F5" claim="B-005" />
    /// <implements epic="0001" feature="F5" claim="B-054" />
    private static (IReadOnlySet<string> Resolved, IReadOnlySet<string> Unresolved) Resolve(FeatureSpec feature, string key, EdgeForms? forms)
    {
        var resolved = new HashSet<string>(StringComparer.Ordinal);
        var unresolved = new HashSet<string>(StringComparer.Ordinal);
        foreach (var raw in feature.Strings(key))
        {
            if (forms is null)
            {
                resolved.Add(raw.Contains('/') ? raw.Replace("/", "-") : $"{feature.Epic}-{raw}");
            }
            else if (forms.Qualified.Match(raw) is var qualified && qualified.Success)
            {
                resolved.Add(SpechtForms.Render(forms.Forms.Identity, qualified.Groups[EpicGroup].Value, qualified.Groups[IdGroup].Value));
            }
            else if (forms.Local.Match(raw) is var local && local.Success)
            {
                resolved.Add(SpechtForms.Render(forms.Forms.Identity, feature.Epic ?? string.Empty, local.Groups[IdGroup].Value));
            }
            else
            {
                unresolved.Add(raw);
            }
        }

        return (new ReadOnlySet<string>(resolved), new ReadOnlySet<string>(unresolved));
    }

    /// <summary>The two edge forms as anchored expressions, each placeholder a named group of its grammar.</summary>
    /// <param name="Forms">The forms.</param>
    /// <param name="Qualified">The qualified edge form, capturing the epic and the Feature id.</param>
    /// <param name="Local">The local edge form, capturing the Feature id.</param>
    /// <implements epic="0001" feature="F5" claim="B-005" />
    private sealed record EdgeForms(SpechtForms Forms, Regex Qualified, Regex Local)
    {
        /// <summary>The forms, <c>{epic}</c> read by the epic grammar and <c>{id}</c> by the Feature grammar.</summary>
        /// <param name="forms">The forms.</param>
        /// <param name="grammars">The manifest's grammars.</param>
        /// <implements epic="0001" feature="F5" claim="B-005" />
        public EdgeForms(SpechtForms forms, IReadOnlyDictionary<string, string> grammars)
            : this(
                forms,
                Expression(forms.QualifiedEdge, grammars["epic"], grammars["feature"]),
                Expression(forms.LocalEdge, null, grammars["feature"]))
        {
        }

        /// <implements epic="0001" feature="F5" claim="B-005" />
        private static Regex Expression(string form, string? epic, string feature)
        {
            var pattern = Regex.Escape(form).Replace(@"\" + SpechtForms.IdPlaceholder, Group(IdGroup, feature), StringComparison.Ordinal);
            if (epic is not null)
            {
                pattern = pattern.Replace(@"\" + SpechtForms.EpicPlaceholder, Group(EpicGroup, epic), StringComparison.Ordinal);
            }

            return new Regex(@"\A" + pattern + @"\z", RegexOptions.CultureInvariant);
        }

        /// <implements epic="0001" feature="F5" claim="B-005" />
        private static string Group(string name, string grammar) => $"(?<{name}>(?:{grammar.TrimStart('^').TrimEnd('$')}))";
    }

    private const string EpicGroup = "epic";

    private const string IdGroup = "id";
}
