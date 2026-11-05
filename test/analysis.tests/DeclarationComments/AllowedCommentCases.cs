namespace Specht.Analysis.Tests.DeclarationComments;

public sealed class AllowedCommentCases : TheoryData<string>
{
    public AllowedCommentCases()
    {
        Add("class Example { void Run() {} }");
        Add("/// <summary>Example.</summary>\nclass Example {\n/// <summary>Run.</summary>\nvoid Run() {} }");
        Add("/// <spec epic=\"0004\" feature=\"F1\" />\ninternal static class Example {} ");
        Add("class Example {\n/// <implements feature=\"F1\" claim=\"B-001\" />\nvoid Run() {} }");
        Add("class Example { void Run() {\n// body\n} }");
        Add("class Example { string Run() =>\n// expression\n\"// literal\"; }");
        Add("class Example { void Run() {} // trailing method\n} // trailing class\n");
        Add("class Example {\n// constructor\nExample() {} }");
        Add("class Example {\n// property\nint Value => 1; }");
        Add("class Example {\n// field\nint Value; }");
        Add("class Example { void Run() {\n// local\nvoid Local() {} } }");
        Add("// record\nrecord Example;");
        Add("// record class\nrecord class Example;");
        Add("// struct\nstruct Example {}");
        Add("// interface\ninterface Example {}");
        Add("// delegate\ndelegate void Example();");
        Add("// enum\nenum Example { First }");
        Add("#pragma warning disable CS1591 // suppression reason\nclass Example { void Run() {} }");
        Add("class Example {\n#pragma warning disable CS1591 // suppression reason\nvoid Run() {} }");
        Add("/* block */ class Example { /* block */ void Run() {} }");
        Add("// file header\n// more header\n\nclass Example { void Run() {} }");
        Add("// header\n \t\n/// <spec feature=\"F1\" />\nclass Example {}");
        Add("// header\n\nusing System;\nclass Example {}");
        Add("class First {} // previous token\nclass Example { void First() {} // previous method\nvoid Run() {} }");
        Add("class Example { void Run() { }\n// class members\n}");
    }
}
