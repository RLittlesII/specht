namespace Specht.Build;

/// <summary>What <see cref="SchemaVersionNumbering.VerifyAsync"/> found wrong with two neighbouring versions (0001-F7 B-040, B-066, B-088).</summary>
public enum SchemaVersionNumberingFault
{
    /// <summary>The higher holds a rule id the lower, 1.0.0 or higher, lacks, and its major is not greater (B-040).</summary>
    NewRuleWithoutHigherMajor,

    /// <summary>The higher holds a rule id the lower lacks, both below 1.0.0, and its minor is not greater (B-066).</summary>
    NewRuleWithoutHigherMinor,

    /// <summary>The two differ only in a template or a rule page's text, and not in their patch alone (B-088).</summary>
    TemplateOrRulePageChangeNotAPatch,
}
