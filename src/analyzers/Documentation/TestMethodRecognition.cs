using System.Collections.Generic;

namespace Specht.Analysis.Documentation;

/// <summary>The recognised test-method symbol ids and whether a current artifact established them.</summary>
public sealed record TestMethodRecognition(HashSet<string> Methods, bool Authoritative);
