using CustomerDocumentValidation.Domain.CadastrosClientes.Enums;
using CustomerDocumentValidation.SharedKernel.Primitives;

namespace CustomerDocumentValidation.Domain.CadastrosClientes.Entities;

public sealed class CadastroCliente : Entity
{
    private CadastroCliente(
        string id,
        string nomeCompleto,
        string numeroDocumento,
        DateOnly dataNascimento,
        DocumentoCadastral documento,
        DateTime criadoEmUtc)
        : base(id)
    {
        NomeCompleto = nomeCompleto;
        NumeroDocumento = numeroDocumento;
        DataNascimento = dataNascimento;
        Documento = documento;
        Status = StatusProcessamento.PendenteProcessamento;
        CriadoEmUtc = criadoEmUtc;
    }

    public string NomeCompleto { get; private set; }

    public string NumeroDocumento { get; private set; }

    public DateOnly DataNascimento { get; private set; }

    public DocumentoCadastral Documento { get; private set; }

    public StatusProcessamento Status { get; private set; }

    public DateTime CriadoEmUtc { get; private set; }

    public DateTime? AtualizadoEmUtc { get; private set; }

    public string? MotivoErro { get; private set; }

    public static CadastroCliente Criar(
        string nomeCompleto,
        string numeroDocumento,
        DateOnly dataNascimento,
        DocumentoCadastral documento,
        DateTime criadoEmUtc)
    {
        if (string.IsNullOrWhiteSpace(nomeCompleto))
            throw new ArgumentException("O nome completo deve ser informado.", nameof(nomeCompleto));

        if (string.IsNullOrWhiteSpace(numeroDocumento))
            throw new ArgumentException("O numero do documento deve ser informado.", nameof(numeroDocumento));

        if (dataNascimento == default)
            throw new ArgumentException("A data de nascimento deve ser informada.", nameof(dataNascimento));

        if (dataNascimento > DateOnly.FromDateTime(criadoEmUtc))
            throw new ArgumentException("A data de nascimento nao pode estar no futuro.", nameof(dataNascimento));

        ArgumentNullException.ThrowIfNull(documento);

        return new CadastroCliente(
            Guid.NewGuid().ToString(),
            nomeCompleto.Trim(),
            numeroDocumento.Trim(),
            dataNascimento,
            documento,
            criadoEmUtc);
    }

    public void MarcarComoEmProcessamento(DateTime atualizadoEmUtc)
    {
        if (Status == StatusProcessamento.ProcessadoComSucesso)
            throw new InvalidOperationException("Um cadastro processado com sucesso nao pode ser processado novamente.");

        Status = StatusProcessamento.EmProcessamento;
        AtualizadoEmUtc = atualizadoEmUtc;
        MotivoErro = null;
    }

    public void MarcarComoProcessadoComSucesso(DateTime atualizadoEmUtc)
    {
        if (Status != StatusProcessamento.EmProcessamento)
            throw new InvalidOperationException("Somente um cadastro em processamento pode ser concluido com sucesso.");

        Status = StatusProcessamento.ProcessadoComSucesso;
        AtualizadoEmUtc = atualizadoEmUtc;
        MotivoErro = null;
    }

    public void MarcarComoProcessadoComErro(string motivoErro, DateTime atualizadoEmUtc)
    {
        if (string.IsNullOrWhiteSpace(motivoErro))
            throw new ArgumentException("O motivo do erro deve ser informado.", nameof(motivoErro));

        Status = StatusProcessamento.ProcessadoComErro;
        AtualizadoEmUtc = atualizadoEmUtc;
        MotivoErro = motivoErro.Trim();
    }
}