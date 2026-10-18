using System.Collections.Generic;

namespace Specht.Analysis.Policy;

/// <summary>Explicit-reference validation and mandatory authoring remain separate decisions.</summary>
public sealed record DeclarationPolicy(bool ValidateExplicit, IReadOnlyList<string> RequiredRoles, IReadOnlyList<string> ExemptionReasons);
