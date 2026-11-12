using Domain.Flashcards;
using Domain.Sets;
using Infrastructure.Persistence.Converters;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public class FlashcardsConfiguration : IEntityTypeConfiguration<Flashcard>
{
    public void Configure(EntityTypeBuilder<Flashcard> builder)
    {
        builder.Property(x => x.Id)
            .HasConversion(
                id => id.Value,
                value => new FlashcardId(value)
            )
            .ValueGeneratedOnAdd()
            .HasSentinel(new FlashcardId(0));
        
        
        builder.Property(x => x.SetId)
            .HasConversion(id => id.Value, value => new SetId(value))
            .IsRequired();
        builder.HasOne(f => f.Set)
            .WithMany(s => s.Flashcards)
            .HasForeignKey(f => f.SetId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Property(x => x.Term)
            .HasColumnType("varchar(255)")
            .IsRequired();
        builder.Property(x => x.Definition)
            .HasColumnType("varchar(500)")
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