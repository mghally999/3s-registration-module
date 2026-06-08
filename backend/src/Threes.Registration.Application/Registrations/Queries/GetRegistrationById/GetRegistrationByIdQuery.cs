using MediatR;

namespace Threes.Registration.Application.Registrations.Queries.GetRegistrationById;

// reads one registration by its id. returns the details dto, or the handler
// throws NotFoundException which the api renders as 404.
public sealed record GetRegistrationByIdQuery(Guid Id) : IRequest<RegistrationDetailsDto>;
