using System;
using System.Threading.Tasks;
using Grpc.Core;
using Microsoft.EntityFrameworkCore;
using Vk.Post.Predict.Entities;

namespace Vk.Post.Predict
{
    public class MessageService : MessageSaveService.MessageSaveServiceBase
    {
        private readonly DataContext _context;

        public MessageService(IDbContextFactory<DataContext> factory)
        {
            _context = factory.CreateDbContext();
        }

        public override async Task<MessageSaveResponse> SaveMessage(MessageSaveRequest request,
            ServerCallContext context)
        {
            if (await _context.Messages.AnyAsync(q => q.OwnerId == request.OwnerId && q.Id == request.Id))
            {
                return new MessageSaveResponse
                {
                    Success = false
                };
            }

            try
            {
                await _context.Messages.AddAsync(new Message
                {
                    Category = request.Category,
                    Id = request.Id,
                    Text = request.Text,
                    OwnerId = request.OwnerId
                });
                await _context.SaveChangesAsync();
            }
            catch (Exception)
            {
                return new MessageSaveResponse
                {
                    Success = false
                };
            }

            return new MessageSaveResponse
            {
                Success = true
            };
        }
    }
}