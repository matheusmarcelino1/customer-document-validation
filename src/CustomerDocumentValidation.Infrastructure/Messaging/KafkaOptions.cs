namespace CustomerDocumentValidation.Infrastructure.Messaging;

public sealed class KafkaOptions
{
    public const string SectionName = "Kafka";

    public string BootstrapServers { get; init; } = string.Empty;

    public string ConsumerGroupId { get; init; } = "customer-document-validation-worker";
}