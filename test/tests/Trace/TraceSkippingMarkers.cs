namespace Specht.Tests.Trace;

/// <summary>
/// The traceability Status cells whose tests the trace gives as <c>skipped</c> under a schema <c>0.2.0</c> manifest's default
/// markers (<c>0001-F13</c> B-019): the waived, withdrawn and superseded markers.
/// </summary>
public sealed class TraceSkippingMarkers : TheoryData<string>
{
    public TraceSkippingMarkers()
    {
        Add("Waived");
        Add("Withdrawn");
        Add("Superseded");
    }
}
