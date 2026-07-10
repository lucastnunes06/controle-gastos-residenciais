using System.ComponentModel.DataAnnotations;
using LarFinance.Api.Models;
using LarFinance.Api.Persistence;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.FileProviders;

namespace LarFinance.Tests;

public sealed class HouseholdRepositoryTests : IDisposable
{
    private readonly string _temporaryDirectory;
    private readonly string _dataFile;
    private readonly JsonHouseholdRepository _repository;

    public HouseholdRepositoryTests()
    {
        _temporaryDirectory = Path.Combine(
            Path.GetTempPath(),
            "LarFinanceTests",
            Guid.NewGuid().ToString());

        Directory.CreateDirectory(_temporaryDirectory);

        _dataFile = Path.Combine(_temporaryDirectory, "larfinance-test.json");

        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["DataFile"] = _dataFile
            })
            .Build();

        var environment = new TestWebHostEnvironment
        {
            ContentRootPath = _temporaryDirectory
        };

        _repository = new JsonHouseholdRepository(environment, configuration);
    }

    [Fact]
    public async Task AddPersonAsync_WithValidData_ShouldCreatePerson()
    {
        var person = await _repository.AddPersonAsync("  Ana Souza  ", 30);

        Assert.NotEqual(Guid.Empty, person.Id);
        Assert.Equal("Ana Souza", person.Name);
        Assert.Equal(30, person.Age);

        var people = await _repository.GetPeopleAsync();

        Assert.Single(people);
        Assert.Equal(person.Id, people[0].Id);
    }

    [Fact]
    public async Task AddTransactionAsync_ShouldAllowExpenseForMinor()
    {
        var person = await _repository.AddPersonAsync("Pedro", 16);

        var result = await _repository.AddTransactionAsync(
            new CreateTransactionRequest
            {
                Description = "Material escolar",
                Amount = 120m,
                Type = TransactionType.Expense,
                PersonId = person.Id
            });

        Assert.NotNull(result.Value);
        Assert.Null(result.Error);
        Assert.Equal(TransactionType.Expense, result.Value.Type);
        Assert.Equal(120m, result.Value.Amount);
    }

    [Fact]
    public async Task AddTransactionAsync_ShouldRejectIncomeForMinor()
    {
        var person = await _repository.AddPersonAsync("Pedro", 16);

        var result = await _repository.AddTransactionAsync(
            new CreateTransactionRequest
            {
                Description = "Mesada",
                Amount = 100m,
                Type = TransactionType.Income,
                PersonId = person.Id
            });

        Assert.Null(result.Value);
        Assert.Equal(
            "Menores de 18 anos podem cadastrar apenas despesas.",
            result.Error);

        Assert.Empty(await _repository.GetTransactionsAsync());
    }

    [Fact]
    public async Task AddTransactionAsync_ShouldAllowIncomeAtExactly18YearsOld()
    {
        var person = await _repository.AddPersonAsync("Marina", 18);

        var result = await _repository.AddTransactionAsync(
            new CreateTransactionRequest
            {
                Description = "Pagamento",
                Amount = 500m,
                Type = TransactionType.Income,
                PersonId = person.Id
            });

        Assert.NotNull(result.Value);
        Assert.Null(result.Error);
        Assert.Equal(500m, result.Value.Amount);
    }

    [Fact]
    public async Task AddTransactionAsync_ShouldRejectUnknownPerson()
    {
        var result = await _repository.AddTransactionAsync(
            new CreateTransactionRequest
            {
                Description = "Compra",
                Amount = 50m,
                Type = TransactionType.Expense,
                PersonId = Guid.NewGuid()
            });

        Assert.Null(result.Value);
        Assert.Equal("A pessoa informada não existe.", result.Error);
        Assert.Empty(await _repository.GetTransactionsAsync());
    }

    [Fact]
    public async Task DeletePersonAsync_ShouldDeleteRelatedTransactions()
    {
        var person = await _repository.AddPersonAsync("Carlos", 35);

        await _repository.AddTransactionAsync(
            new CreateTransactionRequest
            {
                Description = "Salário",
                Amount = 2_500m,
                Type = TransactionType.Income,
                PersonId = person.Id
            });

        await _repository.AddTransactionAsync(
            new CreateTransactionRequest
            {
                Description = "Aluguel",
                Amount = 800m,
                Type = TransactionType.Expense,
                PersonId = person.Id
            });

        var wasDeleted = await _repository.DeletePersonAsync(person.Id);

        Assert.True(wasDeleted);
        Assert.Empty(await _repository.GetPeopleAsync());
        Assert.Empty(await _repository.GetTransactionsAsync());
    }

    [Fact]
    public async Task DeletePersonAsync_WithUnknownId_ShouldReturnFalse()
    {
        var wasDeleted = await _repository.DeletePersonAsync(Guid.NewGuid());

        Assert.False(wasDeleted);
    }

    [Fact]
    public async Task GetTotalsAsync_ShouldCalculateIndividualAndGeneralTotals()
    {
        var adult = await _repository.AddPersonAsync("Ana", 30);
        var minor = await _repository.AddPersonAsync("Pedro", 16);

        await _repository.AddTransactionAsync(
            new CreateTransactionRequest
            {
                Description = "Salário",
                Amount = 1_000m,
                Type = TransactionType.Income,
                PersonId = adult.Id
            });

        await _repository.AddTransactionAsync(
            new CreateTransactionRequest
            {
                Description = "Mercado",
                Amount = 250m,
                Type = TransactionType.Expense,
                PersonId = adult.Id
            });

        await _repository.AddTransactionAsync(
            new CreateTransactionRequest
            {
                Description = "Material escolar",
                Amount = 100m,
                Type = TransactionType.Expense,
                PersonId = minor.Id
            });

        var totals = await _repository.GetTotalsAsync();

        Assert.Equal(2, totals.People.Count);

        var adultTotals = Assert.Single(
            totals.People,
            item => item.PersonId == adult.Id);

        Assert.Equal(1_000m, adultTotals.Income);
        Assert.Equal(250m, adultTotals.Expenses);
        Assert.Equal(750m, adultTotals.Balance);

        var minorTotals = Assert.Single(
            totals.People,
            item => item.PersonId == minor.Id);

        Assert.Equal(0m, minorTotals.Income);
        Assert.Equal(100m, minorTotals.Expenses);
        Assert.Equal(-100m, minorTotals.Balance);

        Assert.Equal(1_000m, totals.General.Income);
        Assert.Equal(350m, totals.General.Expenses);
        Assert.Equal(650m, totals.General.Balance);
    }

    [Fact]
    public async Task GetTotalsAsync_ShouldIncludePersonWithoutTransactions()
    {
        var person = await _repository.AddPersonAsync("Marina", 25);

        var totals = await _repository.GetTotalsAsync();

        var personTotals = Assert.Single(totals.People);

        Assert.Equal(person.Id, personTotals.PersonId);
        Assert.Equal(0m, personTotals.Income);
        Assert.Equal(0m, personTotals.Expenses);
        Assert.Equal(0m, personTotals.Balance);

        Assert.Equal(0m, totals.General.Income);
        Assert.Equal(0m, totals.General.Expenses);
        Assert.Equal(0m, totals.General.Balance);
    }

    [Fact]
    public async Task Repository_ShouldPersistDataBetweenInstances()
    {
        var person = await _repository.AddPersonAsync("Lucas", 22);

        await _repository.AddTransactionAsync(
            new CreateTransactionRequest
            {
                Description = "Freelance",
                Amount = 900m,
                Type = TransactionType.Income,
                PersonId = person.Id
            });

        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["DataFile"] = _dataFile
            })
            .Build();

        var secondRepository = new JsonHouseholdRepository(
            new TestWebHostEnvironment
            {
                ContentRootPath = _temporaryDirectory
            },
            configuration);

        var persistedPeople = await secondRepository.GetPeopleAsync();
        var persistedTransactions = await secondRepository.GetTransactionsAsync();

        Assert.Single(persistedPeople);
        Assert.Single(persistedTransactions);
        Assert.Equal("Lucas", persistedPeople[0].Name);
        Assert.Equal(900m, persistedTransactions[0].Amount);
    }

    [Fact]
    public void CreatePersonRequest_WithBlankName_ShouldBeInvalid()
    {
        var request = new CreatePersonRequest
        {
            Name = string.Empty,
            Age = 25
        };

        var validationResults = Validate(request);

        Assert.Contains(
            validationResults,
            result => result.MemberNames.Contains(nameof(CreatePersonRequest.Name)));
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(131)]
    public void CreatePersonRequest_WithInvalidAge_ShouldBeInvalid(int age)
    {
        var request = new CreatePersonRequest
        {
            Name = "Pessoa válida",
            Age = age
        };

        var validationResults = Validate(request);

        Assert.Contains(
            validationResults,
            result => result.MemberNames.Contains(nameof(CreatePersonRequest.Age)));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-10)]
    public void CreateTransactionRequest_WithNonPositiveAmount_ShouldBeInvalid(
        int amount)
    {
        var request = new CreateTransactionRequest
        {
            Description = "Transação válida",
            Amount = amount,
            Type = TransactionType.Expense,
            PersonId = Guid.NewGuid()
        };

        var validationResults = Validate(request);

        Assert.Contains(
            validationResults,
            result => result.MemberNames.Contains(
                nameof(CreateTransactionRequest.Amount)));
    }

    private static IReadOnlyList<ValidationResult> Validate(object instance)
    {
        var results = new List<ValidationResult>();

        Validator.TryValidateObject(
            instance,
            new ValidationContext(instance),
            results,
            validateAllProperties: true);

        return results;
    }

    public void Dispose()
    {
        if (Directory.Exists(_temporaryDirectory))
        {
            Directory.Delete(_temporaryDirectory, recursive: true);
        }
    }

    private sealed class TestWebHostEnvironment : IWebHostEnvironment
    {
        public string ApplicationName { get; set; } = "LarFinance.Tests";

        public IFileProvider WebRootFileProvider { get; set; } =
            new NullFileProvider();

        public string WebRootPath { get; set; } = string.Empty;

        public string EnvironmentName { get; set; } = "Test";

        public string ContentRootPath { get; set; } = string.Empty;

        public IFileProvider ContentRootFileProvider { get; set; } =
            new NullFileProvider();
    }
}