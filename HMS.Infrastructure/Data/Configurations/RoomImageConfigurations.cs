using HMS.Core.Entities.RoomModule;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HMS.Infrastructure.Data.Configurations
{
    internal class RoomImageConfigurations : BaseConfigurations<RoomImage, int>, IEntityTypeConfiguration<RoomImage>
    {
        public new void Configure(EntityTypeBuilder<RoomImage> builder)
        {
            base.Configure(builder);

            builder.Property(p => p.ImageUrl).HasMaxLength(500);
        }
    }
}
