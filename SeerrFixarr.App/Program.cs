using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SeerrFixarr.Api;
using SeerrFixarr.Api.Overseerr;
using SeerrFixarr.Api.Radarr;
using SeerrFixarr.Api.Sonarr;
using SeerrFixarr.App;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddOpenApi();
builder.Services.AddHttpClient();

builder.AddSeerrFixerrSettings();
builder.Services.AddSeerFixarrApi();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi("/swagger/v1/swagger.json");
    app.UseSwaggerUI(o => o.EnableTryItOutByDefault());
}

app.MapPost("/webhook",
    async (IOverseerrApi overseerr, IRadarrApi radarr, ISonarrApi sonarr) =>
    {
        var issue = await overseerr.GetIssue(1);
        var someMessage = $"Original comment was '{issue.Comments.First().Message}'";
        await overseerr.PostIssueComment(issue.Id, someMessage);
        
        return issue;
    });

await app.RunAsync();