using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RPMS.DAL.Entities;

namespace RPMS.DAL.Configurations
{
    public class ReviewConfiguration : IEntityTypeConfiguration<Review>
    {
        public void Configure(EntityTypeBuilder<Review> builder)
        {
            builder.ToTable("Reviews");
            builder.HasKey(r => r.ReviewID);
            builder.HasIndex(r => r.ContractID).IsUnique();
            builder.Property(r => r.CreatedDate).HasDefaultValueSql("GETDATE()");
            builder.Property(r => r.UpdatedDate).HasDefaultValueSql("GETDATE()");
            builder.HasOne(r => r.Contract)
                .WithOne(c => c.Review)
                .HasForeignKey<Review>(r => r.ContractID)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("FK_Reviews_Contract");
            builder.HasCheckConstraint("CK_Reviews_Rating", "Rating BETWEEN 1 AND 5");
        }
    }
}