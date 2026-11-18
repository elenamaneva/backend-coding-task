using Claims.Services;
using Microsoft.AspNetCore.Mvc;

namespace Claims.Controllers;

[ApiController]
[Route("[controller]")]
public class CoversController : ControllerBase
{
    private readonly ICoversService _coversService;
    private readonly ILogger<CoversController> _logger;

    public CoversController(ICoversService coversService, ILogger<CoversController> logger)
    {
        _coversService = coversService;
        _logger = logger;
    }

    [HttpPost("compute")]
    public ActionResult ComputePremiumAsync(DateTime startDate, DateTime endDate, CoverType coverType)
    {
        return Ok(_coversService.ComputePremium(startDate, endDate, coverType));
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Cover>>> GetAsync()
    {
        var results = await _coversService.GetAllAsync();
        return Ok(results);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Cover?>> GetAsync(string id)
    {
        var result = await _coversService.GetByIdAsync(id);
        return Ok(result);
    }

    [HttpPost]
    public async Task<ActionResult> CreateAsync(Cover cover)
    {
        var (created, error) = await _coversService.CreateAsync(cover);
        
        if (error is not null)
        {
            return BadRequest(new { error });
        }
        
        return Ok(created);
    }

    [HttpDelete("{id}")]
    public async Task DeleteAsync(string id)
    {
        await _coversService.DeleteAsync(id);
    }
}
