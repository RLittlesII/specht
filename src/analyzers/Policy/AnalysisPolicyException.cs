using System;

namespace Specht.Analysis.Policy;

/// <summary>A supplied policy field the analysis rejects; the message names the field and its fault.</summary>
public sealed class AnalysisPolicyException : ArgumentException
{
    public AnalysisPolicyException(string message)
        : base(message)
    {
    }
}
