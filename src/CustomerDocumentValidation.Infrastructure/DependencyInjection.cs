using Amazon;
using Amazon.Runtime;
using Amazon.S3;
using CustomerDocumentValidation.Application.CadastrosClientes.Repositories;
using CustomerDocumentValidation.Application.CadastrosClientes.Services;
using CustomerDocumentValidation.Infrastructure.Messaging;
using CustomerDocumentValidation.Infrastructure.Persistence.MongoDB;
using CustomerDocumentValidation.Infrastructure.Storage;
using CustomerDocumentValidation.Infrastructure.Time;
using CustomerDocumentValidation.SharedKernel.Time;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace CustomerDocumentValidation.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.Configure<MongoDbOptions>(
            configuration.GetSection(MongoDbOptions.SectionName));

        services.Configure<S3Options>(
            configuration.GetSection(S3Options.SectionName));

        services.Configure<KafkaOptions>(
            configuration.GetSection(KafkaOptions.SectionName));

        services.AddSingleton<IDateTimeProvider, DateTimeProvider>();

        services.AddScoped<ICadastroClienteRepository, CadastroClienteMongoRepository>();

        services.AddScoped<IArmazenamentoDocumentoService, ArmazenamentoDocumentoS3Service>();

        services.AddSingleton<IEventPublisher, KafkaEventPublisher>();

        services.AddSingleton<IAmazonS3>(serviceProvider =>
        {
            var options = configuration
                .GetSection(S3Options.SectionName)
                .Get<S3Options>()!;

            var credentials = new BasicAWSCredentials(
                options.AccessKey,
                options.SecretKey);

            var config = new AmazonS3Config
            {
                ServiceURL = options.ServiceUrl,
                ForcePathStyle = options.ForcePathStyle,
                AuthenticationRegion = RegionEndpoint.USEast1.SystemName
            };

            return new AmazonS3Client(credentials, config);
        });

        return services;
    }
}