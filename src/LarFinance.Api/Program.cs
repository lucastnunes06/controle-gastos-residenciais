using System.Text.Json.Serialization;
using LarFinance.Api.Persistence;

var builder = WebApplication.CreateBuilder(args);

// O log em console funciona de forma previsível em Windows, Linux e contêineres.
builder.Logging.ClearProviders();
builder.Logging.AddConsole();

builder.Services
    .AddControllers()
    .AddJsonOptions(options =>
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));

builder.Services.AddProblemDetails();

// Um único repositório coordena as escritas para manter pessoas e transações consistentes.
builder.Services.AddSingleton<IHouseholdRepository, JsonHouseholdRepository>();

builder.Services.AddCors(options =>
    options.AddPolicy("web", policy =>
        policy
            .WithOrigins(builder.Configuration["WebOrigin"] ?? "http://localhost:5173")
            .AllowAnyHeader()
            .AllowAnyMethod()));

var app = builder.Build();

app.UseExceptionHandler();
app.UseCors("web");

// Em desenvolvimento o Vite serve a interface. Em uma publicação integrada,
// os arquivos compilados do React podem ser copiados para wwwroot.
var hasPublishedWebApp = Directory.Exists(app.Environment.WebRootPath);
if (hasPublishedWebApp)
{
    app.UseDefaultFiles();
    app.UseStaticFiles();
}

app.MapControllers();
app.MapGet("/api/health", () => Results.Ok(new { status = "healthy" }));
if (app.Environment.IsDevelopment())
{
    var openApiDirectory = Path.Combine(app.Environment.ContentRootPath, "OpenApi");
    app.MapGet("/openapi/v1.json", () => Results.File(
        Path.Combine(openApiDirectory, "openapi.json"),
        "application/json"));
    app.MapGet("/swagger", () => Results.File(
        Path.Combine(openApiDirectory, "swagger.html"),
        "text/html"));
}

if (hasPublishedWebApp)
{
    app.MapFallbackToFile("index.html");
}

app.Run();

// Permite que uma suíte de integração referencie o entrypoint futuramente.
public partial class Program;
