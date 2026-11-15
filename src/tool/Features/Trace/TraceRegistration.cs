using Spectre.Console.Cli;

namespace Specht.Tool.Features.Trace;

/// <summary>Registers the trace command and its help description.</summary>
/// <implements epic="0001" feature="F13" claim="B-001" />
public static class TraceRegistration
{
    /// <summary>Adds the trace command.</summary>
    /// <implements epic="0001" feature="F13" claim="B-001" />
    public static IConfigurator AddTrace(this IConfigurator configuration)
    {
        configuration.AddCommand<TraceCommand>("trace").WithDescription("Print where every claim stands.");
        return configuration;
    }
}
