namespace Specht.Comparison;

/// <summary>The comparison-owned steps after release publication, each of which can end a comparison incomplete.</summary>
public enum ReportingStage
{
    PairPersistence,
    Receipt,
    ReceiptWait,
    Collection,
    Parsing,
    Evaluation,
    Rendering,
    ResultTransport,
    ResultWait,
    Attachment,
}
