using MailAssistant.Application.Identity;
using MailAssistant.Application.MailAccounts;
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
        services.AddScoped<OrganizationSettingsService>();
        services.AddScoped<ProjectService>();
        services.AddScoped<SubjectMatchingService>();
        services.AddScoped<CurrentUserService>();
        services.AddScoped<OrganizationAccessService>();
        services.AddScoped<MembershipService>();
        services.AddScoped<GmailAccountService>();

        return services;
    }
}
