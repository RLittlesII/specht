namespace Specht.Cqrs;

/// <summary>Dispatches explicitly registered engine requests without reflection (ADR-0010).</summary>
public interface ICqrs
{
    /// <summary>Sends a read request.</summary>
    Task<TResult> Send<TResult>(IQuery<TResult> query, CancellationToken cancellationToken = default);

    /// <summary>Sends a write request.</summary>
    Task<TResult> Send<TResult>(ICommand<TResult> command, CancellationToken cancellationToken = default);
}
