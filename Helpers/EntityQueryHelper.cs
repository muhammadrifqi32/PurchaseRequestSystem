using Microsoft.EntityFrameworkCore;

namespace PurchaseRequestSystem.Helpers;

public static class EntityQueryHelper
{
    public static IQueryable<T> WhereNotDeleted<T>(this IQueryable<T> query) where T : class
    {
        return HasBooleanProperty<T>("IsDeleted")
            ? query.Where(entity => !EF.Property<bool>(entity, "IsDeleted"))
            : query;
    }

    public static bool HasBooleanProperty<T>(string propertyName) where T : class
    {
        var property = typeof(T).GetProperty(propertyName);
        return property is not null && property.PropertyType == typeof(bool);
    }

    public static void SetSoftDeleted<T>(T entity) where T : class
    {
        var property = typeof(T).GetProperty("IsDeleted");
        if (property is not null && property.PropertyType == typeof(bool))
        {
            property.SetValue(entity, true);
        }
    }

    public static bool SupportsSoftDelete<T>() where T : class
        => HasBooleanProperty<T>("IsDeleted");
}
