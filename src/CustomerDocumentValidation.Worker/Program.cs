using CustomerDocumentValidation.Infrastructure;
using CustomerDocumentValidation.Worker.Services;
using CustomerDocumentValidation.Worker.Workers;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddInfrastructure(builder.Configuration);

builder.Services.AddScoped<ProcessarDocumentoRecebidoService>();

builder.Services.AddHostedService<DocumentoRecebidoWorker>();

var host = builder.Build();

host.Run();