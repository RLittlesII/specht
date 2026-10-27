using Spectre.Console.Cli;

namespace Specht.Tool.Features.Rules;

/// <summary>Registers the rules branch and its default listing command.</summary>
/// <implements epic="0001" feature="F10" claim="B-001" />
public static class RulesRegistration
{
    /// <summary>Adds the rules branch and its help description.</summary>
    /// <implements epic="0001" feature="F10" claim="B-001" />
    public static IConfigurator AddRules(this IConfigurator configuration)
    {
        configuration.AddBranch<RulesCommand.Settings>("rules", static rules =>
        {
            rules.SetDescription("List pinned rules and their effective levels.");
            rules.SetDefaultCommand<RulesCommand>();
        });
        return configuration;
    }
}
