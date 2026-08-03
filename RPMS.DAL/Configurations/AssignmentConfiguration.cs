using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RPMS.DAL.Entities;

namespace RPMS.DAL.Configurations
{
    public class AssignmentConfiguration : IEntityTypeConfiguration<Assignment>
    {
        public void Configure(EntityTypeBuilder<Assignment> builder)
        {
            builder.ToTable("Assignments");
            builder.HasKey(a => a.AssignmentID);
            builder.Property(a => a.AssignedDate).HasDefaultValueSql("GETDATE()");
            builder.Property(a => a.Status).IsRequired().HasMaxLength(20).HasDefaultValue("Active");
            builder.Property(a => a.CreatedDate).HasDefaultValueSql("GETDATE()");
            builder.Property(a => a.UpdatedDate).HasDefaultValueSql("GETDATE()");
            builder.HasIndex(a => new { a.HouseID, a.ManagerID }).IsUnique().HasDatabaseName("UQ_Assignments_House_Manager");
            builder.HasOne(a => a.House)
                .WithMany(h => h.Assignments)
                .HasForeignKey(a => a.HouseID)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("FK_Assignments_House");
            builder.HasOne(a => a.Manager)
                .WithMany(u => u.Assignments)
                .HasForeignKey(a => a.ManagerID)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("FK_Assignments_Manager");
            builder.HasCheckConstraint("CK_Assignments_Status", "[Status] IN (N'Active', N'Inactive')");
            builder.HasIndex(a => a.HouseID);
            builder.HasIndex(a => a.ManagerID);
        }
    }
}