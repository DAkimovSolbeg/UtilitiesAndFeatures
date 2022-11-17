using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;

namespace Utilities.Models
{
    public abstract class TrackedEntityConfiguration<TEntity> : IEntityTypeConfiguration<TEntity>
        where TEntity : TrackedEntity
    {
        public virtual void Configure(EntityTypeBuilder<TEntity> builder)
        {
            builder.HasKey(x => x.Id);
            builder.Property(x => x.CreatedById);
            builder.Property(x => x.CreatedOn);
            builder.Property(x => x.UpdatedById);
            builder.Property(x => x.UpdatedOn);
            builder.Property(a => a.Version)
                .IsConcurrencyToken()
                .HasDefaultValue(1);
        }
    }
}
