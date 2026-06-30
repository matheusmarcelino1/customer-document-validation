namespace CustomerDocumentValidation.Application.CadastrosClientes.Services;

public interface IEventPublisher
{
    Task PublicarAsync<TEvent>(
        string topico,
        TEvent evento,
        CancellationToken cancellationToken);
}