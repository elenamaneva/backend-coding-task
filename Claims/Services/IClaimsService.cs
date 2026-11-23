namespace Claims.Services;

/// <summary>
/// Service interface for managing insurance claims.
/// </summary>
public interface IClaimsService
{
    /// <summary>
    /// Gets all claims from the database.
    /// </summary>
    /// <returns>Collection of all claims.</returns>
    Task<IEnumerable<Claim>> GetAllAsync();

    /// <summary>
    /// Gets a claim by its ID.
    /// </summary>
    /// <param name="id">The unique identifier of the claim.</param>
    /// <returns>The claim if found, null otherwise.</returns>
    Task<Claim?> GetByIdAsync(string id);

    /// <summary>
    /// Creates a new claim after validation.
    /// </summary>
    /// <param name="claim">The claim to create.</param>
    /// <returns>
    /// A tuple containing the created claim (if successful) and any validation error message.
    /// If validation fails, Claim is null and Error contains the message.
    /// </returns>
    Task<(Claim? Claim, string? Error)> CreateAsync(Claim claim);

    /// <summary>
    /// Deletes a claim by its ID.
    /// </summary>
    /// <param name="id">The unique identifier of the claim to delete.</param>
    Task DeleteAsync(string id);
}
