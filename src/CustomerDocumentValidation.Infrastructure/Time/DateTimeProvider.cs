using CustomerDocumentValidation.SharedKernel.Time;

namespace CustomerDocumentValidation.Infrastructure.Time;

public sealed class DateTimeProvider : IDateTimeProvider
{
    public DateTime UtcNow => DateTime.UtcNow;
}