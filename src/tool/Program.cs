using System.IO.Abstractions;
using Specht.Tool;

return await ToolApplication.CreateApp(new FileSystem()).RunAsync(args);
