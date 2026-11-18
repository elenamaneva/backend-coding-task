namespace Claims.Services;

public interface IClaimsService
{
    Task<IEnumerable<Claim>> GetAllAsync();
    Task<Claim?> GetByIdAsync(string id);
    Task<(Claim? Claim, string? Error)> CreateAsync(Claim claim);
    Task DeleteAsync(string id);
}
