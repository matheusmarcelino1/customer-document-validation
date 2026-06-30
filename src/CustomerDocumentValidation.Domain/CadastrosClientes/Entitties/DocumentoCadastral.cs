using CustomerDocumentValidation.Domain.CadastrosClientes.Enums;
using CustomerDocumentValidation.SharedKernel.Primitives;

namespace CustomerDocumentValidation.Domain.CadastrosClientes.Entities;

public sealed class DocumentoCadastral : Entity
{
    private DocumentoCadastral(
        string id,
        TipoDocumento tipo,
        string nomeArquivoOriginal,
        string tipoConteudo,
        long tamanhoEmBytes,
        string bucket,
        string chaveArquivo)
        : base(id)
    {
        Tipo = tipo;
        NomeArquivoOriginal = nomeArquivoOriginal;
        TipoConteudo = tipoConteudo;
        TamanhoEmBytes = tamanhoEmBytes;
        Bucket = bucket;
        ChaveArquivo = chaveArquivo;
    }

    public TipoDocumento Tipo { get; }

    public string NomeArquivoOriginal { get; }

    public string TipoConteudo { get; }

    public long TamanhoEmBytes { get; }

    public string Bucket { get; }

    public string ChaveArquivo { get; }

    public static DocumentoCadastral Criar(
        TipoDocumento tipo,
        string nomeArquivoOriginal,
        string tipoConteudo,
        long tamanhoEmBytes,
        string bucket,
        string chaveArquivo)
    {
        if (tipo == TipoDocumento.Desconhecido)
            throw new ArgumentException("O tipo do documento deve ser informado.", nameof(tipo));

        if (string.IsNullOrWhiteSpace(nomeArquivoOriginal))
            throw new ArgumentException("O nome original do arquivo deve ser informado.", nameof(nomeArquivoOriginal));

        if (string.IsNullOrWhiteSpace(tipoConteudo))
            throw new ArgumentException("O tipo de conteudo do arquivo deve ser informado.", nameof(tipoConteudo));

        if (tamanhoEmBytes <= 0)
            throw new ArgumentException("O tamanho do arquivo deve ser maior que zero.", nameof(tamanhoEmBytes));

        if (string.IsNullOrWhiteSpace(bucket))
            throw new ArgumentException("O bucket deve ser informado.", nameof(bucket));

        if (string.IsNullOrWhiteSpace(chaveArquivo))
            throw new ArgumentException("A chave do arquivo deve ser informada.", nameof(chaveArquivo));

        return new DocumentoCadastral(
            Guid.NewGuid().ToString(),
            tipo,
            nomeArquivoOriginal.Trim(),
            tipoConteudo.Trim(),
            tamanhoEmBytes,
            bucket.Trim(),
            chaveArquivo.Trim());
    }

    public static DocumentoCadastral Reconstituir(
    string id,
    TipoDocumento tipo,
    string nomeArquivoOriginal,
    string tipoConteudo,
    long tamanhoEmBytes,
    string bucket,
    string chaveArquivo)
    {
        return new DocumentoCadastral(
            id,
            tipo,
            nomeArquivoOriginal,
            tipoConteudo,
            tamanhoEmBytes,
            bucket,
            chaveArquivo);
    }
}