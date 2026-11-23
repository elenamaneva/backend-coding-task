using Claims.Auditing;
using Claims.Data;
using Claims.Validators;

namespace Claims.Services;

public class CoversService : ICoversService
{
    private readonly ClaimsContext _claimsContext;
    private readonly IAuditer _auditer;
    private readonly ICoverValidator _coverValidator;

    public CoversService(ClaimsContext claimsContext, IAuditer auditer, ICoverValidator coverValidator)
    {
        _claimsContext = claimsContext;
        _auditer = auditer;
        _coverValidator = coverValidator;
    }

    public async Task<IEnumerable<Cover>> GetAllAsync()
    {
        return await _claimsContext.GetCoversAsync();
    }

    public async Task<Cover?> GetByIdAsync(string id)
    {
        return await _claimsContext.GetCoverAsync(id);
    }

    public async Task<(Cover? Cover, string? Error)> CreateAsync(Cover cover)
    {
        // Validate before creating
        var (isValid, error) = _coverValidator.Validate(cover);
        if (!isValid)
        {
            return (null, error);
        }

        cover.Id = Guid.NewGuid().ToString();
        cover.Premium = ComputePremium(cover.StartDate, cover.EndDate, cover.Type);
        await _claimsContext.AddCoverAsync(cover);
        
        // Audit asynchronously - doesn't block the response
        await _auditer.AuditCoverAsync(cover.Id, "POST");
        
        return (cover, null);
    }

    public async Task DeleteAsync(string id)
    {
        // Audit asynchronously - doesn't block the response
        await _auditer.AuditCoverAsync(id, "DELETE");
        
        await _claimsContext.DeleteCoverAsync(id);
    }

    /// <summary>
    /// Computes the insurance premium based on cover type and period length.
    /// </summary>
    /// <remarks>
    /// Premium calculation rules:
    /// - Base day rate: 1250
    /// - Type multipliers: Yacht +10%, PassengerShip +20%, Tanker +50%, Others +30%
    /// - Progressive discounts by period:
    ///   - Days 1-30: No discount
    ///   - Days 31-180: Yacht 5% discount, Others 2% discount
    ///   - Days 181-365: Yacht 8% discount (5%+3%), Others 3% discount (2%+1%)
    /// </remarks>
    public decimal ComputePremium(DateTime startDate, DateTime endDate, CoverType coverType)
    {
        const decimal baseDayRate = 1250m;

        // Type multiplier: Yacht +10%, PassengerShip +20%, Tanker +50%, Others +30%
        var multiplier = 1.3m;
        if (coverType == CoverType.Yacht)
        {
            multiplier = 1.1m;
        }

        if (coverType == CoverType.PassengerShip)
        {
            multiplier = 1.2m;
        }

        if (coverType == CoverType.Tanker)
        {
            multiplier = 1.5m;
        }


        var premiumPerDay = baseDayRate * multiplier;
        var totalDays = (int)(endDate - startDate).TotalDays;
        var totalPremium = 0m;

        for (var day = 0; day < totalDays; day++)
        {
            var discount = GetDailyDiscount(day, coverType);
            totalPremium += premiumPerDay * (1 - discount);
        }

        return totalPremium;
    }

    private static decimal GetDailyDiscount(int day, CoverType coverType)
    {
        // First 30 days: no discount
        if (day < 30)
        {
            return 0m;
        }
        // Days 31-180
        if (day < 180)
        {
            if (coverType == CoverType.Yacht)
                return 0.05m;  // 5% discount
            else
                return 0.02m;  // 2% discount
        }
        // Days 181+
        if (coverType == CoverType.Yacht)
            return 0.08m;  // 8% discount
        else
            return 0.03m;  // 3% discount
    }
}
