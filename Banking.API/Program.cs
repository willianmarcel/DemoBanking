using Banking.Models;
using Banking.Services;
using FluentValidation;

var builder = WebApplication.CreateBuilder(args);

// Add services
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// FluentValidation
builder.Services.AddValidatorsFromAssemblyContaining<Program>();

// Services
builder.Services.AddSingleton<IContaService, ContaService>();

var app = builder.Build();

// Configure pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Endpoints
app.MapPost("/contas", async (CriarContaRequest request,
    IValidator<CriarContaRequest> validator,
    IContaService service) =>
{
    var validationResult = await validator.ValidateAsync(request);

    if (!validationResult.IsValid)
    {
        return Results.ValidationProblem(validationResult.ToDictionary());
    }

    var conta = await service.CriarContaAsync(request);
    return Results.Created($"/contas/{conta.NumeroConta}", conta);
});

app.MapPost("/contas/{numeroConta}/deposito", async (string numeroConta, DepositoRequest request,
    IValidator<DepositoRequest> validator,
    IContaService service) =>
{
    request = request with { NumeroConta = numeroConta };

    var validationResult = await validator.ValidateAsync(request);
    if (!validationResult.IsValid)
    {
        return Results.ValidationProblem(validationResult.ToDictionary());
    }

    var conta = await service.DepositarAsync(request);
    return Results.Ok(conta);
});

app.MapPost("/contas/{numeroConta}/saque", async (string numeroConta, SaqueRequest request,
    IValidator<SaqueRequest> validator,
    IContaService service) =>
{
    request = request with { NumeroConta = numeroConta };

    var validationResult = await validator.ValidateAsync(request);
    if (!validationResult.IsValid)
    {
        return Results.ValidationProblem(validationResult.ToDictionary());
    }

    var conta = await service.SacarAsync(request);
    return Results.Ok(conta);
});

app.MapPost("/transferencias", async (TransferenciaRequest request,
    IValidator<TransferenciaRequest> validator,
    IContaService service) =>
{
    var validationResult = await validator.ValidateAsync(request);
    if (!validationResult.IsValid)
    {
        return Results.ValidationProblem(validationResult.ToDictionary());
    }

    await service.TransferirAsync(request);
    return Results.Ok(new { Message = "Transferência realizada com sucesso" });
});

app.MapGet("/contas/{numeroConta}", async (string numeroConta, IContaService service) =>
{
    var conta = await service.GetContaAsync(numeroConta);
    return conta is not null ? Results.Ok(conta) : Results.NotFound();
});

app.Run();