using Spectre.Console.Cli;

namespace Specht.Tool.Features.Facts;

/// <summary>Registers the facts command and its help description.</summary>
/// <implements epic="0132" feature="F2" claim="B-001" />
public static class FactsRegistration
{
    /// <summary>Adds the facts command.</summary>
    /// <implements epic="0132" feature="F2" claim="B-001" />
    public static IConfigurator AddFacts(this IConfigurator configuration)
    {
        configuration.AddCommand<FactsCommand>("facts").WithDescription("Print what the specification tree holds.");
        return configuration;
    }
}
