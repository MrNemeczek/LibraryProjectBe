using LibraryProject.Domain.Entities;
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
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(book => book.Author)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(book => book.Isbn)
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(book => book.Description)
            .HasMaxLength(2000);

        builder.HasIndex(book => book.Isbn)
            .IsUnique();

        builder.HasIndex(book => book.Title);
        builder.HasIndex(book => book.Author);

        builder.HasOne(book => book.Category)
            .WithMany(category => category.Books)
            .HasForeignKey(book => book.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
