using System.Collections.Immutable;
using System.Text.Json;
using System.Text.Json.Nodes;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Specht.Model;

namespace BindingSpike;

public static class Program
{
    private static readonly List<object> Evidence = [];
    private static int assertions;
    private static void Check(bool valid, string message)
    {
        assertions++;
        if (!valid) throw new InvalidOperationException(message);
    }

    public static async Task Main(string[] args)
    {
        var fixture = Fixture.Create();
        foreach (var candidate in Enum.GetValues<Candidate>())
        {
            var cohesive = fixture.Class(candidate, string.Join("\n", Enumerable.Range(1, 12).Select(index => Fixture.Method(fixture.Tag(candidate), "Check" + index))));
            await Observe("cohesive", candidate, fixture, cohesive, Enumerable.Repeat(fixture.Alpha.Key, 12).ToArray());
            var mixed = fixture.Class(candidate, Fixture.Method(fixture.Tag(candidate), "Alpha") + Fixture.Method(fixture.Tag(candidate, beta: true, explicitMember: true), "Beta"));
            await Observe("mixed-one-class", candidate, fixture, mixed, [fixture.Alpha.Key, fixture.Beta.Key], knownMismatch: candidate == Candidate.A);
            if (candidate == Candidate.A)
            {
                var split = fixture.Class(candidate, Fixture.Method(fixture.Tag(candidate), "Alpha"), name: "AlphaChecks") + fixture.Class(candidate, Fixture.Method(fixture.Tag(candidate), "Beta"), beta: true, name: "BetaChecks");
                await Observe("mixed-split-classes", candidate, fixture, split, [fixture.Alpha.Key, fixture.Beta.Key]);
            }
            var moved = fixture.Class(candidate, Fixture.Method(fixture.Tag(candidate)), beta: true);
            await Observe("member-moved", candidate, fixture, moved, [fixture.Alpha.Key], knownMismatch: candidate != Candidate.B);
            var relocated = fixture with { Files = fixture.Files.Select(file => file.Path == "src/alpha/.spec/README.md" ? new TextFile("src/relocated/.spec/README.md", file.GetText()!.ToString()) : file).ToImmutableArray() };
            await Observe("specification-moved", candidate, relocated, fixture.Class(candidate, Fixture.Method(fixture.Tag(candidate))),
                candidate == Candidate.A ? [] : [fixture.Alpha.Key], candidate == Candidate.A ? ["SPK004"] : []);
            foreach (var custom in new[] { false, true })
                foreach (var epics in new[] { false, true })
                {
                    var variant = Fixture.Create(custom, epics);
                    await Observe($"grammar-custom-{custom}-epics-{epics}", candidate, variant,
                        variant.Class(candidate, Fixture.Method(variant.Tag(candidate))), [variant.Alpha.Key]);
                }
            await Observe("unknown-claim", candidate, fixture, fixture.Class(candidate, Fixture.Method(fixture.Tag(candidate).Replace("B-001", "B-999"))), [], ["SPK004"]);
            await Observe("malformed-claim", candidate, fixture, fixture.Class(candidate, Fixture.Method(fixture.Tag(candidate).Replace("B-001", "wrong"))), [], ["SPK003"]);
            if (candidate != Candidate.A)
            {
                await Observe("malformed-feature", candidate, fixture, fixture.Class(candidate, Fixture.Method(fixture.Tag(candidate, explicitMember: true).Replace("feature=\"F1\"", "feature=\"wrong\""))), [], ["SPK003"]);
                await Observe("unknown-feature", candidate, fixture, fixture.Class(candidate, Fixture.Method(fixture.Tag(candidate, explicitMember: true).Replace("feature=\"F1\"", "feature=\"F999\""))), [], ["SPK004"]);
                await Observe("wrong-owning-epic", candidate, fixture, fixture.Class(candidate, Fixture.Method(fixture.Tag(candidate, explicitMember: true).Replace("epic=\"0001\"", "epic=\"0002\""))), [], ["SPK004"]);
                await Observe("missing-feature-scope", candidate, fixture, "public class Checks {\n" + Fixture.Method("<verifies claim=\"B-001\" />") + "}", [], [candidate == Candidate.B ? "SPK001" : "SPK004"]);
            }
            await Observe("missing-binding", candidate, fixture, fixture.Class(candidate, Fixture.Method("<remarks>Keep this.</remarks>")), [], ["SPK001"]);
            await Observe("malformed-xml", candidate, fixture, fixture.Class(candidate, Fixture.Method("<verifies")), [], ["SPK002"]);
            await Observe("duplicate-binding", candidate, fixture, fixture.Class(candidate, Fixture.Method(fixture.Tag(candidate) + "\n/// " + fixture.Tag(candidate))), [fixture.Alpha.Key], ["SPK006"]);
            await Observe("multiple-claims", candidate, fixture, fixture.Class(candidate, Fixture.Method(fixture.Tag(candidate) + "\n/// " + fixture.Tag(candidate, second: true))), [fixture.Alpha.Key, fixture.AlphaOther.Key]);
            var duplicateSpecs = fixture with { Files = fixture.Files.Add(new TextFile("src/copy/.spec/README.md", fixture.Files[1].GetText()!.ToString())) };
            await Observe("duplicate-spec-identity", candidate, duplicateSpecs, fixture.Class(candidate, Fixture.Method(fixture.Tag(candidate))),
                candidate == Candidate.A ? [fixture.Alpha.Key] : [], candidate == Candidate.A ? [] : ["SPK005"]);
            var noManifest = fixture with { Files = fixture.Files.RemoveAt(0) };
            await Observe("missing-manifest", candidate, noManifest, fixture.Class(candidate, Fixture.Method(fixture.Tag(candidate))), [], ["SPK008"]);
            var invalidRegex = fixture with { Files = fixture.Files.SetItem(0, new TextFile(".specht/manifest.json", fixture.Files[0].GetText()!.ToString().Replace("^B-[0-9]{3}$", "["))) };
            await Observe("invalid-manifest-regex", candidate, invalidRegex, fixture.Class(candidate, Fixture.Method(fixture.Tag(candidate))), [], ["SPK008"]);
            if (candidate != Candidate.B)
            {
                var first = fixture.Scope(candidate) + "public partial class Checks {" + Fixture.Method(fixture.Tag(candidate)) + "}\n";
                var partialSame = first + "public partial class Checks {}\n";
                await Observe("partial-scope", candidate, fixture, partialSame, [fixture.Alpha.Key]);
                var partialConflict = first + fixture.Scope(candidate, beta: true) + "public partial class Checks {}\n";
                await Observe("partial-conflict", candidate, fixture, partialConflict, [], ["SPK007"]);
                var nested = fixture.Scope(candidate) + "public class Outer { public class Inner {\n" + Fixture.Method(fixture.Tag(candidate)) + "}}";
                await Observe("nested-scope", candidate, fixture, nested, [fixture.Alpha.Key]);
            }
            if (candidate == Candidate.C)
            {
                await Observe("explicit-member-override", candidate, fixture, fixture.Class(candidate, Fixture.Method(fixture.Tag(candidate, beta: true, explicitMember: true))), [fixture.Beta.Key]);
                await Observe("explicit-member-retains-target-after-move", candidate, fixture, fixture.Class(candidate, Fixture.Method(fixture.Tag(candidate, explicitMember: true)), beta: true), [fixture.Alpha.Key]);
                var nestedOverride = fixture.Scope(candidate) + "public class Outer {\n" + fixture.Scope(candidate, beta: true) + "public class Inner {\n" + Fixture.Method(fixture.Tag(candidate)) + "}}";
                await Observe("nearest-nested-scope", candidate, fixture, nestedOverride, [fixture.Beta.Key]);
            }
            var wrongTarget = fixture.Class(candidate, Fixture.Method(fixture.Tag(candidate, beta: true, explicitMember: true)), beta: true);
            await Observe("wrong-but-existing-negative-control", candidate, fixture, wrongTarget, [fixture.Alpha.Key], knownMismatch: true);
            await Fix(candidate, fixture, scoped: true);
            await Fix(candidate, fixture, scoped: false);
        }
        await Materialize(fixture);
        var customFixture = Fixture.Create(custom: true, epics: false);
        var tags = new FeatureTagParser().Parse("@REQ-AU-054\nScenario: Synthetic\n Given a value\n", customFixture.Catalog.Structure);
        Check(tags.Count == 1 && tags[0].Id == customFixture.Claim, "The existing claim-tag parser must honor custom grammar.");
        Evidence.Add(new { Case = "custom-gherkin-tag", Parser = "Specht.Model.FeatureTagParser", Tags = tags.Select(tag => tag.Id) });
        if (args.Contains("--bridge")) Evidence.AddRange(ScenarioBridge.Run("epics/0201-spec-code-analysis/spikes/0236", fixture.Catalog, Check));
        if (args.Contains("--frameworks")) Evidence.Add(await FrameworkEvidence.Run(fixture, Check));
        var output = args.Length == 0 ? ".artifacts/0236/bindings.json" : args[0];
        Directory.CreateDirectory(Path.GetDirectoryName(output)!);
        var json = JsonSerializer.Serialize(new { Assertions = assertions, Cases = Evidence.Count, Roslyn = typeof(DiagnosticAnalyzer).Assembly.GetName().Version!.ToString(), Runtime = System.Runtime.InteropServices.RuntimeInformation.FrameworkDescription, Evidence }, new JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText(output, json);
        Console.WriteLine($"{assertions} assertions passed; {Evidence.Count} observations written to {output}.");
    }

    private static async Task Observe(string name, Candidate candidate, Fixture fixture, string source, string[] intended, string[]? expectedDiagnostics = null, bool knownMismatch = false)
    {
        using var workspace = new AdhocWorkspace();
        var document = fixture.Document(source, workspace);
        var compilation = (await document.Project.GetCompilationAsync())!;
        Check(!compilation.GetDiagnostics().Any(diagnostic => diagnostic.Severity == DiagnosticSeverity.Error), name + ": fixture must compile");
        var diagnostics = await compilation.WithAnalyzers([new BindingAnalyzer(candidate)], new AnalyzerOptions(fixture.Files)).GetAnalyzerDiagnosticsAsync();
        Check(!diagnostics.Any(diagnostic => diagnostic.Id == "AD0001"), name + ": analyzer must not crash");
        var ids = diagnostics.Select(diagnostic => diagnostic.Id).Order(StringComparer.Ordinal).ToArray();
        Check(ids.SequenceEqual((expectedDiagnostics ?? []).Order(StringComparer.Ordinal)), $"{name}/{candidate}: diagnostics {string.Join(',', ids)}");
        Check(diagnostics.All(diagnostic => diagnostic.Id == "SPK008" || diagnostic.Location.GetLineSpan().Path == "test/Bindings.cs"), name + ": diagnostic source location");
        var declaredMethods = (await document.GetSyntaxRootAsync())!.DescendantNodes().OfType<MethodDeclarationSyntax>().ToArray();
        Check(diagnostics.All(diagnostic => diagnostic.Id == "SPK008" || declaredMethods.Any(method => method.Identifier.Span == diagnostic.Location.SourceSpan)), name + ": diagnostic must identify the bound method");
        var actual = new List<string>();
        var root = (await document.GetSyntaxRootAsync())!;
        if (!ids.Contains("SPK008"))
        {
            var model = (await document.GetSemanticModelAsync())!;
            foreach (var method in root.DescendantNodes().OfType<MethodDeclarationSyntax>())
                actual.AddRange(BindingReader.Read(method, model, fixture.Catalog, candidate).Targets.Select(target => target.Key));
        }
        var mismatch = !actual.Order(StringComparer.Ordinal).SequenceEqual(intended.Order(StringComparer.Ordinal));
        Check(mismatch == knownMismatch, $"{name}/{candidate}: targets {string.Join(',', actual)} versus {string.Join(',', intended)}");
        var xml = root.DescendantNodes().OfType<MemberDeclarationSyntax>().SelectMany(node => node.GetLeadingTrivia()).Where(trivia => trivia.GetStructure() is DocumentationCommentTriviaSyntax).Select(trivia => trivia.ToFullString()).ToArray();
        Evidence.Add(new
        {
            Case = name,
            Candidate = candidate.ToString(),
            Intended = intended,
            Actual = actual,
            Diagnostics = ids,
            KnownTargetMismatch = mismatch,
            DiagnosticLocations = diagnostics.Select(diagnostic => new { diagnostic.Id, Path = diagnostic.Location.GetLineSpan().Path, Line = diagnostic.Location.GetLineSpan().StartLinePosition.Line + 1 }),
            Types = root.DescendantNodes().OfType<ClassDeclarationSyntax>().Count(),
            XmlCharacters = xml.Sum(value => value.Length),
            ScopeDeclarations = xml.Sum(value => value.Split("<spec ", StringSplitOptions.None).Length - 1),
            ExplicitMemberIdentities = xml.Sum(value => value.Split("<verifies feature=", StringSplitOptions.None).Length - 1),
        });
    }

    private static async Task Fix(Candidate candidate, Fixture fixture, bool scoped)
    {
        using var workspace = new AdhocWorkspace();
        var summary = "/// <summary>Keep my exact summary.</summary>\n/// <remarks>Keep &amp; preserve.</remarks>\n";
        var source = (scoped ? fixture.Scope(candidate) : "") + "public class Checks {\n" + summary + "[Xunit.Fact]\npublic void Check() {}\n}";
        var document = fixture.Document(source, workspace);
        var compilation = (await document.Project.GetCompilationAsync())!;
        var diagnostics = await compilation.WithAnalyzers([new BindingAnalyzer(candidate)], new AnalyzerOptions(fixture.Files)).GetAnalyzerDiagnosticsAsync();
        var actions = new List<CodeAction>();
        var provider = new BindingFixes(candidate, fixture.Catalog);
        await provider.RegisterCodeFixesAsync(new CodeFixContext(document, diagnostics.Single(), (action, _) => actions.Add(action), CancellationToken.None));
        var expected = candidate == Candidate.A && !scoped ? 0 : candidate != Candidate.B && scoped ? 2 : 3;
        Check(actions.Count == expected, $"{candidate}/fix scope {scoped}: {actions.Count} choices, expected {expected}");
        if (actions.Count > 0)
        {
            var action = actions.Single(value => value.EquivalenceKey == fixture.Alpha.Key);
            var applied = (await action.GetOperationsAsync(CancellationToken.None)).OfType<ApplyChangesOperation>().Single().ChangedSolution.GetDocument(document.Id)!;
            var updated = (await applied.GetTextAsync()).ToString();
            Check(updated.Contains("Keep my exact summary.", StringComparison.Ordinal) && updated.Contains("Keep &amp; preserve.", StringComparison.Ordinal), "A fix must preserve existing documentation.");
            var after = await (await applied.Project.GetCompilationAsync())!.WithAnalyzers([new BindingAnalyzer(candidate)], new AnalyzerOptions(fixture.Files)).GetAnalyzerDiagnosticsAsync();
            Check(after.Length == 0, "The applied binding fix must reanalyze cleanly.");
            var method = (await applied.GetSyntaxRootAsync())!.DescendantNodes().OfType<MethodDeclarationSyntax>().Single();
            var targets = BindingReader.Read(method, (await applied.GetSemanticModelAsync())!, fixture.Catalog, candidate).Targets;
            Check(targets.Single() == fixture.Alpha, "The fix must bind the explicitly selected record.");
        }
        Evidence.Add(new { Case = "missing-binding-code-fix", Candidate = candidate.ToString(), Scoped = scoped, Choices = actions.Select(action => action.EquivalenceKey).ToArray(), AutoSelected = false });
    }

    private static async Task Materialize(Fixture fixture)
    {
        using var workspace = new AdhocWorkspace();
        var source = fixture.Class(Candidate.C, Fixture.Method(fixture.Tag(Candidate.C)));
        var document = fixture.Document(source, workspace);
        var compilation = (await document.Project.GetCompilationAsync())!;
        var diagnostics = await compilation.WithAnalyzers([new BindingAnalyzer(Candidate.C, requireExplicit: true)], new AnalyzerOptions(fixture.Files)).GetAnalyzerDiagnosticsAsync();
        Check(diagnostics.Length == 1 && diagnostics[0].Id == "SPK009" && diagnostics[0].Severity == DiagnosticSeverity.Error,
            "The hardened hybrid must reject inherited-only evidence before a move.");
        var actions = new List<CodeAction>();
        await new BindingFixes(Candidate.C, fixture.Catalog).RegisterCodeFixesAsync(new CodeFixContext(document, diagnostics.Single(), (action, _) => actions.Add(action), CancellationToken.None));
        Check(actions.Count == 1, "A uniquely resolved inherited binding must have one materialization fix.");
        var action = actions.Single();
        var applied = (await action.GetOperationsAsync(CancellationToken.None)).OfType<ApplyChangesOperation>().Single().ChangedSolution.GetDocument(document.Id)!;
        var updated = (await applied.GetTextAsync()).ToString();
        Check(updated.Contains("<verifies feature=", StringComparison.Ordinal), "Materialization must persist an explicit identity.");
        Check(updated.Contains("Synthetic behavior.", StringComparison.Ordinal), "Materialization must preserve summary text.");
        var after = await (await applied.Project.GetCompilationAsync())!.WithAnalyzers([new BindingAnalyzer(Candidate.C, requireExplicit: true)], new AnalyzerOptions(fixture.Files)).GetAnalyzerDiagnosticsAsync();
        Check(after.Length == 0, "Materialized bindings must satisfy the explicit-evidence policy.");
        var moved = updated.Replace(fixture.Scope(Candidate.C).Trim(), fixture.Scope(Candidate.C, beta: true).Trim(), StringComparison.Ordinal);
        await Observe("hybrid-materialized-before-move", Candidate.C, fixture, moved, [fixture.Alpha.Key]);
        Evidence.Add(new { Case = "materialization-code-action", Candidate = "C", Before = source, After = updated, MustRunBeforeMove = true, BlockingDiagnostic = diagnostics[0].Id, Severity = diagnostics[0].Severity.ToString(), DiagnosticsAfterFix = after.Length });
    }
}
