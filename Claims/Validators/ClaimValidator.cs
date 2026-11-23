using Claims.Services;

namespace Claims.Validators;

/// <summary>
/// Validates insurance claims against business rules.
/// </summary>
public class ClaimValidator : IClaimValidator
{
    /// <summary>
    /// Maximum allowed damage cost for a claim.
    /// </summary>
    private const decimal MaxDamageCost = 100000m;
    
    private readonly ICoversService _coversService;

    /// <summary>
    /// Creates a new instance of ClaimValidator.
    /// </summary>
    /// <param name="coversService">Service for retrieving cover information.</param>
    public ClaimValidator(ICoversService coversService)
    {
        _coversService = coversService;
    }

    /// <inheritdoc />
    public async Task<(bool IsValid, string? Error)> ValidateAsync(Claim claim)
    {
        // Rule 1: DamageCost cannot exceed 100,000
        if (claim.DamageCost > MaxDamageCost)
        {
            return (false, $"DamageCost cannot exceed {MaxDamageCost}.");
        }

        // Rule 2: Cover must exist
        var cover = await _coversService.GetByIdAsync(claim.CoverId);
        
        if (cover is null)
        {
            return (false, $"Cover with id '{claim.CoverId}' not found.");
        }

        // Rule 3: Created date must be within the period of the related Cover
        if (claim.Created.Date < cover.StartDate.Date || claim.Created.Date > cover.EndDate.Date)
        {
            return (false, $"Claim Created date must be within the Cover period ({cover.StartDate:yyyy-MM-dd} to {cover.EndDate:yyyy-MM-dd}).");
        }

        return (true, null);
    }
}
