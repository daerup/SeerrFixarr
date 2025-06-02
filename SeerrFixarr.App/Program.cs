using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SeerrFixarr.Api;
using SeerrFixarr.App.Extensions;
using Sysinfocus.AspNetCore.Components;

var builder = WebApplication.CreateBuilder(args);
builder.AddSettings();
builder.AddLogging();

builder.WebHost.ConfigurePort();
builder.WebHost.ConfigureCulture();

builder.Services.AddHealthChecks();
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddOpenApi();
builder.Services.AddHttpClient();

builder.Services.AddArrApis();
builder.Services.AddSeerrFixarrServices();

builder.Services
    .AddSysinfocus(jsCssFromCDN: false)
    .AddRazorComponents()
    .AddInteractiveServerComponents();

var app = builder.Build();
_ = app.Environment.IsDevelopment() switch 
{
    true => app.UseDevelopment(),
    false => app.UseProduction()
};

app.UseBlazor();

app.MapHealthcheck();
app.MapOverseerrWebhook();

await app.RunAsync();