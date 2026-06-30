namespace CustomerDocumentValidation.Application.CadastrosClientes.Services;

public sealed record ResultadoArmazenamentoDocumento(
    string Bucket,
    string ChaveArquivo
);