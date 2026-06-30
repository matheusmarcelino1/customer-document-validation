using CustomerDocumentValidation.Application.CadastrosClientes.Repositories;
using CustomerDocumentValidation.Domain.CadastrosClientes.Entities;
using CustomerDocumentValidation.Domain.CadastrosClientes.Enums;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace CustomerDocumentValidation.Infrastructure.Persistence.MongoDB;

public sealed class CadastroClienteMongoRepository : ICadastroClienteRepository
{
    private readonly IMongoCollection<CadastroClienteDocument> _collection;

    public CadastroClienteMongoRepository(IOptions<MongoDbOptions> options)
    {
        var mongoDbOptions = options.Value;

        var mongoClient = new MongoClient(mongoDbOptions.ConnectionString);
        var database = mongoClient.GetDatabase(mongoDbOptions.DatabaseName);

        _collection = database.GetCollection<CadastroClienteDocument>(
            mongoDbOptions.CadastrosClientesCollectionName);
    }

    public async Task AdicionarAsync(
        CadastroCliente cadastroCliente,
        CancellationToken cancellationToken)
    {
        var document = MapearParaDocument(cadastroCliente);

        await _collection.InsertOneAsync(
            document,
            cancellationToken: cancellationToken);
    }

    public async Task<CadastroCliente?> ObterPorIdAsync(
        string cadastroClienteId,
        CancellationToken cancellationToken)
    {
        var document = await _collection
            .Find(x => x.Id == cadastroClienteId)
            .FirstOrDefaultAsync(cancellationToken);

        return document is null
            ? null
            : MapearParaDominio(document);
    }

    public async Task AtualizarAsync(
        CadastroCliente cadastroCliente,
        CancellationToken cancellationToken)
    {
        var document = MapearParaDocument(cadastroCliente);

        await _collection.ReplaceOneAsync(
            x => x.Id == cadastroCliente.Id,
            document,
            new ReplaceOptions { IsUpsert = false },
            cancellationToken);
    }

    private static CadastroClienteDocument MapearParaDocument(
        CadastroCliente cadastroCliente)
    {
        return new CadastroClienteDocument
        {
            Id = cadastroCliente.Id,
            NomeCompleto = cadastroCliente.NomeCompleto,
            NumeroDocumento = cadastroCliente.NumeroDocumento,
            DataNascimento = cadastroCliente.DataNascimento,
            Status = (int)cadastroCliente.Status,
            CriadoEmUtc = cadastroCliente.CriadoEmUtc,
            AtualizadoEmUtc = cadastroCliente.AtualizadoEmUtc,
            MotivoErro = cadastroCliente.MotivoErro,
            Documento = new DocumentoCadastralDocument
            {
                Id = cadastroCliente.Documento.Id,
                Tipo = (int)cadastroCliente.Documento.Tipo,
                NomeArquivoOriginal = cadastroCliente.Documento.NomeArquivoOriginal,
                TipoConteudo = cadastroCliente.Documento.TipoConteudo,
                TamanhoEmBytes = cadastroCliente.Documento.TamanhoEmBytes,
                Bucket = cadastroCliente.Documento.Bucket,
                ChaveArquivo = cadastroCliente.Documento.ChaveArquivo
            }
        };
    }

    private static CadastroCliente MapearParaDominio(
        CadastroClienteDocument document)
    {
        var documento = DocumentoCadastral.Reconstituir(
            document.Documento.Id,
            (TipoDocumento)document.Documento.Tipo,
            document.Documento.NomeArquivoOriginal,
            document.Documento.TipoConteudo,
            document.Documento.TamanhoEmBytes,
            document.Documento.Bucket,
            document.Documento.ChaveArquivo);

        return CadastroCliente.Reconstituir(
            document.Id,
            document.NomeCompleto,
            document.NumeroDocumento,
            document.DataNascimento,
            documento,
            (StatusProcessamento)document.Status,
            document.CriadoEmUtc,
            document.AtualizadoEmUtc,
            document.MotivoErro);
    }
}