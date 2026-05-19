using LibraryProject.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace LibraryProject.Api.Extensions;

public static class MigrationExtensions
{
    public static WebApplication ApplyMigrations(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();

        var dbContext = scope.ServiceProvider.GetRequiredService<LibraryDbContext>();
        dbContext.Database.Migrate();

        return app;
    }
}
