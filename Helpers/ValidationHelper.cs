using PurchaseRequestSystem.Common.Exceptions;

namespace PurchaseRequestSystem.Helpers;

public static class ValidationHelper
{
    public static void ThrowIfAny(IEnumerable<string> errors)
    {
        var list = errors.Where(error => !string.IsNullOrWhiteSpace(error)).ToList();
        if (list.Count > 0)
            throw new ValidationException(list);
    }

    public static void EnsurePositive(decimal value, string fieldName, ICollection<string> errors)
    {
        if (value <= 0)
            errors.Add($"{fieldName} must be greater than 0.");
    }

    public static void EnsureDate(DateTime value, string fieldName, ICollection<string> errors)
    {
        if (value == default)
            errors.Add($"{fieldName} is required.");
    }
}
