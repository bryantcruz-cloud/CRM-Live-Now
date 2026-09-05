using FluentAssertions;
using LiveNow.CRM.Core.DTOs;
using LiveNow.CRM.Core.Entities;
using LiveNow.CRM.Core.Enums;
using Microsoft.EntityFrameworkCore;

namespace LiveNow.CRM.Tests.Unit;

public class ReportingServiceTests : ServiceTestBase
{
    [Fact]
    public async Task OperationalSummary_Should_Count_Operational_States()
    {
        Customer customer = await SeedCustomerAsync();
        RaceEdition edition = await SeedEditionAsync();
        await SeedSlotAsync(edition.Id);
        Sale sale = await SeedSaleWithSlotAsync(customer, edition, await SeedSlotAsync(edition.Id));
        Context.CustomerChecklists.Add(new CustomerChecklist { CustomerId = customer.Id, SaleId = sale.Id, ItemType = "Docs", Description = "Passport", IsCompleted = false });
        Context.Cancellations.Add(new Cancellation { CustomerId = customer.Id, SaleId = sale.Id, Reason = "Injury", IsInjury = true });
        await Context.SaveChangesAsync();

        OperationalSummaryDto result = await ReportingService.GetOperationalSummaryAsync();

        result.TotalCustomers.Should().BeGreaterThanOrEqualTo(1);
        result.PendingRegistrations.Should().BeGreaterThanOrEqualTo(0);
        result.TotalCancellations.Should().Be(1);
        result.InjuredCustomers.Should().Be(1);
        result.AvailableSlots.Should().BeGreaterThanOrEqualTo(1);
    }

    [Fact]
    public async Task AuditLogs_Should_Filter_By_Entity_And_Action()
    {
        Customer customer = await SeedCustomerAsync();
        IReadOnlyList<AuditLogDto> result = await ReportingService.GetRecentAuditLogsAsync(50, entity: "Customer", action: AuditActionEnum.Create);

        result.Should().NotBeEmpty();
        result.Should().OnlyContain(log => log.EntityName == "Customer" && log.Action == nameof(AuditActionEnum.Create));
        (await Context.Customers.CountAsync(c => c.Id == customer.Id)).Should().Be(1);
    }
}
