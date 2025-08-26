namespace Banking.Validators;

using FluentValidation;
using Banking.Models;
using Banking.Services;

public class DepositoValidator : AbstractValidator<DepositoRequest>
{
    private readonly IContaService _contaService;

    public DepositoValidator(IContaService contaService)
    {
        _contaService = contaService;

        RuleFor(d => d.NumeroConta)
            .NotEmpty().WithMessage("Número da conta é obrigatório")
            .MustAsync(async (numeroConta, cancellation) =>
                await _contaService.ContaExisteAsync(numeroConta))
            .WithMessage("Conta não encontrada");

        RuleFor(d => d.Valor)
            .GreaterThan(0).WithMessage("Valor deve ser maior que zero")
            .LessThanOrEqualTo(50000).WithMessage("Valor máximo para depósito é R$ 50.000")
            .Must(TerDuasCasasDecimais).WithMessage("Valor deve ter no máximo 2 casas decimais");
    }

    private bool TerDuasCasasDecimais(decimal valor)
    {
        return decimal.Round(valor, 2) == valor;
    }
}