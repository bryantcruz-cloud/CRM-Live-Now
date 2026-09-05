using Microsoft.EntityFrameworkCore;
using LiveNow.CRM.Core.Entities;

namespace LiveNow.CRM.Infrastructure.Data;

public class LiveNowDbContext : DbContext
{
    public LiveNowDbContext(DbContextOptions<LiveNowDbContext> options)
        : base(options)
    {
    }

    // CRM Entities
    public DbSet<Customer> Customers => Set<Customer>();
    public DbSet<Race> Races => Set<Race>();
    public DbSet<RaceEdition> RaceEditions => Set<RaceEdition>();
    public DbSet<RaceSlot> RaceSlots => Set<RaceSlot>();
    public DbSet<Supplier> Suppliers => Set<Supplier>();
    public DbSet<Quote> Quotes => Set<Quote>();
    public DbSet<QuoteItem> QuoteItems => Set<QuoteItem>();
    public DbSet<Sale> Sales => Set<Sale>();
    public DbSet<SaleItem> SaleItems => Set<SaleItem>();
    public DbSet<Payment> Payments => Set<Payment>();
    public DbSet<PaymentFee> PaymentFees => Set<PaymentFee>();
    public DbSet<Hotel> Hotels => Set<Hotel>();
    public DbSet<HotelReservation> HotelReservations => Set<HotelReservation>();
    public DbSet<RunnerRegistration> RunnerRegistrations => Set<RunnerRegistration>();
    public DbSet<CustomerChecklist> CustomerChecklists => Set<CustomerChecklist>();
    public DbSet<Cancellation> Cancellations => Set<Cancellation>();
    public DbSet<SlotTransfer> SlotTransfers => Set<SlotTransfer>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();
    public DbSet<User> Users => Set<User>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(LiveNowDbContext).Assembly);
        SeedData.Seed(modelBuilder);
    }
}
