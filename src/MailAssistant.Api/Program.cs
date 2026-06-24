using System.Text.Json.Serialization;
using MailAssistant.Api;
using MailAssistant.Application;
using MailAssistant.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddApplication()
    .AddInfrastructure(builder.Configuration)
    .AddHealthChecks();
builder.Services.AddExceptionHandler<ApiExceptionHandler>();
builder.Services.AddProblemDetails();
builder.Services.ConfigureHttpJsonOptions(options =>
    options.SerializerOptions.Converters.Add(new JsonStringEnumConverter()));

var app = builder.Build();

app.UseExceptionHandler();

app.MapGet("/", () => Results.Ok(new
{
    service = "MailAssistant.Api",
    status = "running",
}));

app.MapHealthChecks("/health");
app.MapOrganizationEndpoints();
app.MapProjectEndpoints();

app.Run();

public partial class Program;
