using CustomerDocumentValidation.Application.CadastrosClientes.Commands;
using CustomerDocumentValidation.Application.CadastrosClientes.Events;
using CustomerDocumentValidation.Application.CadastrosClientes.Repositories;
using CustomerDocumentValidation.Application.CadastrosClientes.Services;
using CustomerDocumentValidation.Application.CadastrosClientes.Topics;
using CustomerDocumentValidation.Domain.CadastrosClientes.Entities;
using CustomerDocumentValidation.Domain.CadastrosClientes.Enums;
using CustomerDocumentValidation.SharedKernel.Time;
using Moq;
using System.Timers;

namespace CustomerDocumentValidation.Application.Tests.CadastrosClientes;

public sealed class CriarCadastroClienteCommandHandlerTests
{
    [Fact]
    public async Task HandleAsync_DeveCriarCadastroArmazenarDocumentoSalvarCadastroEPublicarEvento()
    {
        // Arrange
        var cadastroClienteRepositoryMock = new Mock<ICadastroClienteRepository>();
        var armazenamentoDocumentoServiceMock = new Mock<IArmazenamentoDocumentoService>();
        var eventPublisherMock = new Mock<IEventPublisher>();
        var dateTimeProviderMock = new Mock<IDateTimeProvider>();

        var agora = new DateTime(2026, 6, 30, 10, 0, 0, DateTimeKind.Utc);

        dateTimeProviderMock
            .Setup(x => x.UtcNow)
            .Returns(agora);

        armazenamentoDocumentoServiceMock
            .Setup(x => x.ArmazenarAsync(
                It.IsAny<Stream>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ResultadoArmazenamentoDocumento(
                "documentos-cadastrais",
                "cadastros/123/rg.png"));

        var handler = new CriarCadastroClienteCommandHandler(
            cadastroClienteRepositoryMock.Object,
            armazenamentoDocumentoServiceMock.Object,
            eventPublisherMock.Object,
            dateTimeProviderMock.Object);

        var command = CriarCommandValido();

        // Act
        var response = await handler.HandleAsync(
            command,
            CancellationToken.None);

        // Assert
        Assert.False(string.IsNullOrWhiteSpace(response.CadastroClienteId));
        Assert.Equal(StatusProcessamento.PendenteProcessamento, response.Status);
        Assert.Equal("Cadastro recebido e enviado para processamento.", response.Mensagem);

        armazenamentoDocumentoServiceMock.Verify(x => x.ArmazenarAsync(
            command.ConteudoArquivo,
            command.NomeArquivoOriginal,
            command.TipoConteudoArquivo,
            It.IsAny<CancellationToken>()),
            Times.Once);

        cadastroClienteRepositoryMock.Verify(x => x.AdicionarAsync(
            It.IsAny<CadastroCliente>(),
            It.IsAny<CancellationToken>()),
            Times.Once);

        eventPublisherMock.Verify(x => x.PublicarAsync(
            TopicosMensageria.DocumentoRecebido,
            It.IsAny<DocumentoRecebidoEvent>(),
            It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task HandleAsync_DeveLancarExcecao_QuandoNomeCompletoForVazio()
    {
        // Arrange
        var handler = CriarHandlerPadrao();

        var command = CriarCommandValido() with
        {
            NomeCompleto = ""
        };

        // Act
        var excecao = await Assert.ThrowsAsync<ArgumentException>(() =>
            handler.HandleAsync(command, CancellationToken.None));

        // Assert
        Assert.Contains("nome completo", excecao.Message);
    }

    [Fact]
    public async Task HandleAsync_DeveLancarExcecao_QuandoArquivoNaoForInformado()
    {
        // Arrange
        var handler = CriarHandlerPadrao();

        var command = CriarCommandValido() with
        {
            ConteudoArquivo = null!
        };

        // Act
        var excecao = await Assert.ThrowsAsync<ArgumentException>(() =>
            handler.HandleAsync(command, CancellationToken.None));

        // Assert
        Assert.Contains("conteudo do arquivo", excecao.Message);
    }

    [Fact]
    public async Task HandleAsync_NaoDeveSalvarCadastroNemPublicarEvento_QuandoArmazenamentoFalhar()
    {
        // Arrange
        var cadastroClienteRepositoryMock = new Mock<ICadastroClienteRepository>();
        var armazenamentoDocumentoServiceMock = new Mock<IArmazenamentoDocumentoService>();
        var eventPublisherMock = new Mock<IEventPublisher>();
        var dateTimeProviderMock = new Mock<IDateTimeProvider>();

        dateTimeProviderMock
            .Setup(x => x.UtcNow)
            .Returns(new DateTime(2026, 6, 30, 10, 0, 0, DateTimeKind.Utc));

        armazenamentoDocumentoServiceMock
            .Setup(x => x.ArmazenarAsync(
                It.IsAny<Stream>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Falha ao armazenar documento."));

        var handler = new CriarCadastroClienteCommandHandler(
            cadastroClienteRepositoryMock.Object,
            armazenamentoDocumentoServiceMock.Object,
            eventPublisherMock.Object,
            dateTimeProviderMock.Object);

        var command = CriarCommandValido();

        // Act
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            handler.HandleAsync(command, CancellationToken.None));

        // Assert
        cadastroClienteRepositoryMock.Verify(x => x.AdicionarAsync(
            It.IsAny<CadastroCliente>(),
            It.IsAny<CancellationToken>()),
            Times.Never);

        eventPublisherMock.Verify(x => x.PublicarAsync(
            It.IsAny<string>(),
            It.IsAny<DocumentoRecebidoEvent>(),
            It.IsAny<CancellationToken>()),
            Times.Never);
    }

    private static CriarCadastroClienteCommandHandler CriarHandlerPadrao()
    {
        var cadastroClienteRepositoryMock = new Mock<ICadastroClienteRepository>();
        var armazenamentoDocumentoServiceMock = new Mock<IArmazenamentoDocumentoService>();
        var eventPublisherMock = new Mock<IEventPublisher>();
        var dateTimeProviderMock = new Mock<IDateTimeProvider>();

        dateTimeProviderMock
            .Setup(x => x.UtcNow)
            .Returns(new DateTime(2026, 6, 30, 10, 0, 0, DateTimeKind.Utc));

        armazenamentoDocumentoServiceMock
            .Setup(x => x.ArmazenarAsync(
                It.IsAny<Stream>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ResultadoArmazenamentoDocumento(
                "documentos-cadastrais",
                "cadastros/123/rg.png"));

        return new CriarCadastroClienteCommandHandler(
            cadastroClienteRepositoryMock.Object,
            armazenamentoDocumentoServiceMock.Object,
            eventPublisherMock.Object,
            dateTimeProviderMock.Object);
    }

    private static CriarCadastroClienteCommand CriarCommandValido()
    {
        var conteudoArquivo = new MemoryStream([1, 2, 3, 4]);

        return new CriarCadastroClienteCommand(
            "Maria Silva",
            "12345678900",
            new DateOnly(1998, 5, 10),
            TipoDocumento.Rg,
            "rg.png",
            "image/png",
            conteudoArquivo.Length,
            conteudoArquivo,
            "correlation-id-teste");
    }
}