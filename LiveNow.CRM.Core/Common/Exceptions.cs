namespace LiveNow.CRM.Core.Common;

public sealed class NotFoundException : AppException
{
    public NotFoundException(string errorCode, string message)
        : base(errorCode, message, StatusCodes.ResourceNotFound)
    {
    }
}

public sealed class ConflictException : AppException
{
    public ConflictException(string errorCode, string message)
        : base(errorCode, message, StatusCodes.Conflict)
    {
    }
}

public sealed class ValidationException : AppException
{
    public ValidationException(string errorCode, string message)
        : base(errorCode, message, StatusCodes.UnprocessableEntity)
    {
    }
}

internal static class StatusCodes
{
    public const int ResourceNotFound = 404;
    public const int Conflict = 409;
    public const int UnprocessableEntity = 422;
}