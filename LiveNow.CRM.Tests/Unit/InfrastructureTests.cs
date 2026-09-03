using FluentAssertions;
using LiveNow.CRM.Core.Common;
using LiveNow.CRM.Core.Enums;
using Xunit;

namespace LiveNow.CRM.Tests.Unit;

public class InfrastructureTests
{
    [Fact]
    public void BaseEntity_Should_Have_New_Guid_By_Default()
    {
        // Arrange & Act
        var entity = new TestEntity();

        // Assert
        entity.Id.Should().NotBe(Guid.Empty);
        entity.IsActive.Should().BeTrue();
        entity.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void BaseEntity_Should_Support_Modification_Tracking()
    {
        // Arrange
        var entity = new TestEntity();

        // Act
        entity.UpdatedAt = DateTime.UtcNow;
        entity.UpdatedBy = "test-user";

        // Assert
        entity.UpdatedAt.Should().NotBeNull();
        entity.UpdatedBy.Should().Be("test-user");
    }

    [Fact]
    public void RaceTypeEnum_Should_Have_Expected_Values()
    {
        // Assert
        ((int)RaceTypeEnum.Marathon).Should().Be(1);
        ((int)RaceTypeEnum.HalfMarathon).Should().Be(2);
        ((int)RaceTypeEnum.UltraMarathon).Should().Be(3);
        ((int)RaceTypeEnum.TenK).Should().Be(4);
        ((int)RaceTypeEnum.FiveK).Should().Be(5);
        ((int)RaceTypeEnum.Trail).Should().Be(6);
        ((int)RaceTypeEnum.Other).Should().Be(99);
    }

    [Fact]
    public void CurrencyEnum_Should_Have_Expected_Values()
    {
        // Assert
        ((int)CurrencyEnum.USD).Should().Be(1);
        ((int)CurrencyEnum.EUR).Should().Be(2);
        ((int)CurrencyEnum.MXN).Should().Be(3);
        ((int)CurrencyEnum.GBP).Should().Be(4);
        ((int)CurrencyEnum.JPY).Should().Be(5);
        ((int)CurrencyEnum.CAD).Should().Be(6);
    }

    private class TestEntity : BaseEntity
    {
    }
}
