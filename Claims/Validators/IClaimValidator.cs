namespace Claims.Validators;

/// <summary>
/// Interface for validating insurance claims.
/// </summary>
public interface IClaimValidator
{
    /// <summary>
    /// Validates a claim against business rules.
    /// </summary>
    /// <param name="claim">The claim to validate.</param>
    /// <returns>
    /// A tuple containing:
    /// - IsValid: true if validation passes, false otherwise
    /// - Error: null if valid, error message if invalid
    /// </returns>
    /// <remarks>
    /// Validation rules:
    /// - DamageCost cannot exceed 100,000
    /// - CoverId must reference an existing cover
    /// - Created date must be within the cover's period
    /// </remarks>
    Task<(bool IsValid, string? Error)> ValidateAsync(Claim claim);
}
