using System;
using System.Runtime.CompilerServices;

namespace Specht.Benchmarks;

/// <summary>
/// Declares the workload a benchmark method measures, so that a release comparison reads this revision's own identity from its built
/// assembly's metadata (<c>0109-F5</c> B-025, decision 0005). Nothing reads it at run time and it changes nothing that is measured.
/// </summary>
/// <param name="operationId">The stable id of the measured operation, unique among this harness's benchmarks.</param>
/// <param name="operationRevision">The revision of that operation's contract, raised when what the method measures changes.</param>
/// <param name="fixtureId">The stable id of the input the operation runs over.</param>
/// <param name="fixtureRevision">The revision of that input's definition, raised when the input changes.</param>
/// <param name="boundary">Where measurement starts and stops: <c>in-process</c> or <c>tool-process</c>.</param>
/// <param name="sourceFile">The declaring file, supplied by the compiler; the comparator keeps it only relative to the checkout.</param>
[AttributeUsage(AttributeTargets.Method)]
public sealed class WorkloadIdentityAttribute(
    string operationId,
    string operationRevision,
    string fixtureId,
    string fixtureRevision,
    string boundary,
    [CallerFilePath] string sourceFile = "") : Attribute
{
    /// <summary>Gets the stable id of the measured operation.</summary>
    public string OperationId { get; } = operationId;

    /// <summary>Gets the revision of the operation's contract.</summary>
    public string OperationRevision { get; } = operationRevision;

    /// <summary>Gets the stable id of the input the operation runs over.</summary>
    public string FixtureId { get; } = fixtureId;

    /// <summary>Gets the revision of the input's definition.</summary>
    public string FixtureRevision { get; } = fixtureRevision;

    /// <summary>Gets where measurement starts and stops.</summary>
    public string Boundary { get; } = boundary;

    /// <summary>Gets the declaring file as the compiler saw it.</summary>
    public string SourceFile { get; } = sourceFile;
}
