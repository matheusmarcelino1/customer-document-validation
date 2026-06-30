using CustomerDocumentValidation.Domain.CadastrosClientes.Entities;
using CustomerDocumentValidation.Domain.CadastrosClientes.Enums;

namespace CustomerDocumentValidation.Domain.Tests.CadastrosClientes;

public sealed class DocumentoCadastralTestes
{
    [Fact]
    public void Criar_DeveCriarDocumento_QuandoDadosForemValidos()
    {
        // Act
        var documento = DocumentoCadastral.Criar(
            TipoDocumento.Cnh,
            "cnh.png",
            "image/png",
            2048,
            "documentos-cadastrais",
            "cadastros/123/cnh.png");

        // Assert
        Assert.False(string.IsNullOrWhiteSpace(documento.Id));
        Assert.Equal(TipoDocumento.Cnh, documento.Tipo);
        Assert.Equal("cnh.png", documento.NomeArquivoOriginal);
        Assert.Equal("image/png", documento.TipoConteudo);
        Assert.Equal(2048, documento.TamanhoEmBytes);
        Assert.Equal("documentos-cadastrais", documento.Bucket);
        Assert.Equal("cadastros/123/cnh.png", documento.ChaveArquivo);
    }

    [Fact]
    public void Criar_DeveLancarExcecao_QuandoTipoDocumentoForDesconhecido()
    {
        // Act
        var excecao = Assert.Throws<ArgumentException>(() =>
            DocumentoCadastral.Criar(
                TipoDocumento.Desconhecido,
                "documento.png",
                "image/png",
                1024,
                "documentos-cadastrais",
                "cadastros/123/documento.png"));

        // Assert
        Assert.Contains("tipo do documento", excecao.Message);
    }

    [Fact]
    public void Criar_DeveLancarExcecao_QuandoTamanhoArquivoForInvalido()
    {
        // Act
        var excecao = Assert.Throws<ArgumentException>(() =>
            DocumentoCadastral.Criar(
                TipoDocumento.Rg,
                "rg.png",
                "image/png",
                0,
                "documentos-cadastrais",
                "cadastros/123/rg.png"));

        // Assert
        Assert.Contains("tamanho do arquivo", excecao.Message);
    }

    [Fact]
    public void Criar_DeveLancarExcecao_QuandoBucketForVazio()
    {
        // Act
        var excecao = Assert.Throws<ArgumentException>(() =>
            DocumentoCadastral.Criar(
                TipoDocumento.Rg,
                "rg.png",
                "image/png",
                1024,
                "",
                "cadastros/123/rg.png"));

        // Assert
        Assert.Contains("bucket", excecao.Message);
    }

    [Fact]
    public void Criar_DeveLancarExcecao_QuandoChaveArquivoForVazia()
    {
        // Act
        var excecao = Assert.Throws<ArgumentException>(() =>
            DocumentoCadastral.Criar(
                TipoDocumento.Rg,
                "rg.png",
                "image/png",
                1024,
                "documentos-cadastrais",
                ""));

        // Assert
        Assert.Contains("chave do arquivo", excecao.Message);
    }
}