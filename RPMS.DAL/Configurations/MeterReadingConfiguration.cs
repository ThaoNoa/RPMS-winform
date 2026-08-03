using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RPMS.DAL.Entities;

namespace RPMS.DAL.Configurations
{
    public class MeterReadingConfiguration : IEntityTypeConfiguration<MeterReading>
    {
        public void Configure(EntityTypeBuilder<MeterReading> builder)
        {
            builder.ToTable("MeterReadings");
            builder.HasKey(m => m.ReadingID);
            builder.Property(m => m.OldElectric).HasColumnType("decimal(18,2)").HasDefaultValue(0);
            builder.Property(m => m.NewElectric).HasColumnType("decimal(18,2)").HasDefaultValue(0);
            builder.Property(m => m.OldWater).HasColumnType("decimal(18,2)").HasDefaultValue(0);
            builder.Property(m => m.NewWater).HasColumnType("decimal(18,2)").HasDefaultValue(0);
            builder.Property(m => m.CreatedDate).HasDefaultValueSql("GETDATE()");
            builder.Property(m => m.UpdatedDate).HasDefaultValueSql("GETDATE()");
            builder.HasOne(m => m.Contract)
                .WithMany(c => c.MeterReadings)
                .HasForeignKey(m => m.ContractID)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("FK_MeterReadings_Contract");
            builder.HasOne(m => m.CreatedByUser)
                .WithMany(u => u.CreatedMeterReadings)
                .HasForeignKey(m => m.CreatedBy)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("FK_MeterReadings_User");
            builder.HasCheckConstraint("CK_MeterReadings_Electric", "NewElectric >= OldElectric");
            builder.HasCheckConstraint("CK_MeterReadings_Water", "NewWater >= OldWater");
            builder.HasIndex(m => m.ContractID);
            builder.HasIndex(m => m.ReadingMonth);
        }
    }
}