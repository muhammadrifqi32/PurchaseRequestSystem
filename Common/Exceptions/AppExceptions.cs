namespace PurchaseRequestSystem.Common.Exceptions;

/// <summary>
/// Base type for all application-thrown exceptions that map to a known HTTP status.
/// </summary>
public abstract class AppException : Exception
{
    public abstract int StatusCode { get; }

    protected AppException(string message) : base(message) { }
}

/// <summary>404 - resource does not exist.</summary>
public class NotFoundException : AppException
{
    public override int StatusCode => 404;
    public NotFoundException(string message = "Data not found") : base(message) { }

    public NotFoundException(string entity, object key)
        : base($"{entity} with identifier '{key}' was not found.") { }
}

/// <summary>400 - malformed or semantically invalid request.</summary>
public class BadRequestException : AppException
{
    public override int StatusCode => 400;
    public BadRequestException(string message = "Bad request") : base(message) { }
}

/// <summary>400 - one or more field validation rules failed.</summary>
public class ValidationException : AppException
{
    public override int StatusCode => 400;
    public IReadOnlyList<string> Errors { get; }

    public ValidationException(IEnumerable<string> errors, string message = "Validation failed")
        : base(message)
    {
        Errors = errors.ToList();
    }

    public ValidationException(string error, string message = "Validation failed")
        : this(new[] { error }, message) { }
}

/// <summary>401 - caller is not authenticated / token invalid.</summary>
public class AppUnauthorizedException : AppException
{
    public override int StatusCode => 401;
    public AppUnauthorizedException(string message = "Unauthorized") : base(message) { }
}

/// <summary>403 - authenticated but not allowed.</summary>
public class ForbiddenException : AppException
{
    public override int StatusCode => 403;
    public ForbiddenException(string message = "You do not have permission to perform this action") : base(message) { }
}

/// <summary>400 - a domain/business invariant was violated.</summary>
public class BusinessRuleException : AppException
{
    public override int StatusCode => 400;
    public BusinessRuleException(string message) : base(message) { }
}
