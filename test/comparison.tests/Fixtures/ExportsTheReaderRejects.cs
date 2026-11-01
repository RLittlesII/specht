namespace Specht.Comparison.Tests.Workflow;

public sealed class ExportsTheReaderRejects : TheoryData<string[], string>
{
    public ExportsTheReaderRejects()
    {
        Add(["previous"], "The collected benchmark exports could not be read. Previous revision: Exporter version or Release/debugger evidence is unsupported. "
            + "Re-run all jobs to retry with the saved pair.");
        Add(["current"], "The collected benchmark exports could not be read. Current revision: Exporter version or Release/debugger evidence is unsupported. "
            + "Re-run all jobs to retry with the saved pair.");
        Add(["previous", "current"], "The collected benchmark exports could not be read. "
            + "Previous revision: Exporter version or Release/debugger evidence is unsupported. "
            + "Current revision: Exporter version or Release/debugger evidence is unsupported. Re-run all jobs to retry with the saved pair.");
    }
}
