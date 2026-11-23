using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Claims.Auditing;

/// <summary>
/// Background service that processes audit messages from the queue
/// and persists them to the database asynchronously.
/// </summary>
public class AuditBackgroundService : BackgroundService
{
    private readonly IAuditQueue _auditQueue;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<AuditBackgroundService> _logger;

    public AuditBackgroundService(
        IAuditQueue auditQueue,
        IServiceScopeFactory scopeFactory,
        ILogger<AuditBackgroundService> logger)
    {
        _auditQueue = auditQueue;
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Audit background service started.");

        await foreach (var message in _auditQueue.DequeueAllAsync(stoppingToken))
        {
            try
            {
                await ProcessAuditMessageAsync(message, stoppingToken);
            }
            catch (Exception ex)
            {
                // Log error but continue processing other messages
                _logger.LogError(ex, "Error processing audit message for {EntityType} {EntityId}",
                    message.EntityType, message.EntityId);
            }
        }

        _logger.LogInformation("Audit background service stopped.");
    }

    private async Task ProcessAuditMessageAsync(AuditMessage message, CancellationToken cancellationToken)
    {
        // Create a new scope for each message to get a fresh DbContext
        using var scope = _scopeFactory.CreateScope();
        var auditContext = scope.ServiceProvider.GetRequiredService<AuditContext>();

        switch (message.EntityType)
        {
            case AuditEntityType.Claim:
                var claimAudit = new ClaimAudit
                {
                    ClaimId = message.EntityId,
                    HttpRequestType = message.HttpRequestType,
                    Created = message.Created
                };
                auditContext.ClaimAudits.Add(claimAudit);
                break;

            case AuditEntityType.Cover:
                var coverAudit = new CoverAudit
                {
                    CoverId = message.EntityId,
                    HttpRequestType = message.HttpRequestType,
                    Created = message.Created
                };
                auditContext.CoverAudits.Add(coverAudit);
                break;
        }

        await auditContext.SaveChangesAsync(cancellationToken);

        _logger.LogDebug("Processed audit: {EntityType} {EntityId} {HttpRequestType}",
            message.EntityType, message.EntityId, message.HttpRequestType);
    }
}
