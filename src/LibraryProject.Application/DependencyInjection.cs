using LibraryProject.Application.Authentication;
using LibraryProject.Application.Books;
using LibraryProject.Application.Reservations;
using Microsoft.Extensions.DependencyInjection;

namespace LibraryProject.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<IAuthenticationService, AuthenticationService>();
        services.AddScoped<IBookService, BookService>();
        services.AddScoped<IReservationService, ReservationService>();

        return services;
    }
}
