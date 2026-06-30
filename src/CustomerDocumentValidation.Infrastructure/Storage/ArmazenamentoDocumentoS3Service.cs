using Amazon.S3;
using Amazon.S3.Model;
using CustomerDocumentValidation.Application.CadastrosClientes.Services;
using Microsoft.Extensions.Options;

namespace CustomerDocumentValidation.Infrastructure.Storage;

public sealed class ArmazenamentoDocumentoS3Service : IArmazenamentoDocumentoService
{
    private readonly IAmazonS3 _s3Client;
    private readonly S3Options _options;

    public ArmazenamentoDocumentoS3Service(
        IAmazonS3 s3Client,
        IOptions<S3Options> options)
    {
        _s3Client = s3Client;
        _options = options.Value;
    }

    public async Task<ResultadoArmazenamentoDocumento> ArmazenarAsync(
        Stream conteudoArquivo,
        string nomeArquivoOriginal,
        string tipoConteudo,
        CancellationToken cancellationToken)
    {
        if (conteudoArquivo is null)
            throw new ArgumentException("O conteudo do arquivo deve ser informado.", nameof(conteudoArquivo));

        if (string.IsNullOrWhiteSpace(nomeArquivoOriginal))
            throw new ArgumentException("O nome original do arquivo deve ser informado.", nameof(nomeArquivoOriginal));

        if (string.IsNullOrWhiteSpace(tipoConteudo))
            throw new ArgumentException("O tipo de conteudo deve ser informado.", nameof(tipoConteudo));

        var chaveArquivo = CriarChaveArquivo(nomeArquivoOriginal);

        var request = new PutObjectRequest
        {
            BucketName = _options.BucketName,
            Key = chaveArquivo,
            InputStream = conteudoArquivo,
            ContentType = tipoConteudo
        };

        await _s3Client.PutObjectAsync(request, cancellationToken);

        return new ResultadoArmazenamentoDocumento(
            _options.BucketName,
            chaveArquivo);
    }

    public async Task<Stream> BaixarAsync(
    string bucket,
    string chaveArquivo,
    CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(bucket))
            throw new ArgumentException("O bucket deve ser informado.", nameof(bucket));

        if (string.IsNullOrWhiteSpace(chaveArquivo))
            throw new ArgumentException("A chave do arquivo deve ser informada.", nameof(chaveArquivo));

        var request = new GetObjectRequest
        {
            BucketName = bucket,
            Key = chaveArquivo
        };

        using var response = await _s3Client.GetObjectAsync(
            request,
            cancellationToken);

        var memoryStream = new MemoryStream();

        await response.ResponseStream.CopyToAsync(
            memoryStream,
            cancellationToken);

        memoryStream.Position = 0;

        return memoryStream;
    }

    private static string CriarChaveArquivo(string nomeArquivoOriginal)
    {
        var extensao = Path.GetExtension(nomeArquivoOriginal);
        var nomeSeguro = Path.GetFileNameWithoutExtension(nomeArquivoOriginal)
            .Replace(" ", "-")
            .ToLowerInvariant();

        return $"cadastros/{Guid.NewGuid()}/{nomeSeguro}{extensao}";
    }
}