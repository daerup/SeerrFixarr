using SeerrFixarr.Api;
using SeerrFixarr.App.Extensions;
using SeerrFixarr.WireMock;

var builder = WebApplication.CreateBuilder(args);

builder.AddDataProtection();
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
builder.Services.AddBlazor();

if (await builder.IsRunningInMockedMode())
{
    builder.Services.AddWireMockServices();
}

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
