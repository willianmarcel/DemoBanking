using Banking.Models;

namespace Banking.Services;

public interface IContaService
{
    Task<Conta> CriarContaAsync(CriarContaRequest request);
    Task<Conta> DepositarAsync(DepositoRequest request);
    Task<Conta> SacarAsync(SaqueRequest request);
    Task TransferirAsync(TransferenciaRequest request);
    Task<Conta?> GetContaAsync(string numeroConta);
    Task<bool> ContaExisteAsync(string numeroConta);
    Task<decimal> GetSaldoAsync(string numeroConta);
    Task<bool> CPFJaCadastradoAsync(string cpf);
    Task<decimal> GetTransferenciasDiariasAsync(string numeroConta);
}