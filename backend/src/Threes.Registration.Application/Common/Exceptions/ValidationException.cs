namespace Threes.Registration.Application.Common.Exceptions;

// carries one or more field-level validation failures. the validation pipeline
// throws this; the api turns it into a 400 problem-details with the errors
// grouped by field name.
public sealed class ValidationException : Exception
{
    public ValidationException(IDictionary<string, string[]> errors)
        : base("One or more validation errors occurred.")
    {
        Errors = errors;
    }

    public IDictionary<string, string[]> Errors { get; }
}
