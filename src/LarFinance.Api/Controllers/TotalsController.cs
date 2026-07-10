using LarFinance.Api.Models;
using LarFinance.Api.Persistence;
using Microsoft.AspNetCore.Mvc;

namespace LarFinance.Api.Controllers;

[ApiController]
[Route("api/totals")]
public sealed class TotalsController(IHouseholdRepository repository) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType<TotalsResponse>(StatusCodes.Status200OK)]
    public async Task<ActionResult<TotalsResponse>> GetAsync() =>
        Ok(await repository.GetTotalsAsync());
}
