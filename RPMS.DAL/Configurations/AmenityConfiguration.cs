using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RPMS.DAL.Entities;

namespace RPMS.DAL.Configurations
{
    public class AmenityConfiguration : IEntityTypeConfiguration<Amenity>
    {
        public void Configure(EntityTypeBuilder<Amenity> builder)
        {
            builder.ToTable("Amenities");
            builder.HasKey(a => a.AmenityID);
            builder.Property(a => a.AmenityName).IsRequired().HasMaxLength(100);
            builder.HasIndex(a => a.AmenityName).IsUnique();
        }
    }
}