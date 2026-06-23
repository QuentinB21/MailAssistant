using MailAssistant.Domain;
using Xunit;

namespace MailAssistant.UnitTests;

public sealed class ProjectInfoTests
{
    [Fact]
    public void NameIsStable()
    {
        Assert.Equal("MailAssistant", ProjectInfo.Name);
    }
}
