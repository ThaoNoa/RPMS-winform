using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RPMS.DAL.Entities;

namespace RPMS.DAL.Configurations
{
    public class ChatConversationConfiguration : IEntityTypeConfiguration<ChatConversation>
    {
        public void Configure(EntityTypeBuilder<ChatConversation> builder)
        {
            builder.ToTable("ChatConversations");
            builder.HasKey(c => c.ConversationID);
            builder.HasIndex(c => new { c.LandlordID, c.TenantID }).IsUnique().HasDatabaseName("UQ_ChatConversations_Pair");
            builder.Property(c => c.CreatedDate).HasDefaultValueSql("GETDATE()");
            builder.Property(c => c.UpdatedDate).HasDefaultValueSql("GETDATE()");

            builder.HasOne(c => c.Landlord)
                .WithMany()
                .HasForeignKey(c => c.LandlordID)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("FK_ChatConversations_Landlord");

            builder.HasOne(c => c.Tenant)
                .WithMany()
                .HasForeignKey(c => c.TenantID)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("FK_ChatConversations_Tenant");
        }
    }
}
