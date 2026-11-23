namespace Claims.Validators;

/// <summary>
/// Validates insurance covers against business rules.
/// </summary>
public class CoverValidator : ICoverValidator
{
    /// <summary>
    /// Maximum allowed insurance period in days (1 year).
    /// </summary>
    private const int MaxInsurancePeriodDays = 365;

    /// <inheritdoc />
    public (bool IsValid, string? Error) Validate(Cover cover)
    {
        // Rule 1: StartDate cannot be in the past
        if (cover.StartDate.Date < DateTime.UtcNow.Date)
        {
            return (false, "StartDate cannot be in the past.");
        }

        // Rule 2: Total insurance period cannot exceed 1 year
        var insurancePeriod = (cover.EndDate - cover.StartDate).TotalDays;
        if (insurancePeriod > MaxInsurancePeriodDays)
        {
            return (false, $"Total insurance period cannot exceed {MaxInsurancePeriodDays} days (1 year).");
        }

        // Rule 3: EndDate must be after StartDate
        if (cover.EndDate <= cover.StartDate)
        {
            return (false, "EndDate must be after StartDate.");
        }

        return (true, null);
    }
}
