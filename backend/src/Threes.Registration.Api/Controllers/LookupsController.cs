using MediatR;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using Threes.Registration.Application.Lookups.Contracts;
using Threes.Registration.Application.Lookups.Queries.GetCities;
using Threes.Registration.Application.Lookups.Queries.GetGovernorates;

namespace Threes.Registration.Api.Controllers;

// the lookup endpoints that feed the two dependent dropdowns on the form.
[ApiController]
[Route("api/lookups")]
[Produces("application/json")]
public sealed class LookupsController : ControllerBase
{
    private readonly ISender _sender;

    public LookupsController(ISender sender) => _sender = sender;

    // GET /api/lookups/governorates
    [HttpGet("governorates")]
    [SwaggerOperation(Summary = "List active governorates", Description = "Returns active governorates sorted by name.")]
    [ProducesResponseType(typeof(IReadOnlyList<GovernorateDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<GovernorateDto>>> GetGovernorates(CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new GetGovernoratesQuery(), cancellationToken);
        return Ok(result);
    }

    // GET /api/lookups/cities?governorateId={id}
    [HttpGet("cities")]
    [SwaggerOperation(Summary = "List cities for a governorate", Description = "Returns the cities that belong to the given governorate, sorted by name.")]
    [ProducesResponseType(typeof(IReadOnlyList<CityDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<CityDto>>> GetCities(
        [FromQuery] int governorateId,
        CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new GetCitiesQuery(governorateId), cancellationToken);
        return Ok(result);
    }
}
