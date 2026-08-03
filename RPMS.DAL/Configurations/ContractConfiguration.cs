using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RPMS.DAL.Entities;

namespace RPMS.DAL.Configurations
{
    public class ContractConfiguration : IEntityTypeConfiguration<Contract>
    {
        public void Configure(EntityTypeBuilder<Contract> builder)
        {
            builder.ToTable("Contracts");
            builder.HasKey(c => c.ContractID);
            builder.Property(c => c.ContractCode).IsRequired().HasMaxLength(20);
            builder.HasIndex(c => c.ContractCode).IsUnique();
            builder.Property(c => c.Deposit).HasColumnType("decimal(18,2)").HasDefaultValue(0);
            builder.Property(c => c.MonthlyRent).HasColumnType("decimal(18,2)").IsRequired();
            builder.Property(c => c.ElectricPrice).HasColumnType("decimal(18,2)").HasDefaultValue(0);
            builder.Property(c => c.WaterPrice).HasColumnType("decimal(18,2)").HasDefaultValue(0);
            builder.Property(c => c.Status).IsRequired().HasMaxLength(20).HasDefaultValue("Active");
            builder.Property(c => c.CreatedDate).HasDefaultValueSql("GETDATE()");
            builder.Property(c => c.UpdatedDate).HasDefaultValueSql("GETDATE()");
            builder.HasOne(c => c.Room)
                .WithMany(r => r.Contracts)
                .HasForeignKey(c => c.RoomID)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("FK_Contracts_Room");
            builder.HasOne(c => c.Tenant)
                .WithMany(u => u.TenantContracts)
                .HasForeignKey(c => c.TenantID)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("FK_Contracts_Tenant");
            builder.HasOne(c => c.CreatedByUser)
                .WithMany(u => u.CreatedContracts)
                .HasForeignKey(c => c.CreatedBy)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("FK_Contracts_CreatedBy");
            builder.HasCheckConstraint("CK_Contracts_Status", "[Status] IN (N'Draft', N'Active', N'Expired', N'Terminated')");
            builder.HasCheckConstraint("CK_Contracts_Date", "EndDate >= StartDate");
            builder.HasCheckConstraint("CK_Contracts_MoveOut", "MoveOutDate IS NULL OR MoveOutDate >= MoveInDate");
            builder.HasCheckConstraint("CK_Contracts_Deposit", "Deposit >= 0");
            builder.HasCheckConstraint("CK_Contracts_MonthlyRent", "MonthlyRent > 0");
            builder.HasCheckConstraint("CK_Contracts_ElectricWater", "ElectricPrice >= 0 AND WaterPrice >= 0");
            builder.HasIndex(c => c.RoomID);
            builder.HasIndex(c => c.TenantID);
            builder.HasIndex(c => c.Status);
        }
    }
}