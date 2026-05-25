using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace Fashia.Domain.Common;

public static partial class SlugGenerator
{
    public static string Generate(string value)
    {
        var normalized = value
            .ToLowerInvariant()
            .Normalize(NormalizationForm.FormD);

        var builder = new StringBuilder();

        foreach (var character in normalized)
        {
            var unicodeCategory = CharUnicodeInfo.GetUnicodeCategory(character);

            if (unicodeCategory != UnicodeCategory.NonSpacingMark)
            {
                builder.Append(character);
            }
        }

        var slug = builder
            .ToString()
            .Normalize(NormalizationForm.FormC);

        slug = NonSlugCharactersRegex().Replace(slug, "");
        slug = WhiteSpaceRegex().Replace(slug, "-").Trim('-');
        slug = MultipleDashesRegex().Replace(slug, "-");

        return slug;
    }

    [GeneratedRegex(@"[^a-z0-9\s-]")]
    private static partial Regex NonSlugCharactersRegex();

    [GeneratedRegex(@"\s+")]
    private static partial Regex WhiteSpaceRegex();

    [GeneratedRegex(@"-+")]
    private static partial Regex MultipleDashesRegex();
}