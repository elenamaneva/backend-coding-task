using Claims.Auditing;
using Claims.Data;
using Claims.Validators;

namespace Claims.Services;

public class ClaimsService : IClaimsService
{
    private readonly ClaimsContext _claimsContext;
    private readonly IAuditer _auditer;
    private readonly IClaimValidator _claimValidator;

    public ClaimsService(ClaimsContext claimsContext, IAuditer auditer, IClaimValidator claimValidator)
    {
        _claimsContext = claimsContext;
        _auditer = auditer;
        _claimValidator = claimValidator;
    }

    public async Task<IEnumerable<Claim>> GetAllAsync()
    {
        return await _claimsContext.GetClaimsAsync();
    }

    public async Task<Claim?> GetByIdAsync(string id)
    {
        return await _claimsContext.GetClaimAsync(id);
    }

    public async Task<(Claim? Claim, string? Error)> CreateAsync(Claim claim)
    {
        // Validate before creating
        var (isValid, error) = await _claimValidator.ValidateAsync(claim);
        if (!isValid)
        {
            return (null, error);
        }

        claim.Id = Guid.NewGuid().ToString();
        await _claimsContext.AddClaimAsync(claim);
        
        // Audit asynchronously - doesn't block the response
        await _auditer.AuditClaimAsync(claim.Id, "POST");
        
        return (claim, null);
    }

    public async Task DeleteAsync(string id)
    {
        // Audit asynchronously - doesn't block the response
        await _auditer.AuditClaimAsync(id, "DELETE");
        
        await _claimsContext.DeleteClaimAsync(id);
    }
}
