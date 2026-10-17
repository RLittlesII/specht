using Spectre.Console.Cli;

namespace Specht.Tool.Features.Frontmatter;

/// <summary>Registers the frontmatter command.</summary>
public static class FrontmatterRegistration
{
    /// <summary>Adds the standalone document command and its help description.</summary>
    public static IConfigurator AddFrontmatter(this IConfigurator configuration)
    {
        configuration.AddCommand<FrontmatterCommand>("frontmatter").WithDescription("Check one document's frontmatter against its kind's schema.");
        return configuration;
    }
}
