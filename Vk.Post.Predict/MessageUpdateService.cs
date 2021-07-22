using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Vk.Post.Predict.Entities;
using Vk.Post.Predict.Models;

namespace Vk.Post.Predict
{
    public interface IMessageUpdateService
    {
        /// <summary>
        /// Update Owner's names by id
        /// </summary>
        /// <param name="owners"></param>
        /// <returns>Count affected rows</returns>
        ValueTask<int> UpdateMessageOwners(IEnumerable<UpdateMessageOwner> owners);
    }

    public class MessageUpdateService : IMessageUpdateService
    {
        private readonly DataContext _dataContext;

        public MessageUpdateService(IDbContextFactory<DataContext> _contextFactory)
        {
            _dataContext = _contextFactory.CreateDbContext();
        }

        public async ValueTask<int> UpdateMessageOwners(IEnumerable<UpdateMessageOwner> owners)
        {
            if (owners == null || !owners.Any())
            {
                return 1;
            }

            var ownerDictionary = owners.ToDictionary(f => f.Id, f => f.Name);
            var ownerIds = ownerDictionary.Select(f => f.Key);


            var messages = await _dataContext.Messages.Where(f => ownerIds.Contains(f.OwnerId)).ToArrayAsync();

            foreach (var message in messages)
            {
                message.OwnerName = ownerDictionary[message.OwnerId];
            }

            return messages.Length;
        }
    }
}
