using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RPMS.DAL.Entities;

namespace RPMS.DAL.Configurations
{
    public class PostImageConfiguration : IEntityTypeConfiguration<PostImage>
    {
        public void Configure(EntityTypeBuilder<PostImage> builder)
        {
            builder.ToTable("PostImages");
            builder.HasKey(pi => pi.PostImageID);
            builder.Property(pi => pi.ImagePath).IsRequired().HasMaxLength(255);
            builder.Property(pi => pi.IsMain).HasDefaultValue(false);
            builder.Property(pi => pi.DisplayOrder).HasDefaultValue(0);
            builder.Property(pi => pi.CreatedDate).HasDefaultValueSql("GETDATE()");
            builder.Property(pi => pi.UpdatedDate).HasDefaultValueSql("GETDATE()");
            builder.HasOne(pi => pi.Post)
                .WithMany(p => p.PostImages)
                .HasForeignKey(pi => pi.PostID)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK_PostImages_Post");
            builder.HasIndex(pi => pi.PostID);
        }
    }
}