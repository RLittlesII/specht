using System;
using Microsoft.Extensions.DependencyInjection;
using Specht;
using Specht.Composition;
using Specht.Manifest;
using Specht.Tool;
using Specht.Tool.Features.Check;
using Specht.Tool.Features.Evidence;
using Specht.Tool.Features.Frontmatter;
using Specht.Tool.Features.Init;
using Specht.Tool.Features.Rules;
using Specht.Tool.Features.Schema;
using Specht.Tool.Features.Upgrade;
using Spectre.Console.Cli;

var services = new ServiceCollection();
services.AddSpechtEngine();
services.AddSingleton<Func<string, SchemaSourceSelection?, SpechtReport>>(static provider => provider.GetRequiredService<SpechtRunner>().Run);
services.AddInit();
services.AddUpgrade();
services.AddSingleton<EvidenceReconciler>();

var app = new CommandApp(new TypeRegistrar(services));
app.SetDefaultCommand<CheckCommand>();
app.Configure(static config =>
{
    config.SetApplicationVersion(ThisAssembly.AssemblyInformationalVersion);
    config.AddInit().AddUpgrade().AddSchema().AddFrontmatter().AddRules();
    config.AddCommand<EvidenceCommand>("evidence");
});

return await app.RunAsync(args);
