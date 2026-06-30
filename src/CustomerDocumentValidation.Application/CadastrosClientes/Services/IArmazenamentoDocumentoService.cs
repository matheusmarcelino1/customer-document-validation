namespace CustomerDocumentValidation.Application.CadastrosClientes.Services;

public interface IArmazenamentoDocumentoService
{
    Task<ResultadoArmazenamentoDocumento> ArmazenarAsync(
        Stream conteudoArquivo,
        string nomeArquivoOriginal,
        string tipoConteudo,
        CancellationToken cancellationToken);
}