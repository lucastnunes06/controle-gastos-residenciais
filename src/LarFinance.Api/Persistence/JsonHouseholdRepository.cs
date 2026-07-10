using System.Text.Json;
using System.Text.Json.Serialization;
using LarFinance.Api.Models;

namespace LarFinance.Api.Persistence;

/// <summary>
/// Abstrai a persistência para que a API não dependa do formato de armazenamento.
/// Uma implementação com banco relacional pode substituir o JSON sem alterar os controllers.
/// </summary>
public interface IHouseholdRepository
{
    Task<IReadOnlyList<Person>> GetPeopleAsync();
    Task<Person> AddPersonAsync(string name, int age);
    Task<bool> DeletePersonAsync(Guid id);
    Task<IReadOnlyList<Transaction>> GetTransactionsAsync();
    Task<(Transaction? Value, string? Error)> AddTransactionAsync(CreateTransactionRequest request);
    Task<TotalsResponse> GetTotalsAsync();
}

internal sealed class Database
{
    public List<Person> People { get; init; } = [];
    public List<Transaction> Transactions { get; init; } = [];
}

/// <summary>
/// Repositório local persistente. As operações passam por um semáforo porque pessoas e
/// transações compartilham o mesmo arquivo e precisam permanecer consistentes entre si.
/// </summary>
public sealed class JsonHouseholdRepository : IHouseholdRepository
{
    private readonly string _path;
    private readonly SemaphoreSlim _gate = new(1, 1);
    private readonly JsonSerializerOptions _jsonOptions = new(JsonSerializerDefaults.Web)
    {
        WriteIndented = true,
        Converters = { new JsonStringEnumConverter() }
    };

    public JsonHouseholdRepository(IWebHostEnvironment environment, IConfiguration configuration)
    {
        _path = configuration["DataFile"]
            ?? Path.Combine(environment.ContentRootPath, "App_Data", "larfinance.json");
    }

    public Task<IReadOnlyList<Person>> GetPeopleAsync() =>
        ReadAsync<IReadOnlyList<Person>>(database => database.People
            .OrderBy(person => person.Name)
            .ToList());

    public Task<Person> AddPersonAsync(string name, int age) =>
        WriteAsync(database =>
        {
            var person = new Person(Guid.NewGuid(), name.Trim(), age);
            database.People.Add(person);
            return person;
        });

    public Task<bool> DeletePersonAsync(Guid id) =>
        WriteAsync(database =>
        {
            var personWasRemoved = database.People.RemoveAll(person => person.Id == id) > 0;

            if (personWasRemoved)
            {
                // Regra de exclusão em cascata: uma transação nunca fica sem uma pessoa válida.
                database.Transactions.RemoveAll(transaction => transaction.PersonId == id);
            }

            return personWasRemoved;
        });

    public Task<IReadOnlyList<Transaction>> GetTransactionsAsync() =>
        ReadAsync<IReadOnlyList<Transaction>>(database => database.Transactions
            .OrderByDescending(transaction => transaction.CreatedAt)
            .ToList());

    public Task<(Transaction? Value, string? Error)> AddTransactionAsync(CreateTransactionRequest request) =>
        WriteAsync(database =>
        {
            var person = database.People.SingleOrDefault(person => person.Id == request.PersonId);

            if (person is null)
            {
                return ((Transaction?)null, "A pessoa informada não existe.");
            }

            // A validação fica no servidor para que nenhum cliente consiga contornar a regra.
            if (person.Age < 18 && request.Type == TransactionType.Income)
            {
                return ((Transaction?)null, "Menores de 18 anos podem cadastrar apenas despesas.");
            }

            var transaction = new Transaction(
                Guid.NewGuid(),
                request.Description.Trim(),
                request.Amount,
                request.Type,
                request.PersonId,
                DateTimeOffset.UtcNow);

            database.Transactions.Add(transaction);
            return (transaction, (string?)null);
        });

    public Task<TotalsResponse> GetTotalsAsync() =>
        ReadAsync(database =>
        {
            // Totais são derivados das transações para evitar duplicidade e inconsistência de dados.
            var peopleTotals = database.People
                .OrderBy(person => person.Name)
                .Select(person => CalculatePersonTotals(person, database.Transactions))
                .ToList();

            var income = peopleTotals.Sum(person => person.Income);
            var expenses = peopleTotals.Sum(person => person.Expenses);

            return new TotalsResponse(
                peopleTotals,
                new GeneralTotals(income, expenses, income - expenses));
        });

    private static PersonTotals CalculatePersonTotals(Person person, IEnumerable<Transaction> transactions)
    {
        var personTransactions = transactions.Where(transaction => transaction.PersonId == person.Id);
        var income = personTransactions
            .Where(transaction => transaction.Type == TransactionType.Income)
            .Sum(transaction => transaction.Amount);
        var expenses = personTransactions
            .Where(transaction => transaction.Type == TransactionType.Expense)
            .Sum(transaction => transaction.Amount);

        return new PersonTotals(person.Id, person.Name, income, expenses, income - expenses);
    }

    private async Task<T> ReadAsync<T>(Func<Database, T> operation)
    {
        await _gate.WaitAsync();
        try
        {
            return operation(await LoadAsync());
        }
        finally
        {
            _gate.Release();
        }
    }

    private async Task<T> WriteAsync<T>(Func<Database, T> operation)
    {
        await _gate.WaitAsync();
        try
        {
            var database = await LoadAsync();
            var result = operation(database);
            await SaveAsync(database);
            return result;
        }
        finally
        {
            _gate.Release();
        }
    }

    private async Task<Database> LoadAsync()
    {
        if (!File.Exists(_path))
        {
            return new Database();
        }

        await using var stream = File.OpenRead(_path);
        return await JsonSerializer.DeserializeAsync<Database>(stream, _jsonOptions) ?? new Database();
    }

    private async Task SaveAsync(Database database)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(_path)!);
        var temporaryPath = _path + ".tmp";

        await using (var stream = File.Create(temporaryPath))
        {
            await JsonSerializer.SerializeAsync(stream, database, _jsonOptions);
        }

        // A troca atômica evita que uma interrupção deixe um JSON escrito parcialmente.
        File.Move(temporaryPath, _path, overwrite: true);
    }
}

