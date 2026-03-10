
using HMS.Core.Entities.RoomModule;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HMS.Infrastructure.Data.Configurations
{
    internal class RoomConfigurations : BaseConfigurations<Room, int>, IEntityTypeConfiguration<Room>
    {
        public new void Configure(EntityTypeBuilder<Room> builder)
        {
            base.Configure(builder);

            builder.Property(p => p.Id).UseIdentityColumn(100, 1);
            builder.Property(p => p.Description).HasMaxLength(150);
            builder.Property(p => p.PricePerNight).HasPrecision(18, 2);
        }
    }
}
