using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RPMS.DAL.Entities;

namespace RPMS.DAL.Configurations
{
    public class PostConfiguration : IEntityTypeConfiguration<Post>
    {
        public void Configure(EntityTypeBuilder<Post> builder)
        {
            builder.ToTable("Posts");
            builder.HasKey(p => p.PostID);
            builder.Property(p => p.Title).IsRequired().HasMaxLength(200);
            builder.Property(p => p.PriceSnapshot).HasColumnType("decimal(18,2)").IsRequired();
            builder.Property(p => p.Status).IsRequired().HasMaxLength(20).HasDefaultValue("Pending");
            builder.Property(p => p.ViewCount).HasDefaultValue(0);
            builder.Property(p => p.IsFeatured).HasDefaultValue(false);
            builder.Property(p => p.CreatedDate).HasDefaultValueSql("GETDATE()");
            builder.Property(p => p.UpdatedDate).HasDefaultValueSql("GETDATE()");
            builder.HasOne(p => p.Room)
                .WithMany(r => r.Posts)
                .HasForeignKey(p => p.RoomID)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK_Posts_Room");
            builder.HasOne(p => p.ApprovedByUser)
                .WithMany(u => u.ApprovedPosts)
                .HasForeignKey(p => p.ApprovedBy)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("FK_Posts_ApprovedBy");
            builder.HasCheckConstraint("CK_Posts_Status", "[Status] IN (N'Pending', N'Approved', N'Rejected')");
            builder.HasCheckConstraint("CK_Posts_PriceSnapshot", "PriceSnapshot > 0");
            builder.HasIndex(p => p.RoomID);
            builder.HasIndex(p => p.Status);
            builder.HasIndex(p => p.ExpiryDate);
            builder.HasIndex(p => p.IsFeatured);
        }
    }
}