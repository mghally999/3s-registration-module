namespace Threes.Registration.Domain.Common;

// thrown when someone tries to push the domain into an invalid state, for
// example an empty last name or a sixth address. the application layer's
// fluentvalidation should normally catch bad input first and turn it into a
// nice 400, so a DomainException reaching the api usually means a real bug or
// a request path that skipped validation. we still throw it as a last line of
// defence so the model can never be wrong.
public class DomainException : Exception
{
    public DomainException(string message) : base(message)
    {
    }

    // a short, stable code so callers/tests can branch without string matching.
    public DomainException(string code, string message) : base(message)
    {
        Code = code;
    }

    public string? Code { get; }
}
