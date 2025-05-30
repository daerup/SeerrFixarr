using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Mvc;
using SeerrFixarr.App.Runners.Webhook;
using Serilog;

namespace SeerrFixarr.App.Extensions;

public static class ApplicationExtensions
{
    public static WebApplication UseDevelopment(this WebApplication app)
    {
        app.MapOpenApi("/swagger/v1/swagger.json");
        app.UseSwaggerUI(o => o.EnableTryItOutByDefault());
        return app;
    }

    public static WebApplication UseProduction(this WebApplication app)
    {
        return app;
    }

    public static IEndpointConventionBuilder MapHealthcheck(this WebApplication app)
    {
        return app.MapHealthChecks("/healthz", new HealthCheckOptions
        {
            ResponseWriter = async (context, report) =>
            {
                context.Response.ContentType = "application/json";
                await context.Response.WriteAsJsonAsync(new
                {
                    status = report.Status.ToString(),
                    environment = app.Environment.EnvironmentName,
                    timestamp = DateTime.Now.ToLocalTime(),
                });
            }
        });
    }

    public static RouteHandlerBuilder MapOverseerrWebhook(this WebApplication app)
    {
        return app.MapPost("/webhook", async ([FromServices] WebhookRunner runner, [FromBody] WebhookIssueRoot body) =>
        {
            Log.Information("Webhook received for Issue #{IssueId} reported by {username}", body.IssueId, body.ReportedByUsername);
            await runner.RunAsync(body);
            return Results.Ok();
        });
    }
}