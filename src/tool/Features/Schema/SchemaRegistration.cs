using Specht.Tool.Features.Schema.List;
using Specht.Tool.Features.Schema.Pin;
using Spectre.Console.Cli;

namespace Specht.Tool.Features.Schema;

/// <summary>Registers the schema command branch.</summary>
public static class SchemaRegistration
{
    /// <summary>Adds the schema commands and their help descriptions.</summary>
    public static IConfigurator AddSchema(this IConfigurator configuration)
    {
        configuration.AddBranch("schema", static schema =>
        {
            schema.SetDescription("List and select shipped schema versions.");
            schema.AddCommand<SchemaListCommand>("list").WithDescription("List shipped schema versions and mark the repository's pin.");
            schema.AddCommand<SchemaPinCommand>("pin").WithDescription("Pin a shipped schema version, directly or interactively.");
        });
        return configuration;
    }
}
