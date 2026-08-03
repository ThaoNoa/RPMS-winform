using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RPMS.DAL.Entities;

namespace RPMS.DAL.Configurations
{
    public class MaintenanceRequestConfiguration : IEntityTypeConfiguration<MaintenanceRequest>
    {
        public void Configure(EntityTypeBuilder<MaintenanceRequest> builder)
        {
            builder.ToTable("MaintenanceRequests");
            builder.HasKey(m => m.RequestID);
            builder.Property(m => m.Title).IsRequired().HasMaxLength(200);
            builder.Property(m => m.Image).HasMaxLength(255);
            builder.Property(m => m.Status).IsRequired().HasMaxLength(20).HasDefaultValue("Pending");
            builder.Property(m => m.CreatedDate).HasDefaultValueSql("GETDATE()");
            builder.Property(m => m.UpdatedDate).HasDefaultValueSql("GETDATE()");
            builder.HasOne(m => m.Contract)
                .WithMany(c => c.MaintenanceRequests)
                .HasForeignKey(m => m.ContractID)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("FK_MaintenanceRequests_Contract");
            builder.HasOne(m => m.Manager)
                .WithMany(u => u.AssignedMaintenanceRequests)
                .HasForeignKey(m => m.AssignedManager)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("FK_MaintenanceRequests_Manager");
            builder.HasCheckConstraint("CK_MaintenanceRequests_Status", "[Status] IN (N'Pending', N'Processing', N'Completed')");
            builder.HasIndex(m => m.ContractID);
            builder.HasIndex(m => m.Status);
        }
    }
}