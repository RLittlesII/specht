namespace Specht.Analysis.Policy;

/// <summary>The current declaration facts supplied by the compiler and test adapter.</summary>
public sealed record AnalysisDeclaration(
    string Project, string TargetFramework, string RelativePath, string Kind, string SymbolId,
    bool Generated = false, bool RecognizedTest = false);
