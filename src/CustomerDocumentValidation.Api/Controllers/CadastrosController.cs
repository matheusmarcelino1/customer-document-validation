using CustomerDocumentValidation.Api.CadastrosClientes.Requests;
using CustomerDocumentValidation.Api.CadastrosClientes.Responses;
using CustomerDocumentValidation.Application.CadastrosClientes.Commands;
using Microsoft.AspNetCore.Mvc;

namespace CustomerDocumentValidation.Api.Controllers;

[ApiController]
[Route("api/cadastros")]
public sealed class CadastrosController : ControllerBase
{
    private readonly CriarCadastroClienteCommandHandler _criarCadastroClienteCommandHandler;

    public CadastrosController(
        CriarCadastroClienteCommandHandler criarCadastroClienteCommandHandler)
    {
        _criarCadastroClienteCommandHandler = criarCadastroClienteCommandHandler;
    }

    [HttpPost]
    [Consumes("multipart/form-data")]
    [ProducesResponseType(typeof(CriarCadastroClienteApiResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CriarAsync(
        [FromForm] CriarCadastroClienteRequest request,
        CancellationToken cancellationToken)
    {
        if (request.Documento is null || request.Documento.Length == 0)
            return BadRequest("O documento deve ser informado.");

        var correlationId = ObterOuCriarCorrelationId();

        await using var streamDocumento = request.Documento.OpenReadStream();

        var command = new CriarCadastroClienteCommand(
            request.NomeCompleto,
            request.NumeroDocumento,
            request.DataNascimento,
            request.TipoDocumento,
            request.Documento.FileName,
            request.Documento.ContentType,
            request.Documento.Length,
            streamDocumento,
            correlationId);

        var response = await _criarCadastroClienteCommandHandler.HandleAsync(
            command,
            cancellationToken);

        var apiResponse = new CriarCadastroClienteApiResponse(
            response.CadastroClienteId,
            response.Status.ToString(),
            response.Mensagem);

        return Created(
            $"api/cadastros/{response.CadastroClienteId}",
            apiResponse);
    }

    private string ObterOuCriarCorrelationId()
    {
        if (Request.Headers.TryGetValue("X-Correlation-Id", out var correlationId) &&
            !string.IsNullOrWhiteSpace(correlationId))
        {
            return correlationId.ToString();
        }

        return Guid.NewGuid().ToString();
    }
}