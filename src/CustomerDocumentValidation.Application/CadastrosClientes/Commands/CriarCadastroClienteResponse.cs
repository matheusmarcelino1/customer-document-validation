using CustomerDocumentValidation.Domain.CadastrosClientes.Enums;

namespace CustomerDocumentValidation.Application.CadastrosClientes.Commands;

public sealed record CriarCadastroClienteResponse(
    string CadastroClienteId,
    StatusProcessamento Status,
    string Mensagem
);