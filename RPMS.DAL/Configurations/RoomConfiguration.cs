using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RPMS.DAL.Entities;

namespace RPMS.DAL.Configurations
{
    public class RoomConfiguration : IEntityTypeConfiguration<Room>
    {
        public void Configure(EntityTypeBuilder<Room> builder)
        {
            builder.ToTable("Rooms");
            builder.HasKey(r => r.RoomID);
            builder.Property(r => r.RoomNumber).IsRequired().HasMaxLength(20);
            builder.Property(r => r.Area).HasColumnType("decimal(10,2)").IsRequired();
            builder.Property(r => r.Price).HasColumnType("decimal(18,2)").IsRequired();
            builder.Property(r => r.Capacity).IsRequired().HasDefaultValue(1);
            builder.Property(r => r.Bedroom).IsRequired().HasDefaultValue(0);
            builder.Property(r => r.Bathroom).IsRequired().HasDefaultValue(0);
            builder.Property(r => r.Furniture).HasMaxLength(500);
            builder.Property(r => r.Status).IsRequired().HasMaxLength(20).HasDefaultValue("Available");
            builder.Property(r => r.CreatedDate).HasDefaultValueSql("GETDATE()");
            builder.Property(r => r.UpdatedDate).HasDefaultValueSql("GETDATE()");
            builder.HasIndex(r => new { r.HouseID, r.RoomNumber }).IsUnique().HasDatabaseName("UQ_Rooms_House_RoomNumber");
            builder.HasOne(r => r.House)
                .WithMany(h => h.Rooms)
                .HasForeignKey(r => r.HouseID)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("FK_Rooms_House");
            builder.HasCheckConstraint("CK_Rooms_Status", "[Status] IN (N'Available', N'Occupied', N'Maintenance')");
            builder.HasCheckConstraint("CK_Rooms_Price", "Price > 0");
            builder.HasCheckConstraint("CK_Rooms_Area", "Area > 0");
            builder.HasCheckConstraint("CK_Rooms_Capacity", "Capacity >= 1");
            builder.HasCheckConstraint("CK_Rooms_Bedroom", "Bedroom >= 0");
            builder.HasCheckConstraint("CK_Rooms_Bathroom", "Bathroom >= 0");
            builder.HasIndex(r => r.HouseID);
            builder.HasIndex(r => r.Status);
            builder.HasIndex(r => r.Price);
        }
    }
}