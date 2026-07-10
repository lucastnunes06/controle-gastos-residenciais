using LarFinance.Api.Models;
using LarFinance.Api.Persistence;
using Microsoft.AspNetCore.Mvc;

namespace LarFinance.Api.Controllers;

[ApiController]
[Route("api/transactions")]
public sealed class TransactionsController(IHouseholdRepository repository) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType<IReadOnlyList<Transaction>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<Transaction>>> GetAsync() =>
        Ok(await repository.GetTransactionsAsync());

    [HttpPost]
    [ProducesResponseType<Transaction>(StatusCodes.Status201Created)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status422UnprocessableEntity)]
    public async Task<ActionResult<Transaction>> PostAsync(CreateTransactionRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Description))
        {
            ModelState.AddModelError(nameof(request.Description), "A descrição não pode conter apenas espaços.");
            return ValidationProblem(ModelState);
        }

        var result = await repository.AddTransactionAsync(request);

        if (result.Value is null)
        {
            return Problem(
                statusCode: StatusCodes.Status422UnprocessableEntity,
                title: "Transação não permitida",
                detail: result.Error);
        }

        return Created($"/api/transactions/{result.Value.Id}", result.Value);
    }
}
