namespace Specht.Cqrs;

/// <summary>Handles one read request.</summary>
public interface IQueryHandler<TQuery, TResult> where TQuery : IQuery<TResult>
{
    /// <summary>Handles the request.</summary>
    Task<TResult> Handle(TQuery query, CancellationToken cancellationToken);
}
