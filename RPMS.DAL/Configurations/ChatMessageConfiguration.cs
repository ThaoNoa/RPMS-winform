using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RPMS.DAL.Entities;

namespace RPMS.DAL.Configurations
{
    public class ChatMessageConfiguration : IEntityTypeConfiguration<ChatMessage>
    {
        public void Configure(EntityTypeBuilder<ChatMessage> builder)
        {
            builder.ToTable("ChatMessages");
            builder.HasKey(m => m.MessageID);
            builder.Property(m => m.Content).HasMaxLength(4000);
            builder.Property(m => m.ImagePath).HasMaxLength(255);
            builder.Property(m => m.IsRead).HasDefaultValue(false);
            builder.Property(m => m.CreatedDate).HasDefaultValueSql("GETDATE()");
            builder.HasIndex(m => m.ConversationID);

            builder.HasOne(m => m.Conversation)
                .WithMany(c => c.Messages)
                .HasForeignKey(m => m.ConversationID)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK_ChatMessages_Conversation");

            builder.HasOne(m => m.Sender)
                .WithMany()
                .HasForeignKey(m => m.SenderID)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("FK_ChatMessages_Sender");
        }
    }
}
