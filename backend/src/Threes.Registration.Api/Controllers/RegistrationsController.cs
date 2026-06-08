using MediatR;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using Swashbuckle.AspNetCore.Filters;
using Threes.Registration.Api.Swagger.Examples;
using Threes.Registration.Application.Common.Models;
using Threes.Registration.Application.Registrations.Commands.CreateRegistration;
using Threes.Registration.Application.Registrations.Queries.GetRegistrationById;
using Threes.Registration.Application.Registrations.Queries.SearchRegistrations;

namespace Threes.Registration.Api.Controllers;

// the registration endpoints. the controller is intentionally thin: it hands
// the request to mediatr and shapes the http result. validation, conflicts and
// persistence all happen behind the command/query.
[ApiController]
[Route("api/registrations")]
[Produces("application/json")]
public sealed class RegistrationsController : ControllerBase
{
    private readonly ISender _sender;

    public RegistrationsController(ISender sender) => _sender = sender;

    // POST /api/registrations
    [HttpPost]
    [SwaggerOperation(Summary = "Create a registration", Description = "Validates and stores a new registration with one to five addresses.")]
    [ProducesResponseType(typeof(CreateRegistrationResult), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    [SwaggerRequestExample(typeof(CreateRegistrationCommand), typeof(CreateRegistrationRequestExample))]
    public async Task<IActionResult> Create(
        [FromBody] CreateRegistrationCommand command,
        CancellationToken cancellationToken)
    {
        var result = await _sender.Send(command, cancellationToken);

        // 201 with the created id in the body and a Location header pointing at
        // the details endpoint.
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    // GET /api/registrations?page=&pageSize=&search=
    [HttpGet]
    [SwaggerOperation(Summary = "List/search registrations", Description = "Paged list of registrations, optionally filtered by email or mobile number.")]
    [ProducesResponseType(typeof(PagedResult<RegistrationSummaryDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResult<RegistrationSummaryDto>>> Search(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? search = null,
        CancellationToken cancellationToken = default)
    {
        var result = await _sender.Send(new SearchRegistrationsQuery(page, pageSize, search), cancellationToken);
        return Ok(result);
    }

    // GET /api/registrations/{id}
    [HttpGet("{id:guid}", Name = nameof(GetById))]
    [SwaggerOperation(Summary = "Get a registration by id")]
    [ProducesResponseType(typeof(RegistrationDetailsDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<RegistrationDetailsDto>> GetById(
        Guid id,
        CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new GetRegistrationByIdQuery(id), cancellationToken);
        return Ok(result);
    }
}
