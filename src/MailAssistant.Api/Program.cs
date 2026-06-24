using System.Text.Json.Serialization;
using MailAssistant.Api;
using MailAssistant.Api.Authentication;
using MailAssistant.Application;
using MailAssistant.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddApplication()
    .AddInfrastructure(builder.Configuration)
    .AddHealthChecks();
builder.Services.AddMailAssistantAuthentication(builder.Configuration);
builder.Services.AddExceptionHandler<ApiExceptionHandler>();
builder.Services.AddProblemDetails();
builder.Services.ConfigureHttpJsonOptions(options =>
    options.SerializerOptions.Converters.Add(new JsonStringEnumConverter()));
builder.Services.AddCors(options =>
    options.AddDefaultPolicy(policy =>
        policy
            .WithOrigins(builder.Configuration["Frontend:Origin"] ?? "http://localhost:5173")
            .AllowAnyHeader()
            .AllowAnyMethod()));

var app = builder.Build();

app.UseExceptionHandler();
app.UseCors();
app.UseAuthentication();
app.UseAuthorization();

app.MapGet("/", () => Results.Ok(new
{
    service = "MailAssistant.Api",
    status = "running",
}));

app.MapHealthChecks("/health");
app.MapOrganizationEndpoints();
app.MapProjectEndpoints();
app.MapIdentityEndpoints();

app.Run();

public partial class Program;
