using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using LiveNow.CRM.Core.Entities;

namespace LiveNow.CRM.Infrastructure.Configurations;

public class CustomerChecklistConfiguration : IEntityTypeConfiguration<CustomerChecklist>
{
    public void Configure(EntityTypeBuilder<CustomerChecklist> builder)
    {
        builder.ToTable("CustomerChecklists");

        builder.HasKey(cc => cc.Id);

        builder.Property(cc => cc.ItemType)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(cc => cc.Description)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(cc => cc.Notes)
            .HasMaxLength(1000);

        builder.HasIndex(cc => cc.CustomerId);
        builder.HasIndex(cc => cc.SaleId);
        builder.HasIndex(cc => cc.IsCompleted);

        // Foreign keys
        builder.HasOne(cc => cc.Customer)
            .WithMany(c => c.Checklists)
            .HasForeignKey(cc => cc.CustomerId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(cc => cc.Sale)
            .WithMany(s => s.Checklists)
            .HasForeignKey(cc => cc.SaleId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
