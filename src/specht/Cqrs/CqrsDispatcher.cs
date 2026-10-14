namespace Specht.Cqrs;

/// <summary>Dispatches explicitly registered handlers with no assembly scan or reflective invocation.</summary>
public sealed class CqrsDispatcher : ICqrs
{
    /// <summary>Registers one query handler.</summary>
    public CqrsDispatcher Register<TQuery, TResult>(IQueryHandler<TQuery, TResult> handler) where TQuery : IQuery<TResult>
    {
        _handlers.Add(typeof(TQuery), async (request, token) => (await handler.Handle((TQuery)request, token))!);
        return this;
    }

    /// <summary>Registers one command handler.</summary>
    public CqrsDispatcher Register<TCommand, TResult>(ICommandHandler<TCommand, TResult> handler) where TCommand : ICommand<TResult>
    {
        _handlers.Add(typeof(TCommand), async (request, token) => (await handler.Handle((TCommand)request, token))!);
        return this;
    }

    /// <inheritdoc />
    public Task<TResult> Send<TResult>(IQuery<TResult> query, CancellationToken cancellationToken = default) =>
        Dispatch<TResult>(query, cancellationToken);

    /// <inheritdoc />
    public Task<TResult> Send<TResult>(ICommand<TResult> command, CancellationToken cancellationToken = default) =>
        Dispatch<TResult>(command, cancellationToken);

    private async Task<TResult> Dispatch<TResult>(object request, CancellationToken cancellationToken) =>
        (TResult)await _handlers[request.GetType()].Invoke(request, cancellationToken);

    private readonly Dictionary<Type, Func<object, CancellationToken, Task<object>>> _handlers = [];
}
