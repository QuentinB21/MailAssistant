namespace MailAssistant.Domain.Identity;

public static class OrganizationRolePolicy
{
    public static bool HasAtLeast(
        this OrganizationRole actualRole,
        OrganizationRole requiredRole)
    {
        return GetPermissionLevel(actualRole) >= GetPermissionLevel(requiredRole);
    }

    private static int GetPermissionLevel(OrganizationRole role)
    {
        return role switch
        {
            OrganizationRole.Member => 1,
            OrganizationRole.Admin => 2,
            OrganizationRole.Owner => 3,
            _ => throw new ArgumentOutOfRangeException(nameof(role), role, null),
        };
    }
}
