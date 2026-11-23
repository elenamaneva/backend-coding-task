using Claims;
using Claims.Services;
using Claims.Validators;
using Moq;
using Xunit;

namespace Claims.Tests;

/// <summary>
/// Unit tests for ClaimValidator business rules.
/// </summary>
public class ClaimValidatorTests
{
    private readonly Mock<ICoversService> _mockCoversService;
    private readonly ClaimValidator _validator;

    public ClaimValidatorTests()
    {
        _mockCoversService = new Mock<ICoversService>();
        _validator = new ClaimValidator(_mockCoversService.Object);
    }

    // ==================== DamageCost Validation ====================

    [Fact]
    public async Task Validate_DamageCostExceeds100000_ReturnsError()
    {
        // Arrange
        var claim = new Claim
        {
            CoverId = "cover-123",
            DamageCost = 150000m, // Exceeds 100,000
            Created = DateTime.UtcNow
        };

        // Act
        var (isValid, error) = await _validator.ValidateAsync(claim);

        // Assert
        Assert.False(isValid);
        Assert.Contains("100000", error);
    }

    [Fact]
    public async Task Validate_DamageCostExactly100000_IsValid()
    {
        // Arrange
        var cover = new Cover
        {
            Id = "cover-123",
            StartDate = DateTime.UtcNow.AddDays(-10),
            EndDate = DateTime.UtcNow.AddDays(10)
        };
        
        _mockCoversService.Setup(s => s.GetByIdAsync("cover-123"))
            .ReturnsAsync(cover);

        var claim = new Claim
        {
            CoverId = "cover-123",
            DamageCost = 100000m, // Exactly 100,000
            Created = DateTime.UtcNow
        };

        // Act
        var (isValid, error) = await _validator.ValidateAsync(claim);

        // Assert
        Assert.True(isValid);
        Assert.Null(error);
    }

    [Fact]
    public async Task Validate_DamageCostBelow100000_IsValid()
    {
        // Arrange
        var cover = new Cover
        {
            Id = "cover-123",
            StartDate = DateTime.UtcNow.AddDays(-10),
            EndDate = DateTime.UtcNow.AddDays(10)
        };
        
        _mockCoversService.Setup(s => s.GetByIdAsync("cover-123"))
            .ReturnsAsync(cover);

        var claim = new Claim
        {
            CoverId = "cover-123",
            DamageCost = 50000m,
            Created = DateTime.UtcNow
        };

        // Act
        var (isValid, error) = await _validator.ValidateAsync(claim);

        // Assert
        Assert.True(isValid);
        Assert.Null(error);
    }

    // ==================== Cover Exists Validation ====================

    [Fact]
    public async Task Validate_CoverNotFound_ReturnsError()
    {
        // Arrange
        _mockCoversService.Setup(s => s.GetByIdAsync("non-existent"))
            .ReturnsAsync((Cover?)null);

        var claim = new Claim
        {
            CoverId = "non-existent",
            DamageCost = 5000m,
            Created = DateTime.UtcNow
        };

        // Act
        var (isValid, error) = await _validator.ValidateAsync(claim);

        // Assert
        Assert.False(isValid);
        Assert.Contains("not found", error, StringComparison.OrdinalIgnoreCase);
    }

    // ==================== Created Date Within Cover Period ====================

    [Fact]
    public async Task Validate_CreatedDateBeforeCoverStart_ReturnsError()
    {
        // Arrange
        var cover = new Cover
        {
            Id = "cover-123",
            StartDate = new DateTime(2026, 6, 1),
            EndDate = new DateTime(2026, 12, 1)
        };
        
        _mockCoversService.Setup(s => s.GetByIdAsync("cover-123"))
            .ReturnsAsync(cover);

        var claim = new Claim
        {
            CoverId = "cover-123",
            DamageCost = 5000m,
            Created = new DateTime(2026, 5, 1) // Before cover start
        };

        // Act
        var (isValid, error) = await _validator.ValidateAsync(claim);

        // Assert
        Assert.False(isValid);
        Assert.Contains("within", error, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task Validate_CreatedDateAfterCoverEnd_ReturnsError()
    {
        // Arrange
        var cover = new Cover
        {
            Id = "cover-123",
            StartDate = new DateTime(2026, 6, 1),
            EndDate = new DateTime(2026, 12, 1)
        };
        
        _mockCoversService.Setup(s => s.GetByIdAsync("cover-123"))
            .ReturnsAsync(cover);

        var claim = new Claim
        {
            CoverId = "cover-123",
            DamageCost = 5000m,
            Created = new DateTime(2027, 1, 1) // After cover end
        };

        // Act
        var (isValid, error) = await _validator.ValidateAsync(claim);

        // Assert
        Assert.False(isValid);
        Assert.Contains("within", error, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task Validate_CreatedDateWithinCoverPeriod_IsValid()
    {
        // Arrange
        var cover = new Cover
        {
            Id = "cover-123",
            StartDate = new DateTime(2026, 6, 1),
            EndDate = new DateTime(2026, 12, 1)
        };
        
        _mockCoversService.Setup(s => s.GetByIdAsync("cover-123"))
            .ReturnsAsync(cover);

        var claim = new Claim
        {
            CoverId = "cover-123",
            DamageCost = 5000m,
            Created = new DateTime(2026, 8, 15) // Within cover period
        };

        // Act
        var (isValid, error) = await _validator.ValidateAsync(claim);

        // Assert
        Assert.True(isValid);
        Assert.Null(error);
    }

    [Fact]
    public async Task Validate_CreatedDateOnCoverStartDate_IsValid()
    {
        // Arrange
        var cover = new Cover
        {
            Id = "cover-123",
            StartDate = new DateTime(2026, 6, 1),
            EndDate = new DateTime(2026, 12, 1)
        };
        
        _mockCoversService.Setup(s => s.GetByIdAsync("cover-123"))
            .ReturnsAsync(cover);

        var claim = new Claim
        {
            CoverId = "cover-123",
            DamageCost = 5000m,
            Created = new DateTime(2026, 6, 1) // Exactly on start date
        };

        // Act
        var (isValid, error) = await _validator.ValidateAsync(claim);

        // Assert
        Assert.True(isValid);
        Assert.Null(error);
    }

    [Fact]
    public async Task Validate_CreatedDateOnCoverEndDate_IsValid()
    {
        // Arrange
        var cover = new Cover
        {
            Id = "cover-123",
            StartDate = new DateTime(2026, 6, 1),
            EndDate = new DateTime(2026, 12, 1)
        };
        
        _mockCoversService.Setup(s => s.GetByIdAsync("cover-123"))
            .ReturnsAsync(cover);

        var claim = new Claim
        {
            CoverId = "cover-123",
            DamageCost = 5000m,
            Created = new DateTime(2026, 12, 1) // Exactly on end date
        };

        // Act
        var (isValid, error) = await _validator.ValidateAsync(claim);

        // Assert
        Assert.True(isValid);
        Assert.Null(error);
    }
}
