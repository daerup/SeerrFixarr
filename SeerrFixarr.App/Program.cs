using Microsoft.AspNetCore.Mvc;
using SeerrFixarr.Api;
using SeerrFixarr.App;
using SeerrFixarr.App.Runners.Webhook;
using Serilog;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddOpenApi();
builder.Services.AddHttpClient();

builder.Services.AddSerilog(c => c.ReadFrom.Configuration(builder.Configuration).Enrich.FromLogContext().WriteTo.Console());
builder.AddSeerrFixerrSettings();
builder.Services.AddSeerFixarrApi();
builder.Services.RegisterServices();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi("/swagger/v1/swagger.json");
    app.UseSwaggerUI(o => o.EnableTryItOutByDefault());
}

app.MapPost("/webhook", async ([FromServices] WebhookRunner runner, [FromBody] WebhookIssueRoot body) =>
{
    Log.Information("Webhook received for Issue #{IssueId} reported by {username}", body.IssueId, body.ReportedByUsername);
    await runner.RunAsync(body);
    return Results.Ok();
});

await app.RunAsync();