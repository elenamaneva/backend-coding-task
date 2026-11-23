using Claims.Services;
using Microsoft.AspNetCore.Mvc;

namespace Claims.Controllers;

/// <summary>
/// API controller for managing insurance covers (policies).
/// </summary>
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

    /// <summary>
    /// Computes the premium for a cover without creating it.
    /// </summary>
    /// <param name="startDate">Start date of the insurance period.</param>
    /// <param name="endDate">End date of the insurance period.</param>
    /// <param name="coverType">Type of vessel to cover.</param>
    /// <returns>The calculated premium amount.</returns>
    [HttpPost("compute")]
    public ActionResult ComputePremiumAsync(DateTime startDate, DateTime endDate, CoverType coverType)
    {
        return Ok(_coversService.ComputePremium(startDate, endDate, coverType));
    }

    /// <summary>
    /// Gets all covers.
    /// </summary>
    /// <returns>List of all covers.</returns>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Cover>>> GetAsync()
    {
        var results = await _coversService.GetAllAsync();
        return Ok(results);
    }

    /// <summary>
    /// Gets a cover by ID.
    /// </summary>
    /// <param name="id">The ID of the cover.</param>
    /// <returns>The cover if found, null otherwise.</returns>
    [HttpGet("{id}")]
    public async Task<ActionResult<Cover?>> GetAsync(string id)
    {
        var result = await _coversService.GetByIdAsync(id);
        return Ok(result);
    }

    /// <summary>
    /// Creates a new cover.
    /// </summary>
    /// <param name="cover">The cover to create.</param>
    /// <returns>The created cover with calculated premium, or validation error.</returns>
    /// <response code="200">Cover created successfully.</response>
    /// <response code="400">Validation failed (StartDate in past or period exceeds 1 year).</response>
    [HttpPost]
    [ProducesResponseType(typeof(Cover), 200)]
    [ProducesResponseType(400)]
    public async Task<ActionResult> CreateAsync(Cover cover)
    {
        var (created, error) = await _coversService.CreateAsync(cover);
        
        if (error is not null)
        {
            return BadRequest(new { error });
        }
        
        return Ok(created);
    }

    /// <summary>
    /// Deletes a cover by ID.
    /// </summary>
    /// <param name="id">The ID of the cover to delete.</param>
    [HttpDelete("{id}")]
    public async Task DeleteAsync(string id)
    {
        await _coversService.DeleteAsync(id);
    }
}
