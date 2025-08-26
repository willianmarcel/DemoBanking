namespace Banking.Validators;

using Banking.Models;
using Banking.Services;
using FluentValidation;

public class TransferenciaValidator : AbstractValidator<TransferenciaRequest>
{
    private readonly IContaService _contaService;

    public TransferenciaValidator(IContaService contaService)
    {
        _contaService = contaService;

        RuleFor(t => t.ContaOrigem)
            .NotEmpty().WithMessage("Conta de origem é obrigatória")
            .MustAsync(ContaExiste).WithMessage("Conta de origem não encontrada");

        RuleFor(t => t.ContaDestino)
            .NotEmpty().WithMessage("Conta de destino é obrigatória")
            .MustAsync(ContaExiste).WithMessage("Conta de destino não encontrada");

        RuleFor(t => t.ContaOrigem)
            .NotEqual(t => t.ContaDestino)
            .WithMessage("Não é possível transferir para a mesma conta");

        RuleFor(t => t.Valor)
            .GreaterThan(0).WithMessage("Valor deve ser maior que zero")
            .Must(TerDuasCasasDecimais).WithMessage("Valor deve ter no máximo 2 casas decimais");

        // Validação de limite diário
        RuleFor(t => t)
            .MustAsync(ValidarLimiteDiario)
            .WithMessage("Valor excede limite diário de transferência (R$ 5.000)");

        // Validação de saldo suficiente
        RuleFor(t => t)
            .MustAsync(TerSaldoSuficiente)
            .WithMessage("Saldo insuficiente na conta de origem");

        // Validação de horário
        RuleFor(t => t)
            .Must(_ => ValidarHorarioFuncionamento())
            .WithMessage("Transferências só podem ser realizadas entre 6h e 22h");

        // Validação opcional de descrição
        RuleFor(t => t.Descricao)
            .MaximumLength(100)
            .WithMessage("Descrição deve ter no máximo 100 caracteres")
            .When(t => !string.IsNullOrEmpty(t.Descricao));
    }

    private async Task<bool> ContaExiste(string numeroConta, CancellationToken cancellation)
    {
        return await _contaService.ContaExisteAsync(numeroConta);
    }

    private async Task<bool> ValidarLimiteDiario(TransferenciaRequest request, CancellationToken cancellation)
    {
        var transferenciasHoje = await _contaService.GetTransferenciasDiariasAsync(request.ContaOrigem);
        return (transferenciasHoje + request.Valor) <= 5000;
    }

    private async Task<bool> TerSaldoSuficiente(TransferenciaRequest request, CancellationToken cancellation)
    {
        var saldo = await _contaService.GetSaldoAsync(request.ContaOrigem);
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