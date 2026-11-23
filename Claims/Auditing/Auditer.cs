namespace Claims.Auditing;

/// <summary>
/// Auditer implementation that queues audit messages for asynchronous processing.
/// Messages are processed by AuditBackgroundService.
/// </summary>
public class Auditer : IAuditer
{
    private readonly IAuditQueue _auditQueue;

    public Auditer(IAuditQueue auditQueue)
    {
        _auditQueue = auditQueue;
    }

    /// <inheritdoc />
    public async ValueTask AuditClaimAsync(string claimId, string httpRequestType, CancellationToken cancellationToken = default)
    {
        var message = new AuditMessage
        {
            EntityType = AuditEntityType.Claim,
            EntityId = claimId,
            HttpRequestType = httpRequestType,
            Created = DateTime.UtcNow
        };

        await _auditQueue.EnqueueAsync(message, cancellationToken);
    }

    /// <inheritdoc />
    public async ValueTask AuditCoverAsync(string coverId, string httpRequestType, CancellationToken cancellationToken = default)
    {
        var message = new AuditMessage
        {
            EntityType = AuditEntityType.Cover,
            EntityId = coverId,
            HttpRequestType = httpRequestType,
            Created = DateTime.UtcNow
        };

        await _auditQueue.EnqueueAsync(message, cancellationToken);
    }
}
