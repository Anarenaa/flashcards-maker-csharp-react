using Domain.Sets;
using Infrastructure.Persistence.Converters;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public class SetsConfiguration : IEntityTypeConfiguration<Set>
{
    public void Configure(EntityTypeBuilder<Set> builder)
    {
        builder.Property(x => x.Id)
            .HasConversion(
                id => id.Value,
                value => new SetId(value)
            )
            .ValueGeneratedOnAdd()
            .HasSentinel(new SetId(0));

        builder.Property(x => x.Name)
            .HasColumnType("varchar(255)")
            .IsRequired();
        builder.HasIndex(x => x.Name)
            .IsUnique();
        builder.Property(x => x.Description)
            .HasColumnType("varchar(500)");
        
        builder.Property(x => x.Type)
            .HasConversion(x => x.Value, x => SetType.From(x))
            .HasColumnType("varchar(20)")
            .IsRequired();
    
        builder.Property(x => x.FromLang)
            .HasColumnType("varchar(2)")
            .IsRequired();
        builder.Property(x => x.ToLang)
            .HasColumnType("varchar(2)")
            .IsRequired();
       
        builder.Property(x => x.IsPublic)
            .HasColumnType("boolean")
            .IsRequired();
        builder.Property(x => x.IsGenerated)
            .HasColumnType("boolean")
            .IsRequired();

        builder.Property(x => x.CreatedAt)
            .HasConversion(new DateTimeUtcConverter())
            .HasColumnType("timestamp with time zone")
            .HasDefaultValueSql("timezone('utc', now())")
            .IsRequired();

        builder.Property(x => x.UpdatedAt)
            .HasConversion(new DateTimeUtcConverter())
            .HasColumnType("timestamp with time zone");
    }
}