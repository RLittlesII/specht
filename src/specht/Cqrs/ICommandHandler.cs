namespace Specht.Cqrs;

/// <summary>Handles one write request.</summary>
public interface ICommandHandler<TCommand, TResult> where TCommand : ICommand<TResult>
{
    /// <summary>Handles the request.</summary>
    Task<TResult> Handle(TCommand command, CancellationToken cancellationToken);
}
