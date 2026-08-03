using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RPMS.DAL.Entities;

namespace RPMS.DAL.Configurations
{
    public class HouseConfiguration : IEntityTypeConfiguration<House>
    {
        public void Configure(EntityTypeBuilder<House> builder)
        {
            builder.ToTable("Houses");
            builder.HasKey(h => h.HouseID);
            builder.Property(h => h.HouseName).IsRequired().HasMaxLength(100);
            builder.Property(h => h.Address).IsRequired().HasMaxLength(255);
            builder.Property(h => h.Status).IsRequired().HasMaxLength(20).HasDefaultValue("Active");
            builder.Property(h => h.CreatedDate).HasDefaultValueSql("GETDATE()");
            builder.Property(h => h.UpdatedDate).HasDefaultValueSql("GETDATE()");
            builder.HasOne(h => h.Owner)
                .WithMany(u => u.Houses)
                .HasForeignKey(h => h.OwnerID)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("FK_Houses_User");
            builder.HasCheckConstraint("CK_Houses_Status", "[Status] IN (N'Active', N'Inactive')");
            builder.HasIndex(h => h.OwnerID);
        }
    }
}