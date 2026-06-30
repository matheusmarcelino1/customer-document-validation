using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace CustomerDocumentValidation.Infrastructure.Persistence.MongoDB;

public sealed class CadastroClienteDocument
{
    [BsonId]
    [BsonRepresentation(BsonType.String)]
    public string Id { get; set; } = string.Empty;

    public string NomeCompleto { get; set; } = string.Empty;

    public string NumeroDocumento { get; set; } = string.Empty;

    public DateOnly DataNascimento { get; set; }

    public DocumentoCadastralDocument Documento { get; set; } = new();

    public int Status { get; set; }

    public DateTime CriadoEmUtc { get; set; }

    public DateTime? AtualizadoEmUtc { get; set; }

    public string? MotivoErro { get; set; }
}