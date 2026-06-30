using CustomerDocumentValidation.Domain.CadastrosClientes.Entidades;
using CustomerDocumentValidation.Domain.CadastrosClientes.Enums;

namespace CustomerDocumentValidation.Domain.Tests.CadastrosClientes;

public sealed class CadastroClienteTestes
{
    [Fact]
    public void Criar_DeveCriarCadastro_ComStatusPendenteProcessamento()
    {
        // Arrange
        var documento = CriarDocumentoValido();
        var agora = DateTime.UtcNow;

        // Act
        var cadastro = CadastroCliente.Criar(
            "Maria Silva",
            "12345678900",
            new DateOnly(1998, 5, 10),
            documento,
            agora);

        // Assert
        Assert.False(string.IsNullOrWhiteSpace(cadastro.Id));
        Assert.Equal("Maria Silva", cadastro.NomeCompleto);
        Assert.Equal("12345678900", cadastro.NumeroDocumento);
        Assert.Equal(StatusProcessamento.PendenteProcessamento, cadastro.Status);
        Assert.Equal(agora, cadastro.CriadoEmUtc);
        Assert.Null(cadastro.AtualizadoEmUtc);
        Assert.Null(cadastro.MotivoErro);
    }

    [Fact]
    public void Criar_DeveLancarExcecao_QuandoNomeCompletoForVazio()
    {
        // Arrange
        var documento = CriarDocumentoValido();

        // Act
        var excecao = Assert.Throws<ArgumentException>(() =>
            CadastroCliente.Criar(
                "",
                "12345678900",
                new DateOnly(1998, 5, 10),
                documento,
                DateTime.UtcNow));

        // Assert
        Assert.Contains("nome completo", excecao.Message);
    }

    [Fact]
    public void Criar_DeveLancarExcecao_QuandoNumeroDocumentoForVazio()
    {
        // Arrange
        var documento = CriarDocumentoValido();

        // Act
        var excecao = Assert.Throws<ArgumentException>(() =>
            CadastroCliente.Criar(
                "Maria Silva",
                "",
                new DateOnly(1998, 5, 10),
                documento,
                DateTime.UtcNow));

        // Assert
        Assert.Contains("numero do documento", excecao.Message);
    }

    [Fact]
    public void Criar_DeveLancarExcecao_QuandoDataNascimentoEstiverNoFuturo()
    {
        // Arrange
        var documento = CriarDocumentoValido();
        var agora = new DateTime(2026, 6, 30, 10, 0, 0, DateTimeKind.Utc);
        var dataNascimentoFutura = new DateOnly(2026, 7, 1);

        // Act
        var excecao = Assert.Throws<ArgumentException>(() =>
            CadastroCliente.Criar(
                "Maria Silva",
                "12345678900",
                dataNascimentoFutura,
                documento,
                agora));

        // Assert
        Assert.Contains("data de nascimento nao pode estar no futuro", excecao.Message);
    }

    [Fact]
    public void MarcarComoEmProcessamento_DeveAtualizarStatusParaEmProcessamento()
    {
        // Arrange
        var cadastro = CriarCadastroValido();
        var atualizadoEm = DateTime.UtcNow;

        // Act
        cadastro.MarcarComoEmProcessamento(atualizadoEm);

        // Assert
        Assert.Equal(StatusProcessamento.EmProcessamento, cadastro.Status);
        Assert.Equal(atualizadoEm, cadastro.AtualizadoEmUtc);
        Assert.Null(cadastro.MotivoErro);
    }

    [Fact]
    public void MarcarComoProcessadoComSucesso_DeveAtualizarStatus_QuandoCadastroEstiverEmProcessamento()
    {
        // Arrange
        var cadastro = CriarCadastroValido();
        cadastro.MarcarComoEmProcessamento(DateTime.UtcNow);

        var atualizadoEm = DateTime.UtcNow.AddMinutes(1);

        // Act
        cadastro.MarcarComoProcessadoComSucesso(atualizadoEm);

        // Assert
        Assert.Equal(StatusProcessamento.ProcessadoComSucesso, cadastro.Status);
        Assert.Equal(atualizadoEm, cadastro.AtualizadoEmUtc);
        Assert.Null(cadastro.MotivoErro);
    }

    [Fact]
    public void MarcarComoProcessadoComSucesso_DeveLancarExcecao_QuandoCadastroNaoEstiverEmProcessamento()
    {
        // Arrange
        var cadastro = CriarCadastroValido();

        // Act
        var excecao = Assert.Throws<InvalidOperationException>(() =>
            cadastro.MarcarComoProcessadoComSucesso(DateTime.UtcNow));

        // Assert
        Assert.Contains("cadastro em processamento", excecao.Message);
    }

    [Fact]
    public void MarcarComoProcessadoComErro_DeveAtualizarStatusEMotivoErro()
    {
        // Arrange
        var cadastro = CriarCadastroValido();
        var atualizadoEm = DateTime.UtcNow;

        // Act
        cadastro.MarcarComoProcessadoComErro("Documento nao encontrado no storage.", atualizadoEm);

        // Assert
        Assert.Equal(StatusProcessamento.ProcessadoComErro, cadastro.Status);
        Assert.Equal("Documento nao encontrado no storage.", cadastro.MotivoErro);
        Assert.Equal(atualizadoEm, cadastro.AtualizadoEmUtc);
    }

    private static CadastroCliente CriarCadastroValido()
    {
        return CadastroCliente.Criar(
            "Maria Silva",
            "12345678900",
            new DateOnly(1998, 5, 10),
            CriarDocumentoValido(),
            DateTime.UtcNow);
    }

    private static DocumentoCadastral CriarDocumentoValido()
    {
        return DocumentoCadastral.Criar(
            TipoDocumento.Rg,
            "rg.png",
            "image/png",
            1024,
            "documentos-cadastrais",
            "cadastros/123/rg.png");
    }
}