using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SeerrFixarr.Api;
using SeerrFixarr.App;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddOpenApi();
builder.Services.AddHttpClient();

builder.AddSeerrFixerrSettings();
builder.Services.AddSeerFixarrApi();
builder.Services.AddScoped<FileSizeFormatter>();
builder.Services.AddScoped<WebhookRunner>();
builder.Services.AddScoped<RadarrRunner>();
builder.Services.AddScoped<SonarrRunner>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi("/swagger/v1/swagger.json");
    app.UseSwaggerUI(o => o.EnableTryItOutByDefault());
}

app.MapPost("/webhook", async ([FromServices] WebhookRunner runner, [FromBody] dynamic body) =>
{
    await runner.RunAsync(body);
    Results.Ok();
});

await app.RunAsync();