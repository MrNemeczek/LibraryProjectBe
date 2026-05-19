using LibraryProject.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LibraryProject.Infrastructure.Persistence.Configurations;

internal sealed class BookCopyConfiguration : IEntityTypeConfiguration<BookCopy>
{
    public void Configure(EntityTypeBuilder<BookCopy> builder)
    {
        builder.ToTable("BookCopies");

        builder.HasKey(copy => copy.Id);

        builder.Property(copy => copy.InventoryNumber)
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(copy => copy.Status)
            .HasConversion<string>()
            .HasMaxLength(32)
            .IsRequired();

        builder.HasIndex(copy => copy.InventoryNumber)
            .IsUnique();

        builder.HasIndex(copy => new { copy.BookId, copy.Status });

        builder.HasOne(copy => copy.Book)
            .WithMany(book => book.Copies)
            .HasForeignKey(copy => copy.BookId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
