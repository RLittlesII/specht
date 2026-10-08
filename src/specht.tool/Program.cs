using System;
using Microsoft.Extensions.DependencyInjection;
using specht;
using specht.tool;
using specht.tool.Features.Check;
using Spectre.Console.Cli;

var services = new ServiceCollection();
services.AddSingleton<Func<string, SpecCheckReport>>(SpecCheckRunner.Run);

var app = new CommandApp(new TypeRegistrar(services));
app.SetDefaultCommand<CheckCommand>();

return await app.RunAsync(args);
