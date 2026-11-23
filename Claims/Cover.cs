using MongoDB.Bson.Serialization.Attributes;

namespace Claims;

/// <summary>
/// Represents an insurance cover (policy) for a vessel.
/// </summary>
public class Cover
{
    /// <summary>
    /// Unique identifier for the cover.
    /// </summary>
    [BsonId]
    public string Id { get; set; }

    /// <summary>
    /// Start date of the insurance period. Cannot be in the past.
    /// </summary>
    [BsonElement("startDate")]
    public DateTime StartDate { get; set; }

    /// <summary>
    /// End date of the insurance period. Period cannot exceed 1 year.
    /// </summary>
    [BsonElement("endDate")]
    public DateTime EndDate { get; set; }

    /// <summary>
    /// Type of vessel covered. Available values: Yacht, PassengerShip, ContainerShip, BulkCarrier, Tanker
    /// </summary>
    [BsonElement("claimType")]
    public CoverType Type { get; set; }

    /// <summary>
    /// Calculated premium based on vessel type and insurance period.
    /// </summary>
    [BsonElement("premium")]
    public decimal Premium { get; set; }
}

/// <summary>
/// Types of vessels that can be covered.
/// </summary>
public enum CoverType
{
    /// <summary>Recreational yacht (10% premium increase)</summary>
    Yacht = 0,
    
    /// <summary>Passenger ship (20% premium increase)</summary>
    PassengerShip = 1,
    
    /// <summary>Container cargo ship (30% premium increase)</summary>
    ContainerShip = 2,
    
    /// <summary>Bulk carrier vessel (30% premium increase)</summary>
    BulkCarrier = 3,
    
    /// <summary>Oil/fuel tanker (50% premium increase)</summary>
    Tanker = 4
}
