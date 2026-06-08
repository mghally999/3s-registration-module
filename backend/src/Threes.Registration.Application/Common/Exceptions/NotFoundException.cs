namespace Threes.Registration.Application.Common.Exceptions;

// thrown when a requested resource does not exist, e.g. GET /registrations/{id}
// for an id that was never created. the api maps it to 404.
public sealed class NotFoundException : Exception
{
    public NotFoundException(string resource, object key)
        : base($"{resource} with key '{key}' was not found.")
    {
    }
}
