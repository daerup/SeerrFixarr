using System;
using Serilog;
using System.Globalization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;

namespace SeerrFixarr.App.Extensions;

public static class ConfigureWebHostBuilderExtensions
{
    public static void ConfigurePort(this ConfigureWebHostBuilder builder)
    {
        var port = Environment.GetEnvironmentVariable("PORT") ?? "8080";
        var url = $"http://+:{port}";
        builder.UseUrls(url);
        Log.Information("Application will run on {Url}", url);
    }
    
    public static void ConfigureCulture(this ConfigureWebHostBuilder _)
    {
        var culture = Environment.GetEnvironmentVariable("CULTURE");
        var cultureInfo = string.IsNullOrEmpty(culture)
            ? CultureInfo.InvariantCulture
            : new CultureInfo(culture);
        CultureInfo.DefaultThreadCurrentCulture = cultureInfo;
        CultureInfo.DefaultThreadCurrentUICulture = cultureInfo;
        Log.Information("Application culture set to {Culture}", cultureInfo.DisplayName);
    }
}