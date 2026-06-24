using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace MailAssistant.Infrastructure.Persistence;

public sealed class MailAssistantDbContextFactory
    : IDesignTimeDbContextFactory<MailAssistantDbContext>
{
    private const string DefaultConnectionString =
        "Host=localhost;Port=5432;Database=mailassistant;Username=mailassistant;Password=local-development-only";

    public MailAssistantDbContext CreateDbContext(string[] args)
    {
        var connectionString =
            Environment.GetEnvironmentVariable("ConnectionStrings__Database")
            ?? DefaultConnectionString;

        var options = new DbContextOptionsBuilder<MailAssistantDbContext>()
            .UseNpgsql(connectionString)
            .Options;

        return new MailAssistantDbContext(options);
    }
}
