using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RPMS.DAL.Entities;

namespace RPMS.DAL.Configurations
{
    public class RoomAmenityConfiguration : IEntityTypeConfiguration<RoomAmenity>
    {
        public void Configure(EntityTypeBuilder<RoomAmenity> builder)
        {
            builder.ToTable("RoomAmenities");
            builder.HasKey(ra => ra.RoomAmenityID);
            builder.HasIndex(ra => new { ra.RoomID, ra.AmenityID }).IsUnique().HasDatabaseName("UQ_RoomAmenities_Room_Amenity");
            builder.HasOne(ra => ra.Room)
                .WithMany(r => r.RoomAmenities)
                .HasForeignKey(ra => ra.RoomID)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK_RoomAmenities_Room");
            builder.HasOne(ra => ra.Amenity)
                .WithMany(a => a.RoomAmenities)
                .HasForeignKey(ra => ra.AmenityID)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("FK_RoomAmenities_Amenity");
            builder.HasIndex(ra => ra.RoomID);
        }
    }
}