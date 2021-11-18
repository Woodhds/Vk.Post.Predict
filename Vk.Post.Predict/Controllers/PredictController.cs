using System.Collections.Generic;
using System.Linq;
using System.Net.Mime;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Vk.Post.Predict.Entities;
using Vk.Post.Predict.Services;

namespace Vk.Post.Predict.Controllers;

[ApiController]
[Route("[controller]")]
public class PredictController : ControllerBase
{
    private readonly IConnectionFactory _connectionFactory;
    private readonly IMessageService _messageService;

    public PredictController(
        IConnectionFactory connectionFactory,
        IMessageService messageProvider)
    {
        _connectionFactory = connectionFactory;
        _messageService = messageProvider;
    }

    [HttpGet]
    public async Task<IActionResult> Get()
    {
        var messages = await _messageService.GetMessages();

        return File(JsonSerializer.SerializeToUtf8Bytes(messages, new JsonSerializerOptions
        {
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
        }), MediaTypeNames.Text.Plain, "data.json");
    }


    [HttpPut]
    public async Task<IActionResult> Create([FromBody] Message message)
    {
        await _messageService.Create(message);
        return Ok();
    }

    [HttpDelete]
    public async Task<IActionResult> Delete()
    {
        await using var connection = _connectionFactory.GetConnection();
        await using var command = connection.CreateCommand();
        command.CommandText = @"truncate ""Messages""";
        await command.ExecuteNonQueryAsync();
        return Ok();
    }
}
