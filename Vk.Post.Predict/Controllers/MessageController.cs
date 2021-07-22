using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Vk.Post.Predict.Models;

namespace Vk.Post.Predict.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class MessageController : ControllerBase
    {
        private readonly IMessageUpdateService _messageUpdateService;

        public MessageController(IMessageUpdateService messageUpdateService)
        {
            _messageUpdateService = messageUpdateService;
        }

        [HttpPost]
        public async Task<int> UpdateOwnerNames([FromBody] IEnumerable<UpdateMessageOwner> owners)
        {
            return await _messageUpdateService.UpdateMessageOwners(owners);
        }
    }
}
