using MailAssistant.Application.Abstractions;
using MailAssistant.Infrastructure.Gmail;
using MailAssistant.Infrastructure.Persistence;
using MailAssistant.Infrastructure.Security;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace MailAssistant.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(configuration);

        var connectionString = configuration.GetConnectionString("Database")
            ?? throw new InvalidOperationException(
                "Connection string 'Database' is not configured.");

        services.AddDbContext<MailAssistantDbContext>(options =>
            options.UseNpgsql(connectionString));
        var dataProtection = services
            .AddDataProtection()
            .SetApplicationName("MailAssistant");
        var dataProtectionKeysPath = configuration["DataProtection:KeysPath"];
        if (!string.IsNullOrWhiteSpace(dataProtectionKeysPath))
        {
            dataProtection.PersistKeysToFileSystem(
                new DirectoryInfo(dataProtectionKeysPath));
        }
        services.AddScoped<IOrganizationRepository, OrganizationRepository>();
        services.AddScoped<IOrganizationSettingsRepository, OrganizationSettingsRepository>();
        services.AddScoped<IProjectRepository, ProjectRepository>();
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IMembershipRepository, MembershipRepository>();
        services.AddScoped<IMailAccountRepository, MailAccountRepository>();
        services.AddScoped<IOAuthCredentialRepository, OAuthCredentialRepository>();
        services.AddScoped<
            IProviderClassificationTargetRepository,
            ProviderClassificationTargetRepository>();
        services.AddSingleton<IOAuthTokenProtector, DataProtectionOAuthTokenProtector>();
        services.AddSingleton<
            IGmailAuthorizationStateProtector,
            DataProtectionGmailAuthorizationStateProtector>();
        services.AddHttpClient<IGmailGateway, GoogleGmailGateway>(client =>
        {
            client.Timeout = TimeSpan.FromSeconds(30);
        });
        services.AddScoped<IUnitOfWork>(provider =>
            provider.GetRequiredService<MailAssistantDbContext>());

        return services;
    }
}
