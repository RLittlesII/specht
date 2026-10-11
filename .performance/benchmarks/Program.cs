using System.Linq;
using BenchmarkDotNet.Running;
using Specht.Benchmarks;

var summaries = BenchmarkSwitcher.FromAssembly(typeof(BenchmarkConfiguration).Assembly).Run(args, new BenchmarkConfiguration());

return summaries.Any(static summary => summary.HasCriticalValidationErrors || summary.Reports.Any(static report => !report.Success))
    ? 1
    : 0;
