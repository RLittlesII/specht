using System.Security.Cryptography;
using System.Text;
using AwesomeAssertions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;
using Specht.Analysis.TestEvidence;

namespace Specht.Analysis.Tests.TestEvidence;

/// <summary>Source hashes equal the bytes on disk, so reconciliation does not report generated sources stale (0201-F5 B-019).</summary>
[Trait("Tier", "Unit")]
public sealed class TestEvidenceCompilationInputsUnitTests
{
    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void ASourceWhoseEncodingEmitsAByteOrderMarkOrNot_WhenRead_ShouldHashTheFileBytes(bool emitsByteOrderMark)
    {
        // Given
        const string content = "namespace Generated { public class Case { } }";
        var encoding = new UTF8Encoding(emitsByteOrderMark);
        var fileBytes = encoding.GetPreamble().Concat(Encoding.UTF8.GetBytes(content)).ToArray();
        var root = Path.GetFullPath("consumer");
        var tree = CSharpSyntaxTree.ParseText(SourceText.From(content, encoding), path: Path.Combine(root, "obj", "Case.g.cs"),
            cancellationToken: TestContext.Current.CancellationToken);

        // When
        var inputs = new TestEvidenceCompilationInputs().Read(root, [], [tree], new EmptyOptions(), TestContext.Current.CancellationToken);

        // Then
        inputs.Should().ContainSingle().Which.Sha256.Should().Be(Convert.ToHexStringLower(SHA256.HashData(fileBytes)));
    }

    private sealed class EmptyOptions : AnalyzerConfigOptionsProvider
    {
        public override AnalyzerConfigOptions GlobalOptions => new EmptyConfig();

        public override AnalyzerConfigOptions GetOptions(SyntaxTree tree) => new EmptyConfig();

        public override AnalyzerConfigOptions GetOptions(AdditionalText textFile) => new EmptyConfig();
    }

    private sealed class EmptyConfig : AnalyzerConfigOptions
    {
        public override bool TryGetValue(string key, out string value)
        {
            value = string.Empty;
            return false;
        }
    }
}
