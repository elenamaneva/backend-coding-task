namespace Claims.Services;

public interface ICoversService
{
    Task<IEnumerable<Cover>> GetAllAsync();
    Task<Cover?> GetByIdAsync(string id);
    Task<(Cover? Cover, string? Error)> CreateAsync(Cover cover);
    Task DeleteAsync(string id);
    decimal ComputePremium(DateTime startDate, DateTime endDate, CoverType coverType);
}
