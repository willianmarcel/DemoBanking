namespace Banking.Validators;

using Banking.Models;
using Banking.Services;
using Banking.Utils;
using FluentValidation;

public class CriarContaValidator : AbstractValidator<CriarContaRequest>
{
    private readonly IContaService _contaService;

    public CriarContaValidator(IContaService contaService)
    {
        _contaService = contaService;

        RuleFor(c => c.NomeTitular)
            .NotEmpty().WithMessage("Nome do titular é obrigatório")
            .Length(2, 100).WithMessage("Nome deve ter entre 2 e 100 caracteres")
            .Matches(@"^[a-zA-ZÀ-ÿ\s]+$").WithMessage("Nome deve conter apenas letras e espaços");

        RuleFor(c => c.CPF)
            .NotEmpty().WithMessage("CPF é obrigatório")
            .Must(ValidadorCPF.Validar).WithMessage("CPF inválido")
            .MustAsync(async (cpf, cancellation) => !await _contaService.CPFJaCadastradoAsync(cpf))
            .WithMessage("CPF já possui conta cadastrada");

        RuleFor(c => c.Tipo)
            .IsInEnum().WithMessage("Tipo de conta inválido")
            .NotEqual(TipoConta.Empresarial).WithMessage("Conta empresarial requer validação adicional");
    }
}