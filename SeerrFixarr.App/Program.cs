using SeerrFixarr.Api;
using SeerrFixarr.App;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddEndpointsApiExplorer();


builder.Services.AddOpenApi();
builder.Services.AddHttpClient();

builder.AddLogging();
builder.AddSettings();

builder.Services.AddArrApis();
builder.Services.AddSeerrFixarrServices();

var app = builder.Build();

_ = app.Environment.IsDevelopment() switch 
{
    true => app.UseDevelopment(),
    false => app.UseProduction()
};

app.MapOverseerrWebhook();

await app.RunAsync();