using System.Net.Mime;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Vk.Post.Predict.Abstractions;

namespace Vk.Post.Predict.Controllers;

[ApiController]
[Route("message")]
public class MessageController : ControllerBase
{
    private readonly IMessageService _messageService;

    public MessageController(IMessageService messageUpdateService)
        => _messageService = messageUpdateService;

    [HttpGet]
    public async Task<IActionResult> Get()
    {
        var messages = await _messageService.GetMessages();

        return File(
            JsonSerializer.SerializeToUtf8Bytes(messages,
                new JsonSerializerOptions
                {
                    Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
                }),
            MediaTypeNames.Application.Json, "data.json");
    }
}
