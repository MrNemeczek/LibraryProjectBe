using System.Diagnostics;
using LibraryProject.Application;
using LibraryProject.Infrastructure;
using Scalar.AspNetCore;

const string ScalarRoute = "/scalar/v1";

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddOpenApi();
builder.Services.AddApplication();
builder.Services.AddInfrastructure();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
    OpenScalarInChrome(app, ScalarRoute);
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();

static void OpenScalarInChrome(WebApplication app, string scalarRoute)
{
    app.Lifetime.ApplicationStarted.Register(() =>
    {
        var scalarUrl = GetScalarUrl(app, scalarRoute);

        try
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = "chrome.exe",
                Arguments = $"\"{scalarUrl}\"",
                UseShellExecute = true
            });
        }
        catch (Exception exception)
        {
            app.Logger.LogWarning(exception, "Could not open Scalar API reference in Chrome.");
        }
    });
}

static string GetScalarUrl(WebApplication app, string scalarRoute)
{
    var baseUrl = app.Urls
        .OrderByDescending(url => url.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
        .FirstOrDefault() ?? "http://localhost:5156";

    return $"{baseUrl.TrimEnd('/')}{scalarRoute}";
}
