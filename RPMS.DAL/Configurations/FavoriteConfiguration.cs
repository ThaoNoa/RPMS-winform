using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RPMS.DAL.Entities;

namespace RPMS.DAL.Configurations
{
    public class FavoriteConfiguration : IEntityTypeConfiguration<Favorite>
    {
        public void Configure(EntityTypeBuilder<Favorite> builder)
        {
            builder.ToTable("Favorites");
            builder.HasKey(f => f.FavoriteID);
            builder.Property(f => f.CreatedDate).HasDefaultValueSql("GETDATE()");
            builder.HasIndex(f => new { f.UserID, f.RoomID }).IsUnique().HasDatabaseName("UQ_Favorites_User_Room");
            builder.HasOne(f => f.User)
                .WithMany(u => u.Favorites)
                .HasForeignKey(f => f.UserID)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("FK_Favorites_User");
            builder.HasOne(f => f.Room)
                .WithMany(r => r.Favorites)
                .HasForeignKey(f => f.RoomID)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK_Favorites_Room");
            builder.HasIndex(f => f.UserID);
            builder.HasIndex(f => f.RoomID);
        }
    }
}