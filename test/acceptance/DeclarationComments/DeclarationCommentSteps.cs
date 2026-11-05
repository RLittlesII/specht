using System.Collections.Immutable;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using AwesomeAssertions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;
using Reqnroll;
using Specht.Analysis.DeclarationComments;

namespace Specht.Acceptance.DeclarationComments;

[Binding]
[Scope(Tag = "0201-F8")]
public sealed class DeclarationCommentSteps
{
    [Given("a {string} class declaration with an ordinary line comment on its surface")]
    public void GivenClass(string accessibility) => _source = accessibility == "private nested"
        ? "class Outer {\n// surface\nprivate class Inner {} }"
        : "// surface\n" + accessibility + " class Example {}";

    [Given("a method declaration in a {string} with an ordinary line comment on its surface")]
    public void GivenMethod(string owner) => _source = owner + " Example {\n// surface\npublic void Run() {} }";

    [Given("a class and method carrying only {string} on their surfaces")]
    public void GivenDocumentation(string documentation)
    {
        var xml = documentation switch
        {
            "XML summaries" => "/// <summary>Example.</summary>\n",
            "a traceability-only XML tag" => "/// <spec epic=\"0004\" feature=\"F1\" />\n",
            _ => string.Empty,
        };
        _source = xml + "class Example {\n" + xml + "void Run() {} }";
    }

    [Given("a class and method with an ordinary line comment {string}")]
    public void GivenHeaderPosition(string position)
    {
        var attributes = "[System.Obsolete]\n";
        var second = "[System.CLSCompliant(true)]\n";
        var header = position switch
        {
            "before attributes" => "// surface\n" + attributes + "public {0}",
            "between attributes" => attributes + "// surface\n" + second + "public {0}",
            "between attributes and modifiers" => attributes + "// surface\npublic {0}",
            "between modifiers" => "public\n// surface\n{0}",
            "within the signature" => "{0}\n// surface\n{1}",
            _ => "public {0}\n// surface\n",
        };
        if (position == "within the signature")
        {
            _source = string.Format(header, "class", "Example") + " {\n" + string.Format(header, "void", "Run()") + " {} }";
        }
        else
        {
            _source = string.Format(header, "class Example") + " {\n" + string.Format(header, "void Run()") + " {} }";
        }

        _expectedCount = 2;
    }

    [Given("a class and method with ordinary line comments {string} their XML documentation")]
    public void GivenMixedDocumentation(string position)
    {
        const string xml = "/// <spec feature=\"F1\" />\n";
        var comments = position switch
        {
            "before" => "// surface\n" + xml,
            "after" => xml + "// surface\n",
            _ => xml + "// surface\n" + xml,
        };
        _source = comments + "class Example {\n" + comments + "void Run() {} }";
        _expectedCount = 2;
    }

    [Given("ordinary comments only in {string}")]
    public void GivenOtherLocation(string location) => _source = location switch
    {
        "a method body" => "class Example { void Run() {\n// body\n} }",
        "an expression body" => "class Example { int Run() =>\n// expression\n1; }",
        "after a completed declaration" => "class Example { void Run() {} // trailing\n} // trailing\n",
        "a constructor surface" => "class Example {\n// constructor\nExample() {} }",
        "a property surface" => "class Example {\n// property\nint Value => 1; }",
        "a local function surface" => "class Example { void Run() {\n// local\nvoid Local() {} } }",
        "a record declaration surface" => "// record\nrecord Example;",
        "a struct declaration surface" => "// struct\nstruct Example {}",
        _ => "#pragma warning disable CS1591 // suppression explanation\nclass Example { void Run() {} }",
    };

    [Given("a file beginning with an ordinary comment block separated from its first code or documentation by an empty line")]
    public void GivenFileHeader() => _source = "// file header\n// more header\n\n";

    [Given("a class with an ordinary surface comment after that file header")]
    public void GivenClassAfterHeader() => _source += "// surface\nclass Example {}";

    [Given("a method with an ordinary surface comment separated from its signature by an empty line")]
    public void GivenSeparatedMethod() => _source = "class Example {\n// surface\n\nvoid Run() {} }";

