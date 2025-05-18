using Microsoft.AspNetCore.Mvc;
using SeerrFixarr.Api;
using SeerrFixarr.App;
using SeerrFixarr.App.Runners;
using SeerrFixarr.App.Runners.Radarr;
using SeerrFixarr.App.Runners.Sonarr;
using SeerrFixarr.App.Runners.Webhook;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddOpenApi();
builder.Services.AddHttpClient();

builder.AddSeerrFixerrSettings();
builder.Services.AddSeerFixarrApi();
builder.Services.AddScoped<ITimeOutProvider, TimeOutProvider>();
builder.Services.AddScoped<WebhookRunner>();
builder.Services.AddScoped<RadarrRunner>();
builder.Services.AddScoped<SonarrRunner>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi("/swagger/v1/swagger.json");
    app.UseSwaggerUI(o => o.EnableTryItOutByDefault());
}

app.MapPost("/webhook", async ([FromServices] WebhookRunner runner, [FromBody] WebhookIssueRoot body) =>
{
    await runner.RunAsync(body);
    Results.Ok();
});

await app.RunAsync();