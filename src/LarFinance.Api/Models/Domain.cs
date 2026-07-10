using System.ComponentModel.DataAnnotations;

namespace LarFinance.Api.Models;

/// <summary>Representa uma pessoa responsável por movimentações financeiras da residência.</summary>
public sealed record Person(Guid Id, string Name, int Age);

/// <summary>Tipos aceitos pelo domínio. A API serializa estes valores como texto.</summary>
public enum TransactionType
{
    Expense,
    Income
}

/// <summary>Movimentação financeira vinculada obrigatoriamente a uma pessoa.</summary>
public sealed record Transaction(
    Guid Id,
    string Description,
    decimal Amount,
    TransactionType Type,
    Guid PersonId,
    DateTimeOffset CreatedAt);

public sealed class CreatePersonRequest
{
    [Required(ErrorMessage = "O nome é obrigatório.")]
    [StringLength(120, MinimumLength = 2, ErrorMessage = "O nome deve ter entre 2 e 120 caracteres.")]
    public string Name { get; init; } = string.Empty;

    [Range(0, 130, ErrorMessage = "A idade deve estar entre 0 e 130 anos.")]
    public int Age { get; init; }
}

public sealed class CreateTransactionRequest
{
    [Required(ErrorMessage = "A descrição é obrigatória.")]
    [StringLength(160, MinimumLength = 2, ErrorMessage = "A descrição deve ter entre 2 e 160 caracteres.")]
    public string Description { get; init; } = string.Empty;

    [PositiveAmount]
    public decimal Amount { get; init; }

    [EnumDataType(typeof(TransactionType), ErrorMessage = "O tipo deve ser Expense ou Income.")]
    public TransactionType Type { get; init; }

    public Guid PersonId { get; init; }
}


/// <summary>
/// Valida valores monetários sem depender da cultura ativa do processo.
/// RangeAttribute com limites textuais pode interpretar ponto e vírgula de formas diferentes.
/// </summary>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter)]
public sealed class PositiveAmountAttribute : ValidationAttribute
{
    private const decimal MaximumAmount = 999_999_999_999m;

    public PositiveAmountAttribute()
        : base("O valor deve ser maior que zero e menor que um trilhão.")
    {
    }

    public override bool IsValid(object? value) =>
        value is decimal amount && amount > 0 && amount <= MaximumAmount;
}
public sealed record PersonTotals(
    Guid PersonId,
    string Name,
    decimal Income,
    decimal Expenses,
    decimal Balance);

public sealed record GeneralTotals(decimal Income, decimal Expenses, decimal Balance);

public sealed record TotalsResponse(
    IReadOnlyList<PersonTotals> People,
    GeneralTotals General);
