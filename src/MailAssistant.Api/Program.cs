using MailAssistant.Application;
using MailAssistant.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddApplication()
    .AddInfrastructure(builder.Configuration)
    .AddHealthChecks();

var app = builder.Build();

app.MapGet("/", () => Results.Ok(new
{
    service = "MailAssistant.Api",
    status = "running",
}));

app.MapHealthChecks("/health");

app.Run();

public partial class Program;

