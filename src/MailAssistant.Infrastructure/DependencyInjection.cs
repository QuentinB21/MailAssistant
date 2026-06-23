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
        return services;
    }
}
