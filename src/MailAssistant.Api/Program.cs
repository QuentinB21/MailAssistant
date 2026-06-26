using System.Text.Json.Serialization;
using MailAssistant.Api;
using MailAssistant.Api.Authentication;
using MailAssistant.Application;
using MailAssistant.Infrastructure;
using MailAssistant.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

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
var frontendOrigins = builder.Configuration
    .GetSection("Frontend:Origins")
    .Get<string[]>()
    ?? [builder.Configuration["Frontend:Origin"] ?? "http://localhost:5173"];
builder.Services.AddCors(options =>
    options.AddDefaultPolicy(policy =>
        policy
            .WithOrigins(frontendOrigins)
            .AllowAnyHeader()
            .AllowAnyMethod()));

var app = builder.Build();

await using (var scope = app.Services.CreateAsyncScope())
{
    var database = scope.ServiceProvider.GetRequiredService<MailAssistantDbContext>();
    await database.Database.MigrateAsync();
}

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
app.MapGmailEndpoints();

app.Run();

public partial class Program;
