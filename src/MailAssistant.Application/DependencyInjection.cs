using MailAssistant.Application.Organizations;
using MailAssistant.Application.Projects;
using MailAssistant.Domain.Matching;
using Microsoft.Extensions.DependencyInjection;

namespace MailAssistant.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddSingleton(TimeProvider.System);
        services.AddSingleton<ISubjectNormalizer, SubjectNormalizer>();
        services.AddSingleton<IMatchingStrategy, ContainsSubjectMatchingStrategy>();
        services.AddSingleton<IConflictResolutionPolicy, ConflictResolutionPolicy>();
        services.AddScoped<OrganizationService>();
        services.AddScoped<ProjectService>();

        return services;
    }
}
