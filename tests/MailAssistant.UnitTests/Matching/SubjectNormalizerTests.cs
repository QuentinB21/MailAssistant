using MailAssistant.Domain.Matching;
using Xunit;

namespace MailAssistant.UnitTests.Matching;

public sealed class SubjectNormalizerTests
{
    private readonly SubjectNormalizer _normalizer = new();

    [Theory]
    [InlineData("RE: Projet Émeraude", "projet emeraude")]
    [InlineData("FWD:   RE: [Projet-A]   Compte rendu", "projet a compte rendu")]
    [InlineData("TR:FW:Déploiement CRM", "deploiement crm")]
    [InlineData("  Espaces    multiples  ", "espaces multiples")]
    [InlineData("Facture #42 / Client", "facture 42 client")]
    [InlineData("", "")]
    public void NormalizeProducesExpectedValue(string subject, string expected)
    {
        Assert.Equal(expected, _normalizer.Normalize(subject));
    }
}
