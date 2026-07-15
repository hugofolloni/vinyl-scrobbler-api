using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using ScrobblerApi.Models.DTOs;

namespace ScrobblerApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AlexaController : ControllerBase
{
    [HttpPost("webhook")]
    public IActionResult ReceiveRequest([FromBody] JsonElement jsonRequest)
    {
        try
        {
            var root = jsonRequest.GetProperty("request");
            var requestType = root.GetProperty("type").GetString();

            if (requestType == "SessionEndedRequest") return Ok();

            if (requestType == "LaunchRequest")
            {
                return Ok(CreateResponse("Bem-vindo ao toca discos. Qual vinil você vai registrar?", false));
            }

            if (requestType == "IntentRequest")
            {
                var intent = root.GetProperty("intent");
                var intentName = intent.GetProperty("name").GetString();

                if (intentName == "ScrobblarVinilIntent")
                {
                    string confirmationStatus = "NONE";
                    if (intent.TryGetProperty("confirmationStatus", out var confStatusProp))
                    {
                        confirmationStatus = confStatusProp.GetString() ?? "NONE";
                    }

                    if (confirmationStatus == "DENIED")
                    {
                        return Ok(CreateResponse("Tudo bem, scrobble cancelado.", true));
                    }

                    string vinylName = "";
                    string groupName = "";
                    
                    if (intent.TryGetProperty("slots", out JsonElement slots))
                    {
                        if (slots.TryGetProperty("vinylName", out JsonElement vSlot) && vSlot.TryGetProperty("value", out JsonElement vVal))
                            vinylName = vVal.GetString() ?? "";

                        if (slots.TryGetProperty("groupName", out JsonElement gSlot) && gSlot.TryGetProperty("value", out JsonElement gVal))
                            groupName = gVal.GetString() ?? "";
                    }

                    if (confirmationStatus == "NONE" && !string.IsNullOrEmpty(vinylName))
                    {
                        return Ok(CreateDelegateResponse());
                    }

                    return Ok(CreateResponse($"Feito! Registrei o disco {vinylName} do {groupName}.", true));
                }
            }

            return Ok(CreateResponse("Desculpe, não reconheci essa ação.", true));
        }
        catch (Exception ex)
        {
            Console.WriteLine($"\n[ERRO FATAL NO C#] {ex.Message}\n{ex.StackTrace}\n");
            return Ok(CreateResponse("Deu algum erro interno no código do backend.", true));
        }
    }

    private AlexaResponse CreateResponse(string speechText, bool endSession)
    {
        var response = new AlexaResponse();
        response.Response.OutputSpeech.Text = speechText;
        response.Response.ShouldEndSession = endSession;
        return response;
    }

    private dynamic CreateDelegateResponse()
    {
        return new
        {
            version = "1.0",
            response = new { directives = new[] { new { type = "Dialog.Delegate" } }, shouldEndSession = false }
        };
    }
}