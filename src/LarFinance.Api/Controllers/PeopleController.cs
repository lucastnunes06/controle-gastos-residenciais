using LarFinance.Api.Models;
using LarFinance.Api.Persistence;
using Microsoft.AspNetCore.Mvc;

namespace LarFinance.Api.Controllers;

[ApiController]
[Route("api/people")]
public sealed class PeopleController(IHouseholdRepository repository) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType<IReadOnlyList<Person>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<Person>>> GetAsync() =>
        Ok(await repository.GetPeopleAsync());

    [HttpPost]
    [ProducesResponseType<Person>(StatusCodes.Status201Created)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<Person>> PostAsync(CreatePersonRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
        {
            ModelState.AddModelError(nameof(request.Name), "O nome não pode conter apenas espaços.");
            return ValidationProblem(ModelState);
        }

        var person = await repository.AddPersonAsync(request.Name, request.Age);
        return Created($"/api/people/{person.Id}", person);
    }

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteAsync(Guid id)
    {
        if (await repository.DeletePersonAsync(id))
        {
            return NoContent();
        }

        return Problem(
            statusCode: StatusCodes.Status404NotFound,
            title: "Pessoa não encontrada",
            detail: "Não existe uma pessoa com o identificador informado.");
    }
}
