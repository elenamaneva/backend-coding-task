using Claims;
using Claims.Services;
using Claims.Validators;
using Claims.Auditing;
using Claims.Data;
using Moq;
using Xunit;

namespace Claims.Tests;

/// <summary>
/// Unit tests for the ComputePremium calculation logic.
/// </summary>
public class ComputePremiumTests
{
    private readonly CoversService _coversService;

    public ComputePremiumTests()
    {
        // Create mocks for dependencies (we only test ComputePremium, not DB operations)
        var mockAuditer = new Mock<IAuditer>();
        var mockValidator = new Mock<ICoverValidator>();
        
        // Pass null for ClaimsContext since ComputePremium doesn't use it
        _coversService = new CoversService(null!, mockAuditer.Object, mockValidator.Object);
    }

    // ==================== Type Multiplier Tests ====================

    [Fact]
    public void ComputePremium_Yacht_Has10PercentIncrease()
    {
        // Arrange: 1 day, Yacht (1.1x multiplier)
        var startDate = new DateTime(2026, 1, 1);
        var endDate = new DateTime(2026, 1, 2); // 1 day

        // Act
        var premium = _coversService.ComputePremium(startDate, endDate, CoverType.Yacht);

        // Assert: Base rate 1250 * 1.1 = 1375
        Assert.Equal(1375m, premium);
    }

    [Fact]
    public void ComputePremium_PassengerShip_Has20PercentIncrease()
    {
        // Arrange: 1 day, PassengerShip (1.2x multiplier)
        var startDate = new DateTime(2026, 1, 1);
        var endDate = new DateTime(2026, 1, 2); // 1 day

        // Act
        var premium = _coversService.ComputePremium(startDate, endDate, CoverType.PassengerShip);

        // Assert: Base rate 1250 * 1.2 = 1500
        Assert.Equal(1500m, premium);
    }

    [Fact]
    public void ComputePremium_Tanker_Has50PercentIncrease()
    {
        // Arrange: 1 day, Tanker (1.5x multiplier)
        var startDate = new DateTime(2026, 1, 1);
        var endDate = new DateTime(2026, 1, 2); // 1 day

        // Act
        var premium = _coversService.ComputePremium(startDate, endDate, CoverType.Tanker);

        // Assert: Base rate 1250 * 1.5 = 1875
        Assert.Equal(1875m, premium);
    }

    [Fact]
    public void ComputePremium_ContainerShip_Has30PercentIncrease()
    {
        // Arrange: 1 day, ContainerShip (1.3x multiplier - default)
        var startDate = new DateTime(2026, 1, 1);
        var endDate = new DateTime(2026, 1, 2); // 1 day

        // Act
        var premium = _coversService.ComputePremium(startDate, endDate, CoverType.ContainerShip);

        // Assert: Base rate 1250 * 1.3 = 1625
        Assert.Equal(1625m, premium);
    }

    // ==================== Period Discount Tests ====================

    [Fact]
    public void ComputePremium_First30Days_NoDiscount()
    {
        // Arrange: 30 days, Yacht
        var startDate = new DateTime(2026, 1, 1);
        var endDate = new DateTime(2026, 1, 31); // 30 days

        // Act
        var premium = _coversService.ComputePremium(startDate, endDate, CoverType.Yacht);

        // Assert: 30 days * 1375 (no discount) = 41250
        Assert.Equal(41250m, premium);
    }

    [Fact]
    public void ComputePremium_Days31To180_Yacht_5PercentDiscount()
    {
        // Arrange: 31 days (1 day at full rate, 1 day at 5% discount)
        var startDate = new DateTime(2026, 1, 1);
        var endDate = new DateTime(2026, 2, 1); // 31 days

        // Act
        var premium = _coversService.ComputePremium(startDate, endDate, CoverType.Yacht);

        // Assert: 
        // Days 0-29 (30 days): 30 * 1375 = 41250
        // Day 30 (1 day): 1 * 1375 * 0.95 = 1306.25
        // Total: 41250 + 1306.25 = 42556.25
        Assert.Equal(42556.25m, premium);
    }

    [Fact]
    public void ComputePremium_Days31To180_OtherTypes_2PercentDiscount()
    {
        // Arrange: 31 days, Tanker
        var startDate = new DateTime(2026, 1, 1);
        var endDate = new DateTime(2026, 2, 1); // 31 days

        // Act
        var premium = _coversService.ComputePremium(startDate, endDate, CoverType.Tanker);

        // Assert: 
        // Days 0-29 (30 days): 30 * 1875 = 56250
        // Day 30 (1 day): 1 * 1875 * 0.98 = 1837.50
        // Total: 56250 + 1837.50 = 58087.50
        Assert.Equal(58087.50m, premium);
    }

    [Fact]
    public void ComputePremium_Days181Plus_Yacht_8PercentDiscount()
    {
        // Arrange: 181 days, Yacht
        var startDate = new DateTime(2026, 1, 1);
        var endDate = new DateTime(2026, 7, 1); // 181 days

        // Act
        var premium = _coversService.ComputePremium(startDate, endDate, CoverType.Yacht);

        // Assert:
        // Days 0-29 (30 days): 30 * 1375 = 41250
        // Days 30-179 (150 days): 150 * 1375 * 0.95 = 195937.50
        // Day 180 (1 day): 1 * 1375 * 0.92 = 1265
        // Total: 41250 + 195937.50 + 1265 = 238452.50
        Assert.Equal(238452.50m, premium);
    }

    [Fact]
    public void ComputePremium_Days181Plus_OtherTypes_3PercentDiscount()
    {
        // Arrange: 181 days, PassengerShip
        var startDate = new DateTime(2026, 1, 1);
        var endDate = new DateTime(2026, 7, 1); // 181 days

        // Act
        var premium = _coversService.ComputePremium(startDate, endDate, CoverType.PassengerShip);

        // Assert:
        // Days 0-29 (30 days): 30 * 1500 = 45000
        // Days 30-179 (150 days): 150 * 1500 * 0.98 = 220500
        // Day 180 (1 day): 1 * 1500 * 0.97 = 1455
        // Total: 45000 + 220500 + 1455 = 266955
        Assert.Equal(266955m, premium);
    }

    // ==================== Edge Cases ====================

    [Fact]
    public void ComputePremium_ZeroDays_ReturnsZero()
    {
        // Arrange: Same start and end date
        var startDate = new DateTime(2026, 1, 1);
        var endDate = new DateTime(2026, 1, 1); // 0 days

        // Act
        var premium = _coversService.ComputePremium(startDate, endDate, CoverType.Yacht);

        // Assert
        Assert.Equal(0m, premium);
    }

    [Fact]
    public void ComputePremium_FullYear_CalculatesCorrectly()
    {
        // Arrange: 365 days, Yacht
        var startDate = new DateTime(2026, 1, 1);
        var endDate = new DateTime(2027, 1, 1); // 365 days

        // Act
        var premium = _coversService.ComputePremium(startDate, endDate, CoverType.Yacht);

        // Assert: Premium should be positive and calculated with all discount tiers
        Assert.True(premium > 0);
        
        // Verify structure:
        // Days 0-29: 30 * 1375 = 41250
        // Days 30-179: 150 * 1375 * 0.95 = 195937.50
        // Days 180-364: 185 * 1375 * 0.92 = 234025
        // Total: 41250 + 195937.50 + 234025 = 471212.50
        Assert.Equal(471212.50m, premium);
    }
}
