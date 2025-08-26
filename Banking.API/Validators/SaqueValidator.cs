namespace Banking.Validators;

using FluentValidation;
using Banking.Models;
using Banking.Services;

public class SaqueValidator : AbstractValidator<SaqueRequest>
{
    private readonly IContaService _contaService;

    public SaqueValidator(IContaService contaService)
    {
        _contaService = contaService;

        RuleFor(s => s.NumeroConta)
            .NotEmpty().WithMessage("Número da conta é obrigatório")
            .MustAsync(ContaExiste).WithMessage("Conta não encontrada");

        RuleFor(s => s.Valor)
            .GreaterThan(0).WithMessage("Valor deve ser maior que zero")
            .Must(TerDuasCasasDecimais).WithMessage("Valor deve ter no máximo 2 casas decimais");

        // Validação condicional para conta poupança
        RuleFor(s => s.Valor)
            .LessThanOrEqualTo(1000)
            .When(s => TipoContaPoupanca(s.NumeroConta))
            .WithMessage("Conta poupança permite saque máximo de R$ 1.000 por operação");

        // Validação de saldo suficiente
        RuleFor(s => s)
            .MustAsync(TerSaldoSuficiente)
            .WithMessage("Saldo insuficiente para realizar o saque");

        // Validação de horário (apenas exemplo)
        RuleFor(s => s)
            .Must(_ => ValidarHorarioFuncionamento())
            .WithMessage("Saques só podem ser realizados entre 6h e 22h");
    }

    private async Task<bool> ContaExiste(string numeroConta, CancellationToken cancellation)
    {
        return await _contaService.ContaExisteAsync(numeroConta);
    }

    private bool TipoContaPoupanca(string numeroConta)
    {
        var conta = _contaService.GetContaAsync(numeroConta).Result;
        return conta?.Tipo == TipoConta.Poupanca;
    }

    private async Task<bool> TerSaldoSuficiente(SaqueRequest request, CancellationToken cancellation)
    {
        var saldo = await _contaService.GetSaldoAsync(request.NumeroConta);
        return saldo >= request.Valor;
    }

    private bool ValidarHorarioFuncionamento()
    {
        var agora = DateTime.Now.Hour;
        return agora >= 6 && agora <= 22;
    }

    private bool TerDuasCasasDecimais(decimal valor)
    {
        return decimal.Round(valor, 2) == valor;
    }
}