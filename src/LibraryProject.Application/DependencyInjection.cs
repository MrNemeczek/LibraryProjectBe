using LibraryProject.Application.Authentication;
using Microsoft.Extensions.DependencyInjection;

namespace LibraryProject.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<IAuthenticationService, AuthenticationService>();

        return services;
    }
}
