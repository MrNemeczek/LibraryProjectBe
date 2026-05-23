using LibraryProject.Application.Authentication;
using LibraryProject.Application.Books;
using Microsoft.Extensions.DependencyInjection;

namespace LibraryProject.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<IAuthenticationService, AuthenticationService>();
        services.AddScoped<IBookService, BookService>();

        return services;
    }
}
