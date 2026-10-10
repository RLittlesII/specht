using System;
using System.IO.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using Specht;
using Specht.Tool;
using Specht.Tool.Features.Check;
using Specht.Tool.Features.Init;
using Spectre.Console.Cli;
using InitWriter = Specht.Tool.Features.Init.InitWriter;

var services = new ServiceCollection();
services.AddSingleton<Func<string, SpechtReport>>(SpechtRunner.Run);
services.AddSingleton(static _ => new InitWriter(InitWriter.ShippingCopy(typeof(InitWriter).Assembly), new FileSystem()));

var app = new CommandApp(new TypeRegistrar(services));
app.SetDefaultCommand<CheckCommand>();
app.Configure(static config =>
    config.AddCommand<InitCommand>("init")
        .WithDescription("Write the newest schema set and templates under .spec/, never overwriting a file."));

return await app.RunAsync(args);
