using FluentAssertions;
using LibraryProject.Domain.Common;
using LibraryProject.Domain.Entities;
using LibraryProject.Domain.Enums;

namespace LibraryProject.Application.Tests.Domain;

public class ReservationEntityTests
{
    [Fact]
    public void Create_with_default_deadline_should_set_properties()
    {
        var reservation = Reservation.Create(1, 2);

        reservation.UserId.Should().Be(1);
        reservation.BookId.Should().Be(2);
        reservation.Status.Should().Be(ReservationStatus.Active);
        reservation.ReservationDate.Should().Be(DateOnly.FromDateTime(DateTime.UtcNow));
        reservation.PickupDeadline.Should().Be(
            DateOnly.FromDateTime(DateTime.UtcNow).AddDays(Reservation.DefaultPickupDeadlineDays));
    }

    [Fact]
    public void Create_with_custom_deadline_should_use_it()
    {
        var reservation = Reservation.Create(1, 2, 5);

        reservation.PickupDeadline.Should().Be(
            DateOnly.FromDateTime(DateTime.UtcNow).AddDays(5));
    }

    [Fact]
    public void Create_should_throw_when_pickup_deadline_is_less_than_1()
    {
        var act = () => Reservation.Create(1, 2, 0);
        act.Should().Throw<DomainValidationException>()
            .Which.Code.Should().Be("RESERVATION_INVALID_PICKUP_DEADLINE");
    }

    [Fact]
    public void Create_should_throw_when_pickup_deadline_exceeds_max()
    {
        var act = () => Reservation.Create(1, 2, 8);
        act.Should().Throw<DomainValidationException>()
            .Which.Code.Should().Be("RESERVATION_INVALID_PICKUP_DEADLINE");
    }

    [Fact]
    public void Cancel_should_change_status_to_cancelled()
    {
        var reservation = Reservation.Create(1, 2);

        reservation.Cancel();

        reservation.Status.Should().Be(ReservationStatus.Cancelled);
    }

    [Fact]
    public void Fulfill_should_change_status_to_fulfilled()
    {
        var reservation = Reservation.Create(1, 2);

        reservation.Fulfill();

        reservation.Status.Should().Be(ReservationStatus.Fulfilled);
    }

    [Fact]
    public void Expire_should_change_status_to_expired()
    {
        var reservation = Reservation.Create(1, 2);

        reservation.Expire();

        reservation.Status.Should().Be(ReservationStatus.Expired);
    }

    [Theory]
    [InlineData(ReservationStatus.Fulfilled)]
    [InlineData(ReservationStatus.Cancelled)]
    [InlineData(ReservationStatus.Expired)]
    public void Cancel_should_throw_when_not_active(ReservationStatus status)
    {
        var reservation = Reservation.Create(1, 2);
        SetReservationStatus(reservation, status);

        var act = () => reservation.Cancel();
        act.Should().Throw<DomainValidationException>()
            .Which.Code.Should().Be("RESERVATION_NOT_ACTIVE");
    }

    [Theory]
    [InlineData(ReservationStatus.Fulfilled)]
    [InlineData(ReservationStatus.Cancelled)]
    [InlineData(ReservationStatus.Expired)]
    public void Fulfill_should_throw_when_not_active(ReservationStatus status)
    {
        var reservation = Reservation.Create(1, 2);
        SetReservationStatus(reservation, status);

        var act = () => reservation.Fulfill();
        act.Should().Throw<DomainValidationException>()
            .Which.Code.Should().Be("RESERVATION_NOT_ACTIVE");
    }

    private static void SetReservationStatus(Reservation reservation, ReservationStatus status)
    {
        var prop = typeof(Reservation).GetProperty("Status")!;
        prop.SetValue(reservation, status);
    }
}
