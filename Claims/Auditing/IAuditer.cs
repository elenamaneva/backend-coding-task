namespace Claims.Auditing;

/// <summary>
/// Interface for auditing operations.
/// Allows dependency injection and easier testing.
/// </summary>
public interface IAuditer
{
    /// <summary>
    /// Audits a claim operation asynchronously.
    /// </summary>
    /// <param name="claimId">The ID of the claim.</param>
    /// <param name="httpRequestType">The HTTP method (POST, DELETE, etc.).</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    ValueTask AuditClaimAsync(string claimId, string httpRequestType, CancellationToken cancellationToken = default);

    /// <summary>
    /// Audits a cover operation asynchronously.
    /// </summary>
    /// <param name="coverId">The ID of the cover.</param>
    /// <param name="httpRequestType">The HTTP method (POST, DELETE, etc.).</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    ValueTask AuditCoverAsync(string coverId, string httpRequestType, CancellationToken cancellationToken = default);
}
