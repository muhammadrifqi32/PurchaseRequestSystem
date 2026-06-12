using System.Text.RegularExpressions;

namespace PurchaseRequestSystem.Helpers;

public static class DocumentNumberGenerator
{
    public static string Generate(string prefix, DateTime documentDate, IEnumerable<string> existingNumbers)
    {
        var period = documentDate.ToString("yyyyMM");
        var documentPrefix = $"{prefix}-{period}-";

        var maxSequence = existingNumbers
            .Where(number => !string.IsNullOrWhiteSpace(number) && number.StartsWith(documentPrefix, StringComparison.OrdinalIgnoreCase))
            .Select(number => TryGetSequence(number, documentPrefix))
            .DefaultIfEmpty(0)
            .Max();

        return $"{documentPrefix}{maxSequence + 1:0000}";
    }

    public static string GetPeriodPrefix(string prefix, DateTime documentDate)
        => $"{prefix}-{documentDate:yyyyMM}-";

    private static int TryGetSequence(string documentNumber, string documentPrefix)
    {
        var value = documentNumber[documentPrefix.Length..];
        return Regex.IsMatch(value, "^\\d+$") && int.TryParse(value, out var sequence)
            ? sequence
            : 0;
    }
}
