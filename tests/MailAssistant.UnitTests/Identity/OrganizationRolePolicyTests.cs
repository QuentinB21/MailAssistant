using MailAssistant.Domain.Identity;
using Xunit;

namespace MailAssistant.UnitTests.Identity;

public sealed class OrganizationRolePolicyTests
{
    [Theory]
    [InlineData(OrganizationRole.Owner, OrganizationRole.Owner, true)]
    [InlineData(OrganizationRole.Owner, OrganizationRole.Admin, true)]
    [InlineData(OrganizationRole.Admin, OrganizationRole.Member, true)]
    [InlineData(OrganizationRole.Member, OrganizationRole.Admin, false)]
    [InlineData(OrganizationRole.Admin, OrganizationRole.Owner, false)]
    public void HasAtLeastAppliesExplicitHierarchy(
        OrganizationRole actual,
        OrganizationRole required,
        bool expected)
    {
        Assert.Equal(expected, actual.HasAtLeast(required));
    }
}
