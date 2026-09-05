using FluentAssertions;
using LiveNow.CRM.API.Services;
using LiveNow.CRM.Core.DTOs;
using LiveNow.CRM.Core.Enums;
using LiveNow.CRM.Core.Interfaces.Services;
using Xunit;

namespace LiveNow.CRM.Tests.Unit;

public class FinancialCalculatorTests
{
    private readonly IFinancialCalculator _calculator = new FinancialCalculatorService();

    [Fact]
    public void Compute_Should_Calculate_GrossProfit_And_Margin()
    {
        // Sale = 1000, Cost = 600, Fees = 35.30
        FinancialComputationDto result = _calculator.Compute(1000m, 600m, 35.30m);

        result.GrossProfit.Should().Be(364.70m);
        result.ProfitMargin.Should().BeApproximately(36.47m, 0.01m);
    }

    [Fact]
    public void Compute_With_Zero_SalePrice_Should_Return_Zero_Margin()
    {
        FinancialComputationDto result = _calculator.Compute(0m, 0m, 0m);

        result.GrossProfit.Should().Be(0m);
        result.ProfitMargin.Should().Be(0m);
    }

    [Fact]
    public void Compute_With_No_Costs_Should_Return_Full_Margin()
    {
        FinancialComputationDto result = _calculator.Compute(500m, 0m, 0m);

        result.GrossProfit.Should().Be(500m);
        result.ProfitMargin.Should().Be(100m);
    }
}

public class PaymentFeeCalculatorTests
{
    private readonly IPaymentFeeCalculator _calculator = new PaymentFeeCalculator();

    [Fact]
    public void Percentage_Only_Should_Calculate_Correctly()
    {
        // 1000 * 3.5% = 35
        decimal result = _calculator.Calculate(1000m, PaymentFeeTypeEnum.Percentage, 3.5m, null);

        result.Should().Be(35m);
    }

    [Fact]
    public void Percentage_Plus_Fixed_Should_Calculate_Correctly()
    {
        // 1000 * 3.5% + 0.30 = 35.30
        decimal result = _calculator.Calculate(1000m, PaymentFeeTypeEnum.PercentagePlusFixed, 3.5m, 0.30m);

        result.Should().Be(35.30m);
    }

    [Fact]
    public void Fixed_Amount_Only_Should_Return_Fixed()
    {
        decimal result = _calculator.Calculate(1000m, PaymentFeeTypeEnum.FixedAmount, null, 25m);

        result.Should().Be(25m);
    }

    [Fact]
    public void No_Fee_Should_Return_Zero()
    {
        decimal result = _calculator.Calculate(1000m, PaymentFeeTypeEnum.NoFee, null, null);

        result.Should().Be(0m);
    }
}
