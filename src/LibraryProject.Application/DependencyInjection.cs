using LibraryProject.Application.Authentication;
using LibraryProject.Application.Books;
using LibraryProject.Application.Categories;
using LibraryProject.Application.Loans;
using LibraryProject.Application.Reservations;
using Microsoft.Extensions.DependencyInjection;

namespace LibraryProject.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<IAuthenticationService, AuthenticationService>();
        services.AddScoped<IBookService, BookService>();
        services.AddScoped<ICategoryService, CategoryService>();
        services.AddScoped<IReservationService, ReservationService>();
        services.AddScoped<ILoanService, LoanService>();

        return services;
    }
}
