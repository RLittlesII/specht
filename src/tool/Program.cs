using System;
using Microsoft.Extensions.DependencyInjection;
using Specht.Check;
using Specht.Composition;
using Specht.Evaluation;
using Specht.Manifest;
using Specht.Tool;
using Specht.Tool.Features.Check;
using Specht.Tool.Features.Evidence;
using Specht.Tool.Features.Facts;
using Specht.Tool.Features.Frontmatter;
using Specht.Tool.Features.Init;
using Specht.Tool.Features.Manifest;
using Specht.Tool.Features.Mint;
using Specht.Tool.Features.Rules;
using Specht.Tool.Features.Schema;
using Specht.Tool.Features.Upgrade;
using Spectre.Console.Cli;

var services = new ServiceCollection();
services.AddSpechtEngine();
services.AddSingleton<Func<string, SchemaSourceSelection?, SpechtReport>>(static provider => provider.GetRequiredService<SpechtCheck>().Run);
services.AddInit();
services.AddUpgrade();
services.AddMint();
services.AddSingleton<EvidenceReconciler>();

var app = new CommandApp(new TypeRegistrar(services));
app.SetDefaultCommand<CheckCommand>();
app.Configure(static config =>
{
    config.SetApplicationVersion(ThisAssembly.AssemblyInformationalVersion)
        .SetHelpProvider(new SpechtHelpProvider(config.Settings));
    config.AddInit().AddUpgrade().AddSchema().AddFrontmatter().AddRules().AddManifest().AddMint().AddFacts();
    config.AddCommand<EvidenceCommand>("evidence");
});

return await app.RunAsync(args);
