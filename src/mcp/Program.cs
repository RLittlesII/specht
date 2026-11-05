using Specht.Mcp.Features.Host;
using Spectre.Console.Cli;

var app = new CommandApp<HostCommand>();
app.Configure(static config => config.AddHost());

return await app.RunAsync(args);
