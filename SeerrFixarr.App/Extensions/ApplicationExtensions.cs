using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Mvc;
using SeerrFixarr.Api.Overseerr;
using SeerrFixarr.App.Runners.Webhook;
using Serilog;

namespace SeerrFixarr.App.Extensions;

public static class ApplicationExtensions
{
    public static WebApplication UseDevelopment(this WebApplication app)
    {
        app.MapOpenApi("/swagger/v1/swagger.json");
        app.UseSwaggerUI(o => o.EnableTryItOutByDefault());
        app.UseDeveloperExceptionPage();
        return app;
    }

    public static WebApplication UseProduction(this WebApplication app)
    {
        app.UseExceptionHandler("/Error");
        return app;
    }
    
    public static void UseBlazor(this WebApplication app)
    {
        app.UseStaticFiles();
        app.UseRouting();
        app.UseAntiforgery();
        app.MapRazorComponents<App>().AddInteractiveServerRenderMode();
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
        app.MapGet("GetTestToken", ([FromServices] TokenCreator tokenCreator, int id, int MediaType) =>
        {
            var token = tokenCreator.CreateToken(id, (MediaType)MediaType, TimeSpan.FromMinutes(15));
            return Results.Ok(token);
        });
        
        return app.MapPost("/webhook", async ([FromServices] WebhookRunner runner, [FromBody] WebhookIssueRoot body) =>
        {
            Log.Information("Webhook received for Issue #{IssueId} reported by {username}", body.IssueId, body.ReportedByUsername);
            await runner.RunAsync(body);
            return Results.Ok();
        });
    }
}