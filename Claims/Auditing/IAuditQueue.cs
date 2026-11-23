namespace Claims.Auditing;

/// <summary>
/// Interface for audit message queue.
/// Allows swapping implementations (in-memory, Service Bus, etc.) without changing consumers.
/// </summary>
public interface IAuditQueue
{
    /// <summary>
    /// Enqueues an audit message for asynchronous processing.
    /// This method returns immediately without blocking.
    /// </summary>
    /// <param name="message">The audit message to enqueue.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    ValueTask EnqueueAsync(AuditMessage message, CancellationToken cancellationToken = default);

    /// <summary>
    /// Dequeues audit messages as an async stream.
    /// Used by the background processor.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Async enumerable of audit messages.</returns>
    IAsyncEnumerable<AuditMessage> DequeueAllAsync(CancellationToken cancellationToken);
}
