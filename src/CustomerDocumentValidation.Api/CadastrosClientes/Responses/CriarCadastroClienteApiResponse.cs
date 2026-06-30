namespace CustomerDocumentValidation.Api.CadastrosClientes.Responses;

public sealed record CriarCadastroClienteApiResponse(
    string CadastroClienteId,
    string Status,
    string Mensagem
);