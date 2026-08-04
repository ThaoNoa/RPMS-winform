using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RPMS.DAL.Entities;

namespace RPMS.DAL.Configurations
{
    public class AppointmentConfiguration : IEntityTypeConfiguration<Appointment>
    {
        public void Configure(EntityTypeBuilder<Appointment> builder)
        {
            builder.ToTable("Appointments");
            builder.HasKey(a => a.AppointmentID);
            builder.Property(a => a.Status).IsRequired().HasMaxLength(20).HasDefaultValue("Pending");
            builder.Property(a => a.CreatedDate).HasDefaultValueSql("GETDATE()");
            builder.Property(a => a.UpdatedDate).HasDefaultValueSql("GETDATE()");
            builder.HasOne(a => a.Room)
                .WithMany(r => r.Appointments)
                .HasForeignKey(a => a.RoomID)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK_Appointments_Room");
            builder.HasOne(a => a.Tenant)
                .WithMany(u => u.Appointments)
                .HasForeignKey(a => a.TenantID)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("FK_Appointments_Tenant");
            builder.HasCheckConstraint("CK_Appointments_Status", "[Status] IN (N'Pending', N'Accepted', N'Rejected', N'Completed', N'Cancelled')");
            builder.HasIndex(a => a.RoomID);
            builder.HasIndex(a => a.TenantID);
            builder.HasIndex(a => a.Status);
        }
    }
}