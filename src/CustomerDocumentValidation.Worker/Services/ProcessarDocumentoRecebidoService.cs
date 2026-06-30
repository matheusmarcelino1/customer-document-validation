using CustomerDocumentValidation.Application.CadastrosClientes.Events;
using CustomerDocumentValidation.Application.CadastrosClientes.Repositories;
using CustomerDocumentValidation.Application.CadastrosClientes.Services;
using CustomerDocumentValidation.SharedKernel.Time;

namespace CustomerDocumentValidation.Worker.Services;

public sealed class ProcessarDocumentoRecebidoService
{
    private readonly ICadastroClienteRepository _cadastroClienteRepository;
    private readonly IArmazenamentoDocumentoService _armazenamentoDocumentoService;
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly ILogger<ProcessarDocumentoRecebidoService> _logger;

    public ProcessarDocumentoRecebidoService(
        ICadastroClienteRepository cadastroClienteRepository,
        IArmazenamentoDocumentoService armazenamentoDocumentoService,
        IDateTimeProvider dateTimeProvider,
        ILogger<ProcessarDocumentoRecebidoService> logger)
    {
        _cadastroClienteRepository = cadastroClienteRepository;
        _armazenamentoDocumentoService = armazenamentoDocumentoService;
        _dateTimeProvider = dateTimeProvider;
        _logger = logger;
    }

    public async Task ProcessarAsync(
        DocumentoRecebidoEvent evento,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(evento);

        ValidarEvento(evento);

        _logger.LogInformation(
            "Iniciando processamento do documento. CadastroClienteId: {CadastroClienteId}, CorrelationId: {CorrelationId}",
            evento.CadastroClienteId,
            evento.CorrelationId);

        var cadastroCliente = await _cadastroClienteRepository.ObterPorIdAsync(
            evento.CadastroClienteId,
            cancellationToken);

        if (cadastroCliente is null)
        {
            _logger.LogWarning(
                "Cadastro nao encontrado. CadastroClienteId: {CadastroClienteId}, CorrelationId: {CorrelationId}",
                evento.CadastroClienteId,
                evento.CorrelationId);

            return;
        }

        try
        {
            cadastroCliente.MarcarComoEmProcessamento(_dateTimeProvider.UtcNow);

            await _cadastroClienteRepository.AtualizarAsync(
                cadastroCliente,
                cancellationToken);

            await using var documentoStream = await _armazenamentoDocumentoService.BaixarAsync(
                evento.Bucket,
                evento.ChaveArquivo,
                cancellationToken);

            if (documentoStream.Length <= 0)
                throw new InvalidOperationException("Documento baixado esta vazio.");

            cadastroCliente.MarcarComoProcessadoComSucesso(_dateTimeProvider.UtcNow);

            await _cadastroClienteRepository.AtualizarAsync(
                cadastroCliente,
                cancellationToken);

            _logger.LogInformation(
                "Documento processado com sucesso. CadastroClienteId: {CadastroClienteId}, CorrelationId: {CorrelationId}",
                evento.CadastroClienteId,
                evento.CorrelationId);
        }
        catch (Exception ex)
        {
            cadastroCliente.MarcarComoProcessadoComErro(
                ex.Message,
                _dateTimeProvider.UtcNow);

            await _cadastroClienteRepository.AtualizarAsync(
                cadastroCliente,
                cancellationToken);

            _logger.LogError(
                ex,
                "Erro ao processar documento. CadastroClienteId: {CadastroClienteId}, CorrelationId: {CorrelationId}",
                evento.CadastroClienteId,
                evento.CorrelationId);

            throw;
        }
    }

    private static void ValidarEvento(DocumentoRecebidoEvent evento)
    {
        if (string.IsNullOrWhiteSpace(evento.CadastroClienteId))
            throw new ArgumentException("O cadastroClienteId deve ser informado.", nameof(evento));

        if (string.IsNullOrWhiteSpace(evento.DocumentoId))
            throw new ArgumentException("O documentoId deve ser informado.", nameof(evento));

        if (string.IsNullOrWhiteSpace(evento.Bucket))
            throw new ArgumentException("O bucket deve ser informado.", nameof(evento));

        if (string.IsNullOrWhiteSpace(evento.ChaveArquivo))
            throw new ArgumentException("A chave do arquivo deve ser informada.", nameof(evento));

        if (string.IsNullOrWhiteSpace(evento.CorrelationId))
            throw new ArgumentException("O correlationId deve ser informado.", nameof(evento));
    }
}