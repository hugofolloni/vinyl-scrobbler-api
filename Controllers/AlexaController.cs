using Microsoft.AspNetCore.Mvc;
using ScrobblerApi.Models.DTOs;
using ScrobblerApi.Services;

namespace ScrobblerApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AlexaController : ControllerBase
{
    private readonly IScrobbleService _scrobbleService;

    // Injeção de dependência do serviço
    public AlexaController(IScrobbleService scrobbleService)
    {
        _scrobbleService = scrobbleService;
    }

    [HttpPost("webhook")]
    public async Task<IActionResult> ReceiveRequest([FromBody] AlexaRequest request)
    {
        // 1. GUARDIÃO 1: Se a sessão está sendo encerrada ou deu erro na Amazon, temos que ficar em silêncio.
        if (request.Request.Type == "SessionEndedRequest")
        {
            return Ok(); // O 200 OK vazio evita o INVALID_RESPONSE
        }

        // 2. GUARDIÃO 2: O usuário disse apenas "Alexa, abra toca discos inteligente"
        if (request.Request.Type == "LaunchRequest")
        {
            return Ok(CreateResponse("Bem-vindo ao toca discos. Qual vinil você vai ouvir agora?", false));
        }

        // 3. FLUXO PRINCIPAL: Intent de Scrobble
        if (request.Request.Type == "IntentRequest" && request.Request.Intent?.Name == "ScrobblarVinilIntent")
        {
            var intent = request.Request.Intent;
            var slots = intent.Slots;

            if (intent.ConfirmationStatus == "DENIED")
            {
                return Ok(CreateResponse("Tudo bem. O scrobble foi cancelado.", true));
            }

            string vinylName = slots != null && slots.ContainsKey("vinylName") ? slots["vinylName"].Value : string.Empty;
            string groupName = slots != null && slots.ContainsKey("groupName") ? slots["groupName"].Value : string.Empty;

            if (!string.IsNullOrEmpty(vinylName))
            {
                if (intent.ConfirmationStatus == "NONE")
                {
                    return Ok(CreateDelegateResponse());
                }

                // Logamos no terminal para validar
                Console.WriteLine($"\n========================================");
                Console.WriteLine($"[SUCESSO] A Alexa bateu no localhost!");
                Console.WriteLine($"[DADOS] Vinil: {vinylName} | Artista: {groupName}");
                Console.WriteLine($"========================================\n");

                return Ok(CreateResponse($"Tudo certo! Loguei o disco {vinylName}, do {groupName} no terminal.", true));
            }
        }

        // 4. FALLBACK: Se ela mandou uma IntentRequest que não conhecemos ou veio sem dados
        return Ok(CreateResponse("Desculpe, não consegui entender o nome do vinil.", true));
    }
    
    // Método auxiliar para criar respostas faladas limpas
    private AlexaResponse CreateResponse(string speechText, bool endSession)
    {
        var response = new AlexaResponse();
        response.Response.OutputSpeech.Text = speechText;
        response.Response.ShouldEndSession = endSession;
        return response;
    }

    // Método auxiliar para delegar a conversa de volta para a Alexa (para ela fazer a pergunta de confirmação)
    private dynamic CreateDelegateResponse()
    {
        return new
        {
            version = "1.0",
            response = new
            {
                directives = new[]
                {
                    new { type = "Dialog.Delegate" }
                },
                shouldEndSession = false
            }
        };
    }
}