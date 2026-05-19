using LibraryProject.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LibraryProject.Infrastructure.Persistence.Configurations;

internal sealed class ReservationConfiguration : IEntityTypeConfiguration<Reservation>
{
    public void Configure(EntityTypeBuilder<Reservation> builder)
    {
        builder.ToTable("Reservations");

        builder.HasKey(reservation => reservation.Id);

        builder.Property(reservation => reservation.ReservationDate)
            .HasColumnType("date")
            .IsRequired();

        builder.Property(reservation => reservation.PickupDeadline)
            .HasColumnType("date");

        builder.Property(reservation => reservation.Status)
            .HasConversion<string>()
            .HasMaxLength(32)
            .IsRequired();

        builder.HasIndex(reservation => new { reservation.UserId, reservation.Status });
        builder.HasIndex(reservation => new { reservation.BookId, reservation.Status });

        builder.HasOne(reservation => reservation.User)
            .WithMany(user => user.Reservations)
            .HasForeignKey(reservation => reservation.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(reservation => reservation.Book)
            .WithMany(book => book.Reservations)
            .HasForeignKey(reservation => reservation.BookId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
