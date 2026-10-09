using System;
using System.IO.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using specht;
using specht.tool;
using specht.tool.Features.Check;
using specht.tool.Features.Init;
using Spectre.Console.Cli;

var services = new ServiceCollection();
services.AddSingleton<Func<string, SpecCheckReport>>(SpecCheckRunner.Run);
services.AddSingleton(static _ => new InitWriter(InitWriter.ShippingCopy(typeof(InitWriter).Assembly), new FileSystem()));

var app = new CommandApp(new TypeRegistrar(services));
app.SetDefaultCommand<CheckCommand>();
app.Configure(static config =>
    config.AddCommand<InitCommand>("init")
        .WithDescription("Write the newest schema set and templates under .spec/, never overwriting a file."));

return await app.RunAsync(args);
