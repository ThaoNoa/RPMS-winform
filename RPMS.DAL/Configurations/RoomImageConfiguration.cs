using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RPMS.DAL.Entities;

namespace RPMS.DAL.Configurations
{
    public class RoomImageConfiguration : IEntityTypeConfiguration<RoomImage>
    {
        public void Configure(EntityTypeBuilder<RoomImage> builder)
        {
            builder.ToTable("RoomImages");
            builder.HasKey(ri => ri.ImageID);
            builder.Property(ri => ri.ImagePath).IsRequired().HasMaxLength(255);
            builder.Property(ri => ri.DisplayOrder).HasDefaultValue(0);
            builder.Property(ri => ri.CreatedDate).HasDefaultValueSql("GETDATE()");
            builder.Property(ri => ri.UpdatedDate).HasDefaultValueSql("GETDATE()");
            builder.HasOne(ri => ri.Room)
                .WithMany(r => r.RoomImages)
                .HasForeignKey(ri => ri.RoomID)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK_RoomImages_Room");
            builder.HasIndex(ri => ri.RoomID);
        }
    }
}