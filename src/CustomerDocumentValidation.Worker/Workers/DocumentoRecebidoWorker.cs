using System.Text.Json;
using Confluent.Kafka;
using CustomerDocumentValidation.Application.CadastrosClientes.Events;
using CustomerDocumentValidation.Application.CadastrosClientes.Topics;
using CustomerDocumentValidation.Infrastructure.Messaging;
using CustomerDocumentValidation.Worker.Services;
using Microsoft.Extensions.Options;

namespace CustomerDocumentValidation.Worker.Workers;

public sealed class DocumentoRecebidoWorker : BackgroundService
{
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private readonly KafkaOptions _kafkaOptions;
    private readonly ILogger<DocumentoRecebidoWorker> _logger;

    public DocumentoRecebidoWorker(
        IServiceScopeFactory serviceScopeFactory,
        IOptions<KafkaOptions> kafkaOptions,
        ILogger<DocumentoRecebidoWorker> logger)
    {
        _serviceScopeFactory = serviceScopeFactory;
        _kafkaOptions = kafkaOptions.Value;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var consumerConfig = new ConsumerConfig
        {
            BootstrapServers = _kafkaOptions.BootstrapServers,
            GroupId = _kafkaOptions.ConsumerGroupId,
            AutoOffsetReset = AutoOffsetReset.Earliest,
            EnableAutoCommit = false
        };

        using var consumer = new ConsumerBuilder<string, string>(consumerConfig)
            .Build();

        consumer.Subscribe(TopicosMensageria.DocumentoRecebido);

        _logger.LogInformation(
            "Worker iniciado. Consumindo topico {Topico} com grupo {Grupo}.",
            TopicosMensageria.DocumentoRecebido,
            _kafkaOptions.ConsumerGroupId);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var resultadoConsumo = consumer.Consume(stoppingToken);

                if (resultadoConsumo?.Message is null)
                    continue;

                await ProcessarMensagemAsync(
                    resultadoConsumo,
                    stoppingToken);

                consumer.Commit(resultadoConsumo);
            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation("Worker cancelado.");
                break;
            }
            catch (ConsumeException ex)
            {
                _logger.LogError(
                    ex,
                    "Erro ao consumir mensagem do Kafka.");
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Erro inesperado no Worker.");
            }
        }

        consumer.Close();
    }

    private async Task ProcessarMensagemAsync(
        ConsumeResult<string, string> resultadoConsumo,
        CancellationToken cancellationToken)
    {
        var payload = resultadoConsumo.Message.Value;

        if (string.IsNullOrWhiteSpace(payload))
        {
            _logger.LogWarning("Mensagem recebida com payload vazio.");
            return;
        }

        DocumentoRecebidoEvent? evento;

        try
        {
            evento = JsonSerializer.Deserialize<DocumentoRecebidoEvent>(
                payload,
                new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
        }
        catch (JsonException ex)
        {
            _logger.LogError(
                ex,
                "Erro ao desserializar evento. Payload: {Payload}",
                payload);

            return;
        }

        if (evento is null)
        {
            _logger.LogWarning(
                "Evento desserializado como nulo. Payload: {Payload}",
                payload);

            return;
        }

        using var scope = _serviceScopeFactory.CreateScope();

        var processador = scope.ServiceProvider
            .GetRequiredService<ProcessarDocumentoRecebidoService>();

        await processador.ProcessarAsync(
            evento,
            cancellationToken);
    }
}