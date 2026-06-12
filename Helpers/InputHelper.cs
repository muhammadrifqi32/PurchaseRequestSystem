using PurchaseRequestSystem.Common.Exceptions;

namespace PurchaseRequestSystem.Helpers;

public static class InputHelper
{
    public static string Required(string? value, string fieldName)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ValidationException($"{fieldName} is required.");

        return value.Trim();
    }

    public static string NormalizeCode(string? value, string fieldName)
        => Required(value, fieldName).ToUpperInvariant();
}
