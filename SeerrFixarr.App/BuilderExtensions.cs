using System.Reflection;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SeerrFixarr.Shared.Settings;

namespace SeerrFixarr.App;

public static class BuilderExtensions
{
    public static void AddSeerrFixerrSettings(this WebApplicationBuilder builder)
    {
        builder.Configuration
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .AddEnvironmentVariables()
            .AddUserSecrets(Assembly.GetExecutingAssembly(), optional: true);
        builder.Services.Configure<SeerrFixarrSettings>(builder.Configuration);
    }
}