using CustomerDocumentValidation.Domain.CadastrosClientes.Enums;

namespace CustomerDocumentValidation.Application.CadastrosClientes.Events;

public sealed record DocumentoRecebidoEvent(
    string EventoId,
    string CadastroClienteId,
    string DocumentoId,
    string Bucket,
    string ChaveArquivo,
    TipoDocumento TipoDocumento,
    string CorrelationId,
    DateTime CriadoEmUtc
);