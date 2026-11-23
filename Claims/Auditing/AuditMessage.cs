namespace Claims.Auditing;

/// <summary>
/// Message DTO for audit events to be processed asynchronously.
/// </summary>
public class AuditMessage
{
    /// <summary>
    /// Type of entity being audited (Claim or Cover).
    /// </summary>
    public AuditEntityType EntityType { get; set; }

    /// <summary>
    /// The ID of the entity being audited.
    /// </summary>
    public string EntityId { get; set; } = string.Empty;

    /// <summary>
    /// The HTTP method that triggered the audit (POST, DELETE, etc.).
    /// </summary>
    public string HttpRequestType { get; set; } = string.Empty;

    /// <summary>
    /// Timestamp when the audit event was created.
    /// </summary>
    public DateTime Created { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// Types of entities that can be audited.
/// </summary>
public enum AuditEntityType
{
    Claim,
    Cover
}
