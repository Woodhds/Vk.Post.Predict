using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Vk.Post.Predict.Persistence.Abstractions;

namespace Vk.Post.Predict.Controllers;

[ApiController]
[Route("predict")]
public class PredictController : ControllerBase
{
    private readonly IConnectionFactory _connectionFactory;

    public PredictController(IConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
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
