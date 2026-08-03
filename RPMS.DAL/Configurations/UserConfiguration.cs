using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RPMS.DAL.Entities;

namespace RPMS.DAL.Configurations
{
    public class UserConfiguration : IEntityTypeConfiguration<User>
    {
        public void Configure(EntityTypeBuilder<User> builder)
        {
            builder.ToTable("Users");
            builder.HasKey(u => u.UserID);
            builder.Property(u => u.FullName).IsRequired().HasMaxLength(100);
            builder.Property(u => u.Phone).HasMaxLength(20);
            builder.Property(u => u.Email).HasMaxLength(100);
            builder.HasIndex(u => u.Email).IsUnique();
            builder.Property(u => u.Username).IsRequired().HasMaxLength(50);
            builder.HasIndex(u => u.Username).IsUnique();
            builder.Property(u => u.Password).IsRequired().HasMaxLength(255);
            builder.Property(u => u.Address).HasMaxLength(255);
            builder.Property(u => u.Status).IsRequired().HasMaxLength(20).HasDefaultValue("Active");
            builder.Property(u => u.CreatedDate).HasDefaultValueSql("GETDATE()");
            builder.Property(u => u.UpdatedDate).HasDefaultValueSql("GETDATE()");
            builder.HasOne(u => u.Role)
                .WithMany(r => r.Users)
                .HasForeignKey(u => u.RoleID)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("FK_Users_Role");
            builder.HasCheckConstraint("CK_Users_Status", "[Status] IN (N'Active', N'Inactive')");
            builder.HasIndex(u => u.RoleID);
            builder.HasIndex(u => u.Status);
        }
    }
}