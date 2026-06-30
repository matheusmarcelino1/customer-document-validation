using System.Text.Json;
using Confluent.Kafka;
using CustomerDocumentValidation.Application.CadastrosClientes.Services;
using Microsoft.Extensions.Options;

namespace CustomerDocumentValidation.Infrastructure.Messaging;

public sealed class KafkaEventPublisher : IEventPublisher, IDisposable
{
    private readonly IProducer<string, string> _producer;

    public KafkaEventPublisher(IOptions<KafkaOptions> options)
    {
        var producerConfig = new ProducerConfig
        {
            BootstrapServers = options.Value.BootstrapServers,
            Acks = Acks.All,
            EnableIdempotence = true
        };

        _producer = new ProducerBuilder<string, string>(producerConfig).Build();
    }

    public async Task PublicarAsync<TEvent>(
        string topico,
        TEvent evento,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(topico))
            throw new ArgumentException("O topico deve ser informado.", nameof(topico));

        ArgumentNullException.ThrowIfNull(evento);

        var payload = JsonSerializer.Serialize(evento);

        var chave = ObterChaveEvento(evento);

        var message = new Message<string, string>
        {
            Key = chave,
            Value = payload
        };

        await _producer.ProduceAsync(
            topico,
            message,
            cancellationToken);
    }

    public void Dispose()
    {
        _producer.Dispose();
    }

    private static string ObterChaveEvento<TEvent>(TEvent evento)
    {
        var propriedadeCadastroClienteId = typeof(TEvent).GetProperty("CadastroClienteId");

        var valor = propriedadeCadastroClienteId?.GetValue(evento)?.ToString();

        return string.IsNullOrWhiteSpace(valor)
            ? Guid.NewGuid().ToString()
            : valor;
    }
}