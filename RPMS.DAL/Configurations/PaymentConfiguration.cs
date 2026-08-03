using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RPMS.DAL.Entities;

namespace RPMS.DAL.Configurations
{
    public class PaymentConfiguration : IEntityTypeConfiguration<Payment>
    {
        public void Configure(EntityTypeBuilder<Payment> builder)
        {
            builder.ToTable("Payments");
            builder.HasKey(p => p.PaymentID);
            builder.Property(p => p.PaymentDate).HasDefaultValueSql("GETDATE()");
            builder.Property(p => p.Amount).HasColumnType("decimal(18,2)").IsRequired();
            builder.Property(p => p.Method).IsRequired().HasMaxLength(50);
            builder.Property(p => p.Status).IsRequired().HasMaxLength(20).HasDefaultValue("Completed");
            builder.Property(p => p.CreatedDate).HasDefaultValueSql("GETDATE()");
            builder.Property(p => p.UpdatedDate).HasDefaultValueSql("GETDATE()");
            builder.HasOne(p => p.Invoice)
                .WithMany(i => i.Payments)
                .HasForeignKey(p => p.InvoiceID)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("FK_Payments_Invoice");
            builder.HasCheckConstraint("CK_Payments_Method", "Method IN (N'Cash', N'Banking', N'Momo', N'VNPay', N'ZaloPay')");
            builder.HasCheckConstraint("CK_Payments_Status", "[Status] IN (N'Pending', N'Completed', N'Failed')");
            builder.HasCheckConstraint("CK_Payments_Amount", "Amount > 0");
            builder.HasIndex(p => p.InvoiceID);
            builder.HasIndex(p => p.Method);
            builder.HasIndex(p => p.Status);
        }
    }
}