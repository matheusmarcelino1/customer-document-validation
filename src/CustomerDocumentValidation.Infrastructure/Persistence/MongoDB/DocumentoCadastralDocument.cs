namespace CustomerDocumentValidation.Infrastructure.Persistence.MongoDB;

public sealed class DocumentoCadastralDocument
{
    public string Id { get; set; } = string.Empty;

    public int Tipo { get; set; }

    public string NomeArquivoOriginal { get; set; } = string.Empty;

    public string TipoConteudo { get; set; } = string.Empty;

    public long TamanhoEmBytes { get; set; }

    public string Bucket { get; set; } = string.Empty;

    public string ChaveArquivo { get; set; } = string.Empty;
}