namespace Specht.Analysis.Tests.Documentation;

public sealed class TypeDefaultCases : TheoryData<string, string, string?>
{
    public TypeDefaultCases()
    {
        Add("/// <spec feature=\"cap:outer\" />\npublic class Outer { public class Inner { public void Run() {} } }",
            "M:Outer.Inner.Run", "cap:outer");
        Add("""
            /// <spec epic="0201" feature="cap:outer" />
            public class Outer
            {
                /// <spec feature="cap:inner" />
                public class Inner { public void Run() {} }
            }
            """, "M:Outer.Inner.Run", "cap:inner");
        Add("""
            /// <spec feature="cap:shared" />
            public partial class Example { }
            public partial class Example { public void Run() {} }
            """, "M:Example.Run", "cap:shared");
        Add("""
            /// <spec feature="cap:shared" epic="0201" />
            public partial class Example { }
            /// <spec epic="0201" feature="cap:shared"></spec>
            public partial class Example { }
            public partial class Example { public void Run() {} }
            """, "M:Example.Run", "cap:shared");
        Add("""
            /// <spec feature="cap:base" />
            public class Base { }
            public class Example : Base { public void Run() {} }
            """, "M:Example.Run", null);
        Add("""
            /// <spec feature="cap:base" />
            public class Base { }
            /// <spec feature="cap:outer" />
            public class Outer { public class Example : Base { public void Run() {} } }
            """, "M:Outer.Example.Run", "cap:outer");
        Add("/// <spec feature=\"cap:own\" />\npublic class Example {}", "T:Example", "cap:own");
    }
}
