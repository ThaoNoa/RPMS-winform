using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RPMS.DAL.Entities;

namespace RPMS.DAL.Configurations
{
    public class ActivityLogConfiguration : IEntityTypeConfiguration<ActivityLog>
    {
        public void Configure(EntityTypeBuilder<ActivityLog> builder)
        {
            builder.ToTable("ActivityLogs");
            builder.HasKey(a => a.LogID);
            builder.Property(a => a.Action).IsRequired().HasMaxLength(200);
            builder.Property(a => a.IPAddress).HasMaxLength(45);
            builder.Property(a => a.CreatedDate).HasDefaultValueSql("GETDATE()");
            builder.HasOne(a => a.User)
                .WithMany(u => u.ActivityLogs)
                .HasForeignKey(a => a.UserID)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("FK_ActivityLogs_User");
            builder.HasIndex(a => a.UserID);
            builder.HasIndex(a => a.CreatedDate);
        }
    }
}