using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Vk.Post.Predict.Entities;
using Vk.Post.Predict.Persistence.Abstractions;
using Vk.Post.Predict.Services;
using Vk.Post.Predict.Services.Abstractions;

namespace Vk.Post.Predict.Controllers;

[ApiController]
[Route("[controller]")]
public class PredictController : ControllerBase
{
    private readonly IConnectionFactory _connectionFactory;
    private readonly IMessageService _messageService;
    private readonly IMessagePredictService _messagePredictService;

    public PredictController(
        IConnectionFactory connectionFactory,
        IMessageService messageProvider,
        IMessagePredictService messagePredictService)
    {
        _connectionFactory = connectionFactory;
        _messageService = messageProvider;
        _messagePredictService = messagePredictService;
    }

    [HttpPost]
    public async Task<IActionResult> Predict([FromBody] MessagePredictRequest[] request, CancellationToken ct = default)
    {
        return Ok(await _messagePredictService.Predict(request, ct));
    }

    [HttpPost("{ownerId:int}/{id:int}")]
    public IActionResult Predict(int ownerId, int id, [FromBody]MessagePredictRequest message)
    {

        return Ok(_messagePredictService.Predict(new MessagePredictRequest(ownerId, id, message.Text)));
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
        await connection.OpenAsync();
        await command.ExecuteNonQueryAsync();
        return Ok();
    }
}
