using Spectre.Console.Cli;

namespace Specht.Tool.Features.Frontmatter;

/// <summary>Registers the frontmatter command.</summary>
/// <implements epic="0001" feature="F8" claim="B-001" />
public static class FrontmatterRegistration
{
    /// <summary>Adds the standalone document command and its help description.</summary>
    /// <implements epic="0001" feature="F8" claim="B-001" />
    public static IConfigurator AddFrontmatter(this IConfigurator configuration)
    {
        configuration.AddCommand<FrontmatterCommand>("frontmatter").WithDescription("Check one document's frontmatter against its kind's schema.");
        return configuration;
    }
}
