using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace MailAssistant.Domain.Matching;

public sealed partial class SubjectNormalizer : ISubjectNormalizer
{
    public string Normalize(string subject)
    {
        ArgumentNullException.ThrowIfNull(subject);

        var withoutPrefixes = ReplyPrefixRegex().Replace(subject.Trim(), string.Empty);
        var decomposed = withoutPrefixes.Normalize(NormalizationForm.FormD);
        var builder = new StringBuilder(decomposed.Length);

        foreach (var character in decomposed)
        {
            var category = CharUnicodeInfo.GetUnicodeCategory(character);
            if (category == UnicodeCategory.NonSpacingMark)
            {
                continue;
            }

            builder.Append(char.IsLetterOrDigit(character) ? char.ToLowerInvariant(character) : ' ');
        }

        return MultipleWhitespaceRegex()
            .Replace(builder.ToString(), " ")
            .Trim();
    }

    [GeneratedRegex(
        @"^(?:(?:re|fw|fwd|tr)\s*:\s*)+",
        RegexOptions.IgnoreCase | RegexOptions.CultureInvariant)]
    private static partial Regex ReplyPrefixRegex();

    [GeneratedRegex(@"\s+", RegexOptions.CultureInvariant)]
    private static partial Regex MultipleWhitespaceRegex();
}
