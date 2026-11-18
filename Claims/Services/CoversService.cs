using Claims.Auditing;
using Claims.Data;
using Claims.Validators;

namespace Claims.Services;

public class CoversService : ICoversService
{
    private readonly ClaimsContext _claimsContext;
    private readonly Auditer _auditer;
    private readonly ICoverValidator _coverValidator;

    public CoversService(ClaimsContext claimsContext, AuditContext auditContext, ICoverValidator coverValidator)
    {
        _claimsContext = claimsContext;
        _auditer = new Auditer(auditContext);
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
        _auditer.AuditCover(cover.Id, "POST");
        return (cover, null);
    }

    public async Task DeleteAsync(string id)
    {
        _auditer.AuditCover(id, "DELETE");
        await _claimsContext.DeleteCoverAsync(id);
    }

    public decimal ComputePremium(DateTime startDate, DateTime endDate, CoverType coverType)
    {
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

        var premiumPerDay = 1250 * multiplier;
        var insuranceLength = (endDate - startDate).TotalDays;
        var totalPremium = 0m;

        for (var i = 0; i < insuranceLength; i++)
        {
            if (i < 30) totalPremium += premiumPerDay;
            if (i < 180 && coverType == CoverType.Yacht) totalPremium += premiumPerDay - premiumPerDay * 0.05m;
            else if (i < 180) totalPremium += premiumPerDay - premiumPerDay * 0.02m;
            if (i < 365 && coverType != CoverType.Yacht) totalPremium += premiumPerDay - premiumPerDay * 0.03m;
            else if (i < 365) totalPremium += premiumPerDay - premiumPerDay * 0.08m;
        }

        return totalPremium;
    }
}
