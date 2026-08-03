using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RPMS.DAL.Entities;

namespace RPMS.DAL.Configurations
{
    public class NotificationConfiguration : IEntityTypeConfiguration<Notification>
    {
        public void Configure(EntityTypeBuilder<Notification> builder)
        {
            builder.ToTable("Notifications");
            builder.HasKey(n => n.NotificationID);
            builder.Property(n => n.Title).IsRequired().HasMaxLength(200);
            builder.Property(n => n.Content).IsRequired();
            builder.Property(n => n.IsRead).HasDefaultValue(false);
            builder.Property(n => n.CreatedDate).HasDefaultValueSql("GETDATE()");
            builder.Property(n => n.UpdatedDate).HasDefaultValueSql("GETDATE()");
            builder.HasOne(n => n.User)
                .WithMany(u => u.Notifications)
                .HasForeignKey(n => n.UserID)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("FK_Notifications_User");
            builder.HasIndex(n => n.UserID);
            builder.HasIndex(n => n.IsRead);
        }
    }
}