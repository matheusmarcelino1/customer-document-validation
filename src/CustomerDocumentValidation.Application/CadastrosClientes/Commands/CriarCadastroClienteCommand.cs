using CustomerDocumentValidation.Domain.CadastrosClientes.Enums;

namespace CustomerDocumentValidation.Application.CadastrosClientes.Commands;

public sealed record CriarCadastroClienteCommand(
    string NomeCompleto,
    string NumeroDocumento,
    DateOnly DataNascimento,
    TipoDocumento TipoDocumento,
    string NomeArquivoOriginal,
    string TipoConteudoArquivo,
    long TamanhoArquivoEmBytes,
    Stream ConteudoArquivo,
    string CorrelationId
);