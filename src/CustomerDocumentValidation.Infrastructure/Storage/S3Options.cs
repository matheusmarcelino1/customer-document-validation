namespace CustomerDocumentValidation.Infrastructure.Storage;

public sealed class S3Options
{
    public const string SectionName = "S3";

    public string BucketName { get; init; } = string.Empty;

    public string ServiceUrl { get; init; } = string.Empty;

    public string AccessKey { get; init; } = string.Empty;

    public string SecretKey { get; init; } = string.Empty;

    public bool ForcePathStyle { get; init; } = true;
}