using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Specht.Acceptance.Adoption;

internal sealed class AdoptionOptions : AnalyzerConfigOptionsProvider
{
    public override AnalyzerConfigOptions GlobalOptions => _empty;
    public override AnalyzerConfigOptions GetOptions(SyntaxTree tree) => _empty;
    public override AnalyzerConfigOptions GetOptions(AdditionalText textFile) => textFile is AdoptionFile file
        ? new AdoptionFileOptions(new Dictionary<string, string>
        {
            ["build_metadata.AdditionalFiles.SpechtInputKind"] = file.Kind,
            ["build_metadata.AdditionalFiles.SpechtRelativePath"] = file.Path,
        }) : _empty;

    private readonly AnalyzerConfigOptions _empty = new AdoptionFileOptions(new Dictionary<string, string>());
}
