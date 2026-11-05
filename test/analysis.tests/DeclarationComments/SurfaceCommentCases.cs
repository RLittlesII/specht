namespace Specht.Analysis.Tests.DeclarationComments;

public sealed class SurfaceCommentCases : TheoryData<string, string[]>
{
    public SurfaceCommentCases()
    {
        foreach (var modifier in new[] { "public", "internal", "partial" })
        {
            Add("// class\n" + modifier + " class Example {}", ["// class"]);
        }

        foreach (var modifier in new[] { "private", "protected", "protected internal", "private protected" })
        {
            Add("class Outer {\n// nested\n" + modifier + " class Inner {} }", ["// nested"]);
        }

        foreach (var owner in new[] { "class", "struct", "record", "interface" })
        {
            Add(owner + " Example {\n// method\npublic void Run() {} }", ["// method"]);
        }

        Add("abstract class Example {\n// abstract\nprotected abstract void Run(); }", ["// abstract"]);
        Add("interface IExample {\n// interface\nvoid Run(); }", ["// interface"]);
        Add("interface IExample { void Run(); }\nclass Example : IExample {\n// explicit\nvoid IExample.Run() {} }", ["// explicit"]);
        Add("partial class Example {\n// partial\npartial void Run();\n// implementation\npartial void Run() {} }",
            ["// partial", "// implementation"]);
        Add("class Example {\n// separated\n\npublic void Run() {} }", ["// separated"]);

        foreach (var documentation in new[]
                 {
                     "// ordinary\n/// <spec feature=\"F1\" />\n",
                     "/// <spec feature=\"F1\" />\n// ordinary\n",
                     "/// <summary>Example.</summary>\n// ordinary\n/// <spec feature=\"F1\" />\n",
                 })
        {
            Add(documentation + "class Example {}", ["// ordinary"]);
            Add("class Example {\n" + documentation + "void Run() {} }", ["// ordinary"]);
        }

        foreach (var declaration in new[]
                 {
                     "// before\n[System.Obsolete] public class Example {}",
                     "[System.Obsolete]\n// between\n[System.Serializable] public class Example {}",
                     "[System.Obsolete(\n// attribute\n\"Reason\")] public class Example {}",
                     "[System.Obsolete]\n// modifiers\npublic class Example {}",
                     "public\n// modifier\nsealed class Example {}",
                     "class\n// signature\nExample {}",
                     "class Example\n// delimiter\n{}",
                 })
        {
            var comment = declaration.Split('\n').Single(static line => line.StartsWith("//", StringComparison.Ordinal));
            Add(declaration, [comment]);
        }

        foreach (var declaration in new[]
                 {
                     "// before\n[System.Obsolete] public void Run() {}",
                     "[System.Obsolete]\n// between\n[System.CLSCompliant(true)] public void Run() {}",
                     "[System.Obsolete(\n// attribute\n\"Reason\")] public void Run() {}",
                     "[System.Obsolete]\n// modifiers\npublic void Run() {}",
                     "public\n// modifier\nstatic void Run() {}",
                     "void Run(\n// signature\nint value) {}",
                     "void Run()\n// delimiter\n{}",
                     "int Run()\n// arrow\n=> 1;",
                 })
        {
            var comment = declaration.Split('\n').Single(static line => line.StartsWith("//", StringComparison.Ordinal));
            Add("class Example {\n" + declaration + "\n}", [comment]);
        }

        Add("interface Example { void Run()\n// terminator\n; }", ["// terminator"]);
        Add("// class terminator\nclass Example;", ["// class terminator"]);
        Add("class Example(\n// primary constructor\nint value) {}", ["// primary constructor"]);
        Add("// first\n// second\nclass Example {\n// third\nvoid Run() {} }", ["// first", "// second", "// third"]);
        Add("// header\n\n// surface\nclass Example {}", ["// surface"]);
        Add("// header\n \t\n/// <summary>Example.</summary>\n// surface\nclass Example {}", ["// surface"]);
        Add("class Example {\r\n// windows\r\nvoid Run() {} }", ["// windows"]);
    }
}
