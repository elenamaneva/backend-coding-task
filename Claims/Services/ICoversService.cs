namespace Claims.Services;

/// <summary>
/// Service interface for managing insurance covers (policies).
/// </summary>
public interface ICoversService
{
    /// <summary>
    /// Gets all covers from the database.
    /// </summary>
    /// <returns>Collection of all covers.</returns>
    Task<IEnumerable<Cover>> GetAllAsync();

    /// <summary>
    /// Gets a cover by its ID.
    /// </summary>
    /// <param name="id">The unique identifier of the cover.</param>
    /// <returns>The cover if found, null otherwise.</returns>
    Task<Cover?> GetByIdAsync(string id);

    /// <summary>
    /// Creates a new cover after validation.
    /// </summary>
    /// <param name="cover">The cover to create.</param>
    /// <returns>
    /// A tuple containing the created cover (if successful) and any validation error message.
    /// If validation fails, Cover is null and Error contains the message.
    /// </returns>
    Task<(Cover? Cover, string? Error)> CreateAsync(Cover cover);

    /// <summary>
    /// Deletes a cover by its ID.
    /// </summary>
    /// <param name="id">The unique identifier of the cover to delete.</param>
    Task DeleteAsync(string id);

    /// <summary>
    /// Computes the insurance premium based on cover type and period.
    /// </summary>
    /// <param name="startDate">Start date of the insurance period.</param>
    /// <param name="endDate">End date of the insurance period.</param>
    /// <param name="coverType">Type of vessel to cover.</param>
    /// <returns>The calculated premium amount.</returns>
    /// <remarks>
    /// Premium calculation rules:
    /// - Base rate: 1250 per day
    /// - Multipliers: Yacht +10%, PassengerShip +20%, Tanker +50%, Others +30%
    /// - Discounts: Days 31-180 get 5%/2% off, Days 181+ get 8%/3% off
    /// </remarks>
    decimal ComputePremium(DateTime startDate, DateTime endDate, CoverType coverType);
}
