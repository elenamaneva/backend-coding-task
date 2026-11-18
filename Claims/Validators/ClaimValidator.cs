using Claims.Services;

namespace Claims.Validators;

public class ClaimValidator : IClaimValidator
{
    private const decimal MaxDamageCost = 100000m;
    private readonly ICoversService _coversService;

    public ClaimValidator(ICoversService coversService)
    {
        _coversService = coversService;
    }

    public async Task<(bool IsValid, string? Error)> ValidateAsync(Claim claim)
    {
        // Rule 1: DamageCost cannot exceed 100,000
        if (claim.DamageCost > MaxDamageCost)
        {
            return (false, $"DamageCost cannot exceed {MaxDamageCost}.");
        }

        // Rule 2: Created date must be within the period of the related Cover
        var cover = await _coversService.GetByIdAsync(claim.CoverId);
        
        if (cover is null)
        {
            return (false, $"Cover with id '{claim.CoverId}' not found.");
        }

        if (claim.Created.Date < cover.StartDate.Date || claim.Created.Date > cover.EndDate.Date)
        {
            return (false, $"Claim Created date must be within the Cover period ({cover.StartDate:yyyy-MM-dd} to {cover.EndDate:yyyy-MM-dd}).");
        }

        return (true, null);
    }
}
