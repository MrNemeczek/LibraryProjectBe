using LibraryProject.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LibraryProject.Infrastructure.Persistence.Configurations;

internal sealed class LoanConfiguration : IEntityTypeConfiguration<Loan>
{
    public void Configure(EntityTypeBuilder<Loan> builder)
    {
        builder.ToTable("Loans");

        builder.HasKey(loan => loan.Id);

        builder.Property(loan => loan.LoanDate)
            .HasColumnType("date")
            .IsRequired();

        builder.Property(loan => loan.ReturnDate)
            .HasColumnType("date");

        builder.Property(loan => loan.Status)
            .HasConversion<string>()
            .HasMaxLength(32)
            .IsRequired();

        builder.HasIndex(loan => new { loan.UserId, loan.Status });
        builder.HasIndex(loan => new { loan.BookCopyId, loan.Status });

        builder.HasIndex(loan => loan.ReservationId)
            .IsUnique()
            .HasFilter("[ReservationId] IS NOT NULL");

        builder.HasOne(loan => loan.User)
            .WithMany(user => user.Loans)
            .HasForeignKey(loan => loan.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(loan => loan.BookCopy)
            .WithMany(copy => copy.Loans)
            .HasForeignKey(loan => loan.BookCopyId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(loan => loan.Reservation)
            .WithMany()
            .HasForeignKey(loan => loan.ReservationId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
