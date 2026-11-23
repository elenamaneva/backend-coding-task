namespace Claims.Validators;

/// <summary>
/// Interface for validating insurance covers.
/// </summary>
public interface ICoverValidator
{
    /// <summary>
    /// Validates a cover against business rules.
    /// </summary>
    /// <param name="cover">The cover to validate.</param>
    /// <returns>
    /// A tuple containing:
    /// - IsValid: true if validation passes, false otherwise
    /// - Error: null if valid, error message if invalid
    /// </returns>
    /// <remarks>
    /// Validation rules:
    /// - StartDate cannot be in the past
    /// - Total insurance period cannot exceed 365 days (1 year)
    /// - EndDate must be after StartDate
    /// </remarks>
    (bool IsValid, string? Error) Validate(Cover cover);
}
