using CustomerDocumentValidation.Domain.CadastrosClientes.Enums;

namespace CustomerDocumentValidation.Api.CadastrosClientes.Requests;

public sealed class CriarCadastroClienteRequest
{
    public string NomeCompleto { get; init; } = string.Empty;

    public string NumeroDocumento { get; init; } = string.Empty;

    public DateOnly DataNascimento { get; init; }

    public TipoDocumento TipoDocumento { get; init; }

    public IFormFile Documento { get; init; } = default!;
}