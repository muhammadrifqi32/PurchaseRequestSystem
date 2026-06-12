namespace PurchaseRequestSystem.Helpers;

public static class AuditHelper
{
    public static void SetCreatedAudit<T>(T entity) where T : class
    {
        SetPropertyIfExists(entity, "CreatedAt", DateTime.UtcNow);
    }

    public static void SetUpdatedAudit<T>(T entity) where T : class
    {
        SetPropertyIfExists(entity, "UpdatedAt", DateTime.UtcNow);
    }

    private static void SetPropertyIfExists<T>(T entity, string propertyName, object value) where T : class
    {
        var property = typeof(T).GetProperty(propertyName);
        if (property is not null && property.CanWrite)
        {
            property.SetValue(entity, value);
        }
    }
}
