using CustomerDocumentValidation.Application.CadastrosClientes.Events;
using CustomerDocumentValidation.Application.CadastrosClientes.Repositories;
using CustomerDocumentValidation.Application.CadastrosClientes.Services;
using CustomerDocumentValidation.Application.CadastrosClientes.Topics;
using CustomerDocumentValidation.Domain.CadastrosClientes.Entities;
using CustomerDocumentValidation.SharedKernel.Time;

namespace CustomerDocumentValidation.Application.CadastrosClientes.Commands;

public sealed class CriarCadastroClienteCommandHandler
{
    private readonly ICadastroClienteRepository _cadastroClienteRepository;
    private readonly IArmazenamentoDocumentoService _armazenamentoDocumentoService;
    private readonly IEventPublisher _eventPublisher;
    private readonly IDateTimeProvider _dateTimeProvider;

    public CriarCadastroClienteCommandHandler(
        ICadastroClienteRepository cadastroClienteRepository,
        IArmazenamentoDocumentoService armazenamentoDocumentoService,
        IEventPublisher eventPublisher,
        IDateTimeProvider dateTimeProvider)
    {
        _cadastroClienteRepository = cadastroClienteRepository;
        _armazenamentoDocumentoService = armazenamentoDocumentoService;
        _eventPublisher = eventPublisher;
        _dateTimeProvider = dateTimeProvider;
    }

    public async Task<CriarCadastroClienteResponse> HandleAsync(
        CriarCadastroClienteCommand command,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);

        ValidarCommand(command);

        var resultadoArmazenamento = await _armazenamentoDocumentoService.ArmazenarAsync(
            command.ConteudoArquivo,
            command.NomeArquivoOriginal,
            command.TipoConteudoArquivo,
            cancellationToken);

        var documento = DocumentoCadastral.Criar(
            command.TipoDocumento,
            command.NomeArquivoOriginal,
            command.TipoConteudoArquivo,
            command.TamanhoArquivoEmBytes,
            resultadoArmazenamento.Bucket,
            resultadoArmazenamento.ChaveArquivo);

        var cadastroCliente = CadastroCliente.Criar(
            command.NomeCompleto,
            command.NumeroDocumento,
            command.DataNascimento,
            documento,
            _dateTimeProvider.UtcNow);

        await _cadastroClienteRepository.AdicionarAsync(
            cadastroCliente,
            cancellationToken);

        var evento = new DocumentoRecebidoEvent(
            Guid.NewGuid().ToString(),
            cadastroCliente.Id,
            documento.Id,
            documento.Bucket,
            documento.ChaveArquivo,
            documento.Tipo,
            command.CorrelationId,
            _dateTimeProvider.UtcNow);

        await _eventPublisher.PublicarAsync(
            TopicosMensageria.DocumentoRecebido,
            evento,
            cancellationToken);

        return new CriarCadastroClienteResponse(
            cadastroCliente.Id,
            cadastroCliente.Status,
            "Cadastro recebido e enviado para processamento.");
    }

    private static void ValidarCommand(CriarCadastroClienteCommand command)
    {
        if (string.IsNullOrWhiteSpace(command.NomeCompleto))
            throw new ArgumentException("O nome completo deve ser informado.", nameof(command));

        if (string.IsNullOrWhiteSpace(command.NumeroDocumento))
            throw new ArgumentException("O numero do documento deve ser informado.", nameof(command));

        if (command.DataNascimento == default)
            throw new ArgumentException("A data de nascimento deve ser informada.", nameof(command));

        if (command.TipoDocumento == Domain.CadastrosClientes.Enums.TipoDocumento.Desconhecido)
            throw new ArgumentException("O tipo do documento deve ser informado.", nameof(command));

        if (string.IsNullOrWhiteSpace(command.NomeArquivoOriginal))
            throw new ArgumentException("O nome original do arquivo deve ser informado.", nameof(command));

        if (string.IsNullOrWhiteSpace(command.TipoConteudoArquivo))
            throw new ArgumentException("O tipo de conteudo do arquivo deve ser informado.", nameof(command));

        if (command.TamanhoArquivoEmBytes <= 0)
            throw new ArgumentException("O tamanho do arquivo deve ser maior que zero.", nameof(command));

        if (command.ConteudoArquivo is null)
            throw new ArgumentException("O conteudo do arquivo deve ser informado.", nameof(command));

        if (!command.ConteudoArquivo.CanRead)
            throw new ArgumentException("O conteudo do arquivo deve permitir leitura.", nameof(command));

        if (string.IsNullOrWhiteSpace(command.CorrelationId))
            throw new ArgumentException("O correlationId deve ser informado.", nameof(command));
    }
}