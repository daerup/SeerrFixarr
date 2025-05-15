using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using SeerrFixarr.Api;
using SeerrFixarr.App;
using SeerrFixarr.App.Settings;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddOpenApi();
builder.Services.AddHttpClient();

builder.AddSeerrFixerrSettings();

builder.Services.AddOverseerrApi();
builder.Services.AddSonarrApi();
builder.Services.AddRadarrApi();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi("/swagger/v1/swagger.json");
    app.UseSwaggerUI(o => o.EnableTryItOutByDefault());
}

app.MapPost("/webhook", async (RadarrApi api) =>
{
    await api.GetAllMovies();
    await Task.CompletedTask;
});

await app.RunAsync();