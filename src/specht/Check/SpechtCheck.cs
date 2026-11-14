using Specht.Evaluation;
using Specht.Manifest;
using Specht.Model;

namespace Specht.Check;

/// <summary>
/// The engine's one check entry point (ADR-0026 § "The revised migration design"; item <c>0167</c>): reads the model under a
/// root and evaluates it, and does nothing else. The four input-failure exceptions cross it uncaught (ADR-0010 owner
/// decision 4).
/// </summary>
/// <param name="reader">The model reader.</param>
/// <param name="evaluator">The evaluator over the registered rule set.</param>
/// <implements epic="0001" feature="F1" claim="B-001" />
public sealed class SpechtCheck(SpechtModelReader reader, SpechtEvaluator evaluator)
{
    /// <summary>Reads the tree under <paramref name="root"/> with the frontmatter source selected for this invocation and evaluates it.</summary>
    /// <param name="root">The root to check.</param>
    /// <param name="selection">The schema source the caller selected, or <see langword="null"/> for the manifest's.</param>
    /// <returns>The report.</returns>
    /// <implements epic="0001" feature="F1" claim="B-001" />
    public SpechtReport Run(string root, SchemaSourceSelection? selection) => evaluator.Evaluate(reader.Read(root, selection));
}
