using LibraryProject.Domain.Entities;
using LibraryProject.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LibraryProject.Infrastructure.Persistence.Configurations;

internal sealed class BookConfiguration : IEntityTypeConfiguration<Book>
{
    public void Configure(EntityTypeBuilder<Book> builder)
    {
        builder.ToTable("Books");

        builder.HasKey(book => book.Id);

        builder.Property(book => book.Title)
            .HasMaxLength(Book.MaxTitleLength)
            .IsRequired();

        builder.Property(book => book.Author)
            .HasMaxLength(Book.MaxAuthorLength)
            .IsRequired();

        builder.Property(book => book.Isbn)
            .HasConversion(
                isbn => isbn.Value,
                value => Isbn.Create(value))
            .HasMaxLength(Isbn.MaxLength)
            .IsRequired();

        builder.Property(book => book.Description)
            .HasMaxLength(Book.MaxDescriptionLength);

        builder.Property(book => book.IsDeleted)
            .HasDefaultValue(false)
            .IsRequired();

        builder.Property(book => book.DeletedAt);

        builder.HasIndex(book => book.Isbn)
            .IsUnique()
            .HasFilter("[IsDeleted] = 0");

        builder.HasIndex(book => book.Title);
        builder.HasIndex(book => book.Author);

        builder.HasQueryFilter(book => !book.IsDeleted);

        builder.HasOne(book => book.Category)
            .WithMany(category => category.Books)
            .HasForeignKey(book => book.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
