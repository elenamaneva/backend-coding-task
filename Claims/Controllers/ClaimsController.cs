using Claims.Services;
using Microsoft.AspNetCore.Mvc;

namespace Claims.Controllers;

/// <summary>
/// API controller for managing insurance claims.
/// </summary>
[ApiController]
[Route("[controller]")]
public class ClaimsController : ControllerBase
{
    private readonly ILogger<ClaimsController> _logger;
    private readonly IClaimsService _claimsService;

    public ClaimsController(ILogger<ClaimsController> logger, IClaimsService claimsService)
    {
        _logger = logger;
        _claimsService = claimsService;
    }

    /// <summary>
    /// Gets all claims.
    /// </summary>
    /// <returns>List of all claims.</returns>
    [HttpGet]
    public async Task<IEnumerable<Claim>> GetAsync()
    {
        return await _claimsService.GetAllAsync();
    }

    /// <summary>
    /// Creates a new claim.
    /// </summary>
    /// <param name="claim">The claim to create.</param>
    /// <returns>The created claim or validation error.</returns>
    /// <response code="200">Claim created successfully.</response>
    /// <response code="400">Validation failed (DamageCost exceeds 100,000 or Created date outside Cover period).</response>
    [HttpPost]
    [ProducesResponseType(typeof(Claim), 200)]
    [ProducesResponseType(400)]
    public async Task<ActionResult> CreateAsync(Claim claim)
    {
        var (created, error) = await _claimsService.CreateAsync(claim);
        
        if (error is not null)
        {
            return BadRequest(new { error });
        }
        
        return Ok(created);
    }

    /// <summary>
    /// Deletes a claim by ID.
    /// </summary>
    /// <param name="id">The ID of the claim to delete.</param>
    [HttpDelete("{id}")]
    public async Task DeleteAsync(string id)
    {
        await _claimsService.DeleteAsync(id);
    }

    /// <summary>
    /// Gets a claim by ID.
    /// </summary>
    /// <param name="id">The ID of the claim.</param>
    /// <returns>The claim if found, null otherwise.</returns>
    [HttpGet("{id}")]
    public async Task<Claim?> GetAsync(string id)
    {
        return await _claimsService.GetByIdAsync(id);
    }
}
