# Sistema de Conta Corrente com FluentValidation

Este projeto demonstra a implementação de validações robustas em uma API .NET 8 usando a biblioteca FluentValidation. O sistema simula operações básicas de um banco, incluindo criação de contas, depósitos, saques e transferências.

## Arquitetura do Projeto

O projeto segue uma arquitetura em camadas com separação clara de responsabilidades:

- **Models**: Entidades de domínio e DTOs
- **Services**: Lógica de negócio e operações de domínio
- **Validators**: Regras de validação isoladas usando FluentValidation
- **Extensions**: Métodos de extensão para integração com ASP.NET Core

## Conceitos do FluentValidation

O FluentValidation é uma biblioteca que permite criar validações de forma declarativa e fluente, oferecendo vantagens significativas sobre as tradicionais Data Annotations:

### Separação de Responsabilidades
As validações ficam isoladas em classes específicas, separadas dos modelos de domínio, facilitando manutenção e testes.

### Sintaxe Fluente
Utiliza uma API fluente que torna o código mais legível e expressivo, permitindo encadeamento de regras de validação.

### Validações Condicionais
Suporte nativo a validações condicionais usando `When()` e `Unless()`, permitindo aplicar regras baseadas em contexto.

### Validações Assíncronas
Suporte completo a validações que dependem de operações I/O, como consultas a banco de dados ou APIs externas.

### Testabilidade
Validadores podem ser facilmente testados de forma unitária, aumentando a confiabilidade do código.

## Funcionalidades Implementadas

### 1. Criação de Conta
Permite criar contas corrente, poupança ou empresarial com validações específicas para cada tipo.

**Validações aplicadas:**
- Nome do titular obrigatório, entre 2 e 100 caracteres, apenas letras e espaços
- CPF obrigatório e válido segundo algoritmo brasileiro
- Verificação de unicidade do CPF (validação assíncrona)
- Tipo de conta válido (conta empresarial requer validação adicional)

### 2. Depósito
Operação de depósito em conta existente com validações de valor e limite.

**Validações aplicadas:**
- Número da conta obrigatório e existente (validação assíncrona)
- Valor maior que zero e até duas casas decimais
- Limite máximo de R$ 50.000 por operação

### 3. Saque
Operação de saque com validações condicionais baseadas no tipo de conta.

**Validações aplicadas:**
- Número da conta obrigatório e existente (validação assíncrona)
- Valor maior que zero e até duas casas decimais
- Saldo suficiente (validação assíncrona)
- Limite específico para conta poupança (máximo R$ 1.000 por operação)
- Horário de funcionamento (6h às 22h)

### 4. Transferência
Operação mais complexa com múltiplas validações de negócio.

**Validações aplicadas:**
- Contas de origem e destino obrigatórias e existentes (validações assíncronas)
- Contas diferentes (não permitir transferência para mesma conta)
- Valor maior que zero e até duas casas decimais
- Saldo suficiente na conta origem (validação assíncrona)
- Limite diário de transferência de R$ 5.000 (validação assíncrona)
- Horário de funcionamento (6h às 22h)
- Descrição opcional com até 100 caracteres

## Tipos de Validação Demonstrados

### Validações Básicas
- `NotEmpty()`: Campo obrigatório
- `Length()`: Tamanho de string
- `GreaterThan()`: Valor maior que
- `LessThanOrEqualTo()`: Valor menor ou igual
- `Matches()`: Expressão regular

### Validações Condicionais
```csharp
RuleFor(s => s.Valor)
    .LessThanOrEqualTo(1000)
    .When(s => TipoContaPoupanca(s.NumeroConta))
    .WithMessage("Conta poupança permite saque máximo de R$ 1.000 por operação");
```

### Validações Customizadas
```csharp
RuleFor(c => c.CPF)
    .Must(ValidadorCPF.Validar)
    .WithMessage("CPF inválido");
```

### Validações Assíncronas
```csharp
RuleFor(c => c.CPF)
    .MustAsync(async (cpf, cancellation) => !await _contaService.CPFJaCadastradoAsync(cpf))
    .WithMessage("CPF já possui conta cadastrada");
```

### Validações de Regras de Negócio
```csharp
RuleFor(t => t)
    .MustAsync(ValidarLimiteDiario)
    .WithMessage("Valor excede limite diário de transferência (R$ 5.000)");
```

## Utilitários

### Validador de CPF
Implementa o algoritmo oficial de validação de CPF brasileiro, verificando:
- Formato correto (11 dígitos)
- Dígitos verificadores válidos
- Exclusão de números com todos os dígitos iguais

### Extensions de Validação
Método de extensão para converter `ValidationResult` em formato compatível com ASP.NET Core:
```csharp
public static IDictionary<string, string[]> ToDictionary(this ValidationResult validationResult)
```

## Integração com ASP.NET Core

### Configuração
```csharp
builder.Services.AddValidatorsFromAssemblyContaining<Program>();
```

### Uso nos Endpoints
```csharp
app.MapPost("/contas", async (CriarContaRequest request, 
    IValidator<CriarContaRequest> validator, 
    IContaService service) =>
{
    var validationResult = await validator.ValidateAsync(request);
    
    if (!validationResult.IsValid)
    {
        return Results.ValidationProblem(validationResult.ToDictionary());
    }
    
    // Processar requisição...
});
```

## Requisitos Técnicos

- .NET 8.0
- FluentValidation 11.8.0
- ASP.NET Core Minimal APIs

## Como Executar

1. Clone o repositório
2. Execute `dotnet restore` para restaurar as dependências
3. Execute `dotnet run` no diretório da API
4. Acesse `https://localhost:5001/swagger` para visualizar a documentação da API

## Endpoints Disponíveis

- `POST /contas` - Criar nova conta
- `POST /contas/{numeroConta}/deposito` - Realizar depósito
- `POST /contas/{numeroConta}/saque` - Realizar saque
- `POST /transferencias` - Realizar transferência
- `GET /contas/{numeroConta}` - Consultar conta

## Benefícios da Abordagem

1. **Manutenibilidade**: Validações centralizadas e fáceis de modificar
2. **Testabilidade**: Cada validador pode ser testado isoladamente
3. **Reutilização**: Validadores podem ser reutilizados em diferentes contextos
4. **Performance**: Validações otimizadas com suporte a cache interno
5. **Flexibilidade**: Suporte completo a cenários complexos de validação
6. **Separação de Responsabilidades**: Modelos limpos, focados apenas em representar dados

Esta implementação serve como exemplo prático de como aplicar FluentValidation em projetos reais, demonstrando desde validações simples até cenários complexos de regras de negócio.
