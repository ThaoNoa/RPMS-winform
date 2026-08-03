using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RPMS.DAL.Entities;

namespace RPMS.DAL.Configurations
{
    public class InvoiceConfiguration : IEntityTypeConfiguration<Invoice>
    {
        public void Configure(EntityTypeBuilder<Invoice> builder)
        {
            builder.ToTable("Invoices");
            builder.HasKey(i => i.InvoiceID);
            builder.Property(i => i.InvoiceCode).IsRequired().HasMaxLength(20);
            builder.HasIndex(i => i.InvoiceCode).IsUnique();
            builder.Property(i => i.Rent).HasColumnType("decimal(18,2)").HasDefaultValue(0);
            builder.Property(i => i.ElectricCost).HasColumnType("decimal(18,2)").HasDefaultValue(0);
            builder.Property(i => i.WaterCost).HasColumnType("decimal(18,2)").HasDefaultValue(0);
            builder.Property(i => i.OtherFee).HasColumnType("decimal(18,2)").HasDefaultValue(0);
            builder.Property(i => i.Total).HasColumnType("decimal(18,2)").HasDefaultValue(0);
            builder.Property(i => i.Status).IsRequired().HasMaxLength(20).HasDefaultValue("Unpaid");
            builder.Property(i => i.CreatedDate).HasDefaultValueSql("GETDATE()");
            builder.Property(i => i.UpdatedDate).HasDefaultValueSql("GETDATE()");
            builder.HasOne(i => i.Contract)
                .WithMany(c => c.Invoices)
                .HasForeignKey(i => i.ContractID)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("FK_Invoices_Contract");
            builder.HasOne(i => i.MeterReading)
                .WithMany(m => m.Invoices)
                .HasForeignKey(i => i.ReadingID)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("FK_Invoices_MeterReading");
            builder.HasCheckConstraint("CK_Invoices_Status", "[Status] IN (N'Unpaid', N'Paid', N'Overdue')");
            builder.HasCheckConstraint("CK_Invoices_Total", "Total >= 0");
            builder.HasCheckConstraint("CK_Invoices_Rent", "Rent >= 0");
            builder.HasCheckConstraint("CK_Invoices_ElectricCost", "ElectricCost >= 0");
            builder.HasCheckConstraint("CK_Invoices_WaterCost", "WaterCost >= 0");
            builder.HasCheckConstraint("CK_Invoices_OtherFee", "OtherFee >= 0");
            builder.HasIndex(i => i.ContractID);
            builder.HasIndex(i => i.Status);
        }
    }
}