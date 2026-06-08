namespace Threes.Registration.Application.Common.Exceptions;

// thrown when a request collides with data that already exists, specifically a
// duplicate email or mobile number. the api maps it to 409 Conflict. the
// optional field name lets the frontend highlight the right input.
public sealed class ConflictException : Exception
{
    public ConflictException(string message, string? field = null) : base(message)
    {
        Field = field;
    }

    public string? Field { get; }
}
