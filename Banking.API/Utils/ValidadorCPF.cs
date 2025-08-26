namespace Banking.Utils;

public static class ValidadorCPF
{
    public static bool Validar(string cpf)
    {
        if (string.IsNullOrWhiteSpace(cpf))
            return false;

        // Remove pontos, hífens e espaços
        cpf = cpf.Replace(".", "").Replace("-", "").Replace(" ", "");

        // Verifica se tem 11 dígitos
        if (cpf.Length != 11)
            return false;

        // Verifica se todos os dígitos são iguais
        if (cpf.All(c => c == cpf[0]))
            return false;

        // Validação do primeiro dígito verificador
        int soma = 0;
        for (int i = 0; i < 9; i++)
        {
            soma += int.Parse(cpf[i].ToString()) * (10 - i);
        }

        int resto = soma % 11;
        int digitoVerificador1 = resto < 2 ? 0 : 11 - resto;

        if (int.Parse(cpf[9].ToString()) != digitoVerificador1)
            return false;

        // Validação do segundo dígito verificador
        soma = 0;
        for (int i = 0; i < 10; i++)
        {
            soma += int.Parse(cpf[i].ToString()) * (11 - i);
        }

        resto = soma % 11;
        int digitoVerificador2 = resto < 2 ? 0 : 11 - resto;

        return int.Parse(cpf[10].ToString()) == digitoVerificador2;
    }
}