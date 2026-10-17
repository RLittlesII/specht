using System.Text.Json.Nodes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Specht.Model;

namespace BindingSpike;

public static class ScenarioBridge
{
    public sealed record Link(string Project, string Target, string Source, int ScenarioLine, int? ExampleLine, string Method, int PickleIndex);

    public static object[] Run(string root, Catalog catalog, Action<bool, string> check)
    {
        var links = new List<Link>();
        var testCounts = new Dictionary<string, int>(StringComparer.Ordinal);
        foreach (var (project, folder, stem) in new[] { ("AlphaTests", "reqnroll", "alpha"), ("BetaTests", "reqnroll-beta", "beta") })
        {
            var obj = Path.Combine(root, folder, "obj", "Debug");
            var messageFile = Directory.EnumerateFiles(obj, stem + ".feature.ndjson", SearchOption.AllDirectories).Single();
            var sourceFile = Directory.EnumerateFiles(obj, stem + ".feature.cs", SearchOption.AllDirectories).Single();
            var envelopes = File.ReadAllLines(messageFile).Select(line => JsonNode.Parse(line.TrimStart('\uFEFF'))!).ToArray();
            var pickles = envelopes.Where(item => item["pickle"] is not null).Select(item => item["pickle"]!).ToArray();
            var document = envelopes.Single(item => item["gherkinDocument"] is not null)["gherkinDocument"]!;
            var scenarios = document["feature"]!["children"]!.AsArray().Select(item => item!["scenario"]!).ToArray();
            var rootSyntax = CSharpSyntaxTree.ParseText(File.ReadAllText(sourceFile)).GetRoot();
            var methods = rootSyntax.DescendantNodes().OfType<MethodDeclarationSyntax>().Where(method => method.AttributeLists.SelectMany(list => list.Attributes)
                .Any(attribute => attribute.Name.ToString().EndsWith(".FactAttribute", StringComparison.Ordinal) || attribute.Name.ToString().EndsWith(".TheoryAttribute", StringComparison.Ordinal))).ToArray();
            var linked = new HashSet<int>();
            var xmlBindings = 0;
            foreach (var method in methods)
            {
                xmlBindings += BindingReader.Xml(method).Elements("verifies").Count();
                var attributes = method.AttributeLists.SelectMany(list => list.Attributes).ToArray();
                var inline = attributes.Where(attribute => attribute.Name.ToString().EndsWith(".InlineDataAttribute", StringComparison.Ordinal)).ToArray();
                int[] indexes;
                if (inline.Length == 0)
                {
                    var index = method.DescendantNodes().OfType<VariableDeclaratorSyntax>().Single(variable => variable.Identifier.ValueText == "pickleIndex");
                    indexes = [int.Parse(((LiteralExpressionSyntax)index.Initializer!.Value).Token.ValueText, System.Globalization.CultureInfo.InvariantCulture)];
                }
                else
                {
                    var parameterIndex = method.ParameterList.Parameters.Select((parameter, index) => (parameter, index)).Single(pair => pair.parameter.Identifier.ValueText == "__pickleIndex").index;
                    indexes = inline.Select(attribute => int.Parse(((LiteralExpressionSyntax)attribute.ArgumentList!.Arguments[parameterIndex].Expression).Token.ValueText, System.Globalization.CultureInfo.InvariantCulture)).ToArray();
                }
                var categories = attributes.Concat(method.Ancestors().OfType<ClassDeclarationSyntax>().First().AttributeLists.SelectMany(list => list.Attributes))
                    .Where(attribute => attribute.Name.ToString().EndsWith(".TraitAttribute", StringComparison.Ordinal))
                    .Select(attribute => attribute.ArgumentList!.Arguments.Select(argument => ((LiteralExpressionSyntax)argument.Expression).Token.ValueText).ToArray())
                    .Where(pair => pair[0] == "Category").Select(pair => "@" + pair[1]).ToArray();
                foreach (var index in indexes)
                {
                    check(linked.Add(index), "A generated test row must map to one distinct pickle index.");
                    var pickle = pickles[index];
                    var tags = pickle["tags"]!.AsArray().Select(tag => tag!["name"]!.GetValue<string>()).ToArray();
                    var claim = new FeatureTagParser().Parse(string.Join("\n", tags), catalog.Structure).Single().Id;
                    var feature = tags.Single(tag => tag.StartsWith("@feature-", StringComparison.Ordinal))[9..];
                    var epic = tags.Single(tag => tag.StartsWith("@epic-", StringComparison.Ordinal))[6..];
                    var target = new Target(feature, epic, claim);
                    check(catalog.ById(feature, epic).Single().Claims.Contains(claim), "The generated test's tag must resolve to a declared claim.");
                    check(categories.Contains("@" + claim) && categories.Contains("@feature-" + feature) && categories.Contains("@epic-" + epic), "Generated test metadata and pickle tags must agree.");
                    var astIds = pickle["astNodeIds"]!.AsArray().Select(id => id!.GetValue<string>()).ToArray();
                    var scenario = scenarios.Single(item => astIds.Contains(item["id"]!.GetValue<string>()));
                    var rows = scenario["examples"]!.AsArray().SelectMany(example => example!["tableBody"]!.AsArray()).ToArray();
                    var row = rows.SingleOrDefault(item => astIds.Contains(item!["id"]!.GetValue<string>()));
                    links.Add(new Link(project, target.Key, stem + ".feature", scenario["location"]!["line"]!.GetValue<int>(), row?["location"]?["line"]?.GetValue<int>(), method.Identifier.ValueText, index));
                }
            }
            check(linked.Count == pickles.Length, "Every generated pickle must have a test invocation; a scenario outline is not one invocation.");
            check(xmlBindings == 0, "Default Reqnroll generation does not emit the proposed XML binding contract.");
            testCounts[project] = linked.Count;
        }
        check(testCounts["AlphaTests"] == 3 && testCounts["BetaTests"] == 1, "The two projects must produce three and one test invocations.");
        string[] required = ["AlphaTests", "BetaTests"];
        string[] supplied = ["AlphaTests"];
        var missing = required.Except(supplied, StringComparer.Ordinal).ToArray();
        check(missing.SequenceEqual(["BetaTests"]), "Omitting a required project must keep scope incomplete.");
        var partialLinks = links.Where(link => supplied.Contains(link.Project)).ToArray();
        check(partialLinks.Length == 3 && partialLinks.All(link => link.Target != "0001/F2/B-001"), "An omitted project's real generated tests must not be presented as supplied evidence.");
        return [new { Case = "generated-scenario-test-chain", Links = links, TestCounts = testCounts, XmlBindingsGenerated = 0, TitlesUsedForMatching = false },
            new { Case = "omitted-project", Required = required, Supplied = supplied, Missing = missing, SuppliedTestInvocations = partialLinks.Length, MissingTarget = "0001/F2/B-001", Complete = missing.Length == 0, AppliesTo = "A, B and C" }];
    }
}
