using System.Threading.Channels;

namespace Claims.Auditing;

/// <summary>
/// In-memory implementation of IAuditQueue using System.Threading.Channels.
/// Provides a fast, non-blocking queue for audit messages.
/// </summary>
/// <remarks>
/// Trade-off: Messages are lost if the application crashes before processing.
/// For production, consider Azure Service Bus or similar durable queue.
/// </remarks>
public class InMemoryAuditQueue : IAuditQueue
{
    private readonly Channel<AuditMessage> _channel;

    public InMemoryAuditQueue()
    {
        // Unbounded channel - won't block writers
        // For production, consider bounded channel with backpressure
        _channel = Channel.CreateUnbounded<AuditMessage>(new UnboundedChannelOptions
        {
            SingleReader = true,  // Only AuditBackgroundService reads
            SingleWriter = false  // Multiple services can write
        });
    }

    /// <inheritdoc />
    public async ValueTask EnqueueAsync(AuditMessage message, CancellationToken cancellationToken = default)
    {
        await _channel.Writer.WriteAsync(message, cancellationToken);
    }

    /// <inheritdoc />
    public IAsyncEnumerable<AuditMessage> DequeueAllAsync(CancellationToken cancellationToken)
    {
        return _channel.Reader.ReadAllAsync(cancellationToken);
    }
}