    [Given("generated source identified by {string} carrying ordinary class and method surface comments")]
    public void GivenGenerated(string marker)
    {
        _source = "// surface\nclass Example {\n// surface\nvoid Run() {} }";
        _path = marker switch
        {
            "a .g.cs file name" => "src/Example.g.cs",
            "a .generated.cs file name" => "src/Example.generated.cs",
            _ => "src/Example.cs",
        };
        if (marker == "an auto-generated header")
        {
            _source = "// <auto-generated/>\n" + _source;
        }
    }

    [Given("source with an ordinary class surface comment and no manifest or specification inputs")]
    public void GivenNoInputs() => _source = "// surface\nclass Example {}";

    [Given("source with an ordinary method surface comment and no reporting override")]
    [Given("source with an ordinary method surface comment")]
    public void GivenDefaultMethod() => _source = "class Example {\n// surface\nvoid Run() {} }";

    [Given("{string} applied to the declaration-comment diagnostic")]
    public void GivenControl(string control)
    {
        _control = control;
        if (control == "a warning suppression pragma")
        {
            _source = "#pragma warning disable SPCT0013 // chosen exception\n" + _source;
        }
    }

    [When("declaration comments are analyzed")]
    public async Task WhenAnalyzed()
    {
        var tree = CSharpSyntaxTree.ParseText(_source, path: _path);
        var options = new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary);
        if (_control is "an error severity override" or "a disabled severity override")
        {
            options = options.WithSpecificDiagnosticOptions(ImmutableDictionary<string, ReportDiagnostic>.Empty.Add("SPCT0013",
                _control == "an error severity override" ? ReportDiagnostic.Error : ReportDiagnostic.Suppress));
        }

        var compilation = CSharpCompilation.Create("Example", [tree],
            [MetadataReference.CreateFromFile(typeof(object).Assembly.Location)], options);
        compilation.GetDiagnostics().Where(static diagnostic => diagnostic.Severity == DiagnosticSeverity.Error).Should().BeEmpty();
        _diagnostics = await compilation.WithAnalyzers([new DeclarationCommentAnalyzer()]).GetAnalyzerDiagnosticsAsync(CancellationToken.None);
    }

    [Then("the ordinary line comment is reported at its source span")]
    [Then("the ordinary line comments are reported at their source spans")]
    [Then("every ordinary surface line comment is reported once")]
    [Then("only the class surface comment is reported")]
    public void ThenReported()
    {
        var diagnostics = _diagnostics.OrderBy(static diagnostic => diagnostic.Location.SourceSpan.Start).ToArray();
        diagnostics.Should().HaveCount(_expectedCount);
        var comments = Regex.Matches(_source, @"// surface[^\r\n]*");
        comments.Should().HaveCount(_expectedCount);
        for (var index = 0; index < comments.Count; index++)
        {
            diagnostics[index].Id.Should().Be("SPCT0013");
            diagnostics[index].Location.SourceTree!.FilePath.Should().Be(_path);
            diagnostics[index].Location.SourceSpan.Should().Be(new TextSpan(comments[index].Index, comments[index].Length));
        }
    }

    [Then("no declaration-comment diagnostic is reported")]
    public void ThenNoDiagnostic() => _diagnostics.Should().BeEmpty();

    [Then("its declaration-comment diagnostic is an enabled warning")]
    public void ThenWarning()
    {
        ThenReported();
        _diagnostics.Single().Severity.Should().Be(DiagnosticSeverity.Warning);
        new DeclarationCommentAnalyzer().SupportedDiagnostics.Single().IsEnabledByDefault.Should().BeTrue();
    }

    [Then("the declaration-comment diagnostic follows that reporting control")]
    public void ThenReportingControl()
    {
        if (_control == "an error severity override")
        {
            ThenReported();
            _diagnostics.Single().Severity.Should().Be(DiagnosticSeverity.Error);
        }
        else
        {
            ThenNoDiagnostic();
        }
    }

    private string _source = string.Empty;
    private string _path = "src/Example.cs";
    private string? _control;
    private int _expectedCount = 1;
    private ImmutableArray<Diagnostic> _diagnostics;
}
