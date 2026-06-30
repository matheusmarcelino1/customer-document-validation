using CustomerDocumentValidation.Domain.CadastrosClientes.Entities;

namespace CustomerDocumentValidation.Application.CadastrosClientes.Repositories;

public interface ICadastroClienteRepository
{
    Task AdicionarAsync(
        CadastroCliente cadastroCliente,
        CancellationToken cancellationToken);

    Task<CadastroCliente?> ObterPorIdAsync(
        string cadastroClienteId,
        CancellationToken cancellationToken);

    Task AtualizarAsync(
        CadastroCliente cadastroCliente,
        CancellationToken cancellationToken);
}