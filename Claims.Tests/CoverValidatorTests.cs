using Claims;
using Claims.Validators;
using Xunit;

namespace Claims.Tests;

/// <summary>
/// Unit tests for CoverValidator business rules.
/// </summary>
public class CoverValidatorTests
{
    private readonly CoverValidator _validator;

    public CoverValidatorTests()
    {
        _validator = new CoverValidator();
    }

    // ==================== StartDate Validation ====================

    [Fact]
    public void Validate_StartDateInPast_ReturnsError()
    {
        // Arrange
        var cover = new Cover
        {
            StartDate = DateTime.UtcNow.AddDays(-1), // Yesterday
            EndDate = DateTime.UtcNow.AddDays(30),
            Type = CoverType.Yacht
        };

        // Act
        var (isValid, error) = _validator.Validate(cover);

        // Assert
        Assert.False(isValid);
        Assert.Contains("past", error, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Validate_StartDateToday_IsValid()
    {
        // Arrange
        var cover = new Cover
        {
            StartDate = DateTime.UtcNow.Date, // Today
            EndDate = DateTime.UtcNow.Date.AddDays(30),
            Type = CoverType.Yacht
        };

        // Act
        var (isValid, error) = _validator.Validate(cover);

        // Assert
        Assert.True(isValid);
        Assert.Null(error);
    }

    [Fact]
    public void Validate_StartDateInFuture_IsValid()
    {
        // Arrange
        var cover = new Cover
        {
            StartDate = DateTime.UtcNow.Date.AddDays(7), // Next week
            EndDate = DateTime.UtcNow.Date.AddDays(37),
            Type = CoverType.Yacht
        };

        // Act
        var (isValid, error) = _validator.Validate(cover);

        // Assert
        Assert.True(isValid);
        Assert.Null(error);
    }

    // ==================== Period Length Validation ====================

    [Fact]
    public void Validate_PeriodExceeds1Year_ReturnsError()
    {
        // Arrange
        var cover = new Cover
        {
            StartDate = DateTime.UtcNow.Date.AddDays(1),
            EndDate = DateTime.UtcNow.Date.AddDays(400), // More than 365 days
            Type = CoverType.Yacht
        };

        // Act
        var (isValid, error) = _validator.Validate(cover);

        // Assert
        Assert.False(isValid);
        Assert.Contains("365", error);
    }

    [Fact]
    public void Validate_PeriodExactly1Year_IsValid()
    {
        // Arrange
        var cover = new Cover
        {
            StartDate = DateTime.UtcNow.Date.AddDays(1),
            EndDate = DateTime.UtcNow.Date.AddDays(366), // Exactly 365 days
            Type = CoverType.Yacht
        };

        // Act
        var (isValid, error) = _validator.Validate(cover);

        // Assert
        Assert.True(isValid);
        Assert.Null(error);
    }

    [Fact]
    public void Validate_PeriodLessThan1Year_IsValid()
    {
        // Arrange
        var cover = new Cover
        {
            StartDate = DateTime.UtcNow.Date.AddDays(1),
            EndDate = DateTime.UtcNow.Date.AddDays(100), // 99 days
            Type = CoverType.Yacht
        };

        // Act
        var (isValid, error) = _validator.Validate(cover);

        // Assert
        Assert.True(isValid);
        Assert.Null(error);
    }

    // ==================== EndDate Before StartDate ====================

    [Fact]
    public void Validate_EndDateBeforeStartDate_ReturnsError()
    {
        // Arrange
        var cover = new Cover
        {
            StartDate = DateTime.UtcNow.Date.AddDays(10),
            EndDate = DateTime.UtcNow.Date.AddDays(5), // Before start
            Type = CoverType.Yacht
        };

        // Act
        var (isValid, error) = _validator.Validate(cover);

        // Assert
        Assert.False(isValid);
        Assert.Contains("EndDate", error);
    }

    [Fact]
    public void Validate_EndDateEqualsStartDate_ReturnsError()
    {
        // Arrange
        var cover = new Cover
        {
            StartDate = DateTime.UtcNow.Date.AddDays(10),
            EndDate = DateTime.UtcNow.Date.AddDays(10), // Same as start
            Type = CoverType.Yacht
        };

        // Act
        var (isValid, error) = _validator.Validate(cover);

        // Assert
        Assert.False(isValid);
        Assert.Contains("EndDate", error);
    }
}
