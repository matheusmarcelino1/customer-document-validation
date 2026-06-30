using CustomerDocumentValidation.Application.CadastrosClientes.Commands;
using Microsoft.Extensions.DependencyInjection;

namespace CustomerDocumentValidation.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<CriarCadastroClienteCommandHandler>();

        return services;
    }
}