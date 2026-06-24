using MailAssistant.Application.Abstractions;
using MailAssistant.Domain.Identity;
using MailAssistant.Domain.Organizations;
using MailAssistant.Domain.Projects;
using Microsoft.EntityFrameworkCore;

namespace MailAssistant.Infrastructure.Persistence;

public sealed class MailAssistantDbContext(DbContextOptions<MailAssistantDbContext> options)
    : DbContext(options), IUnitOfWork
{
    public DbSet<Organization> Organizations => Set<Organization>();

    public DbSet<ApplicationUser> Users => Set<ApplicationUser>();

    public DbSet<OrganizationMembership> OrganizationMemberships =>
        Set<OrganizationMembership>();

    public DbSet<OrganizationSettings> OrganizationSettings => Set<OrganizationSettings>();

    public DbSet<Project> Projects => Set<Project>();

    public DbSet<ProjectAlias> ProjectAliases => Set<ProjectAlias>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(MailAssistantDbContext).Assembly);
    }
}
