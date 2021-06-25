using Microsoft.EntityFrameworkCore;
using Vk.Post.Predict.Entities;

namespace Vk.Post.Predict
{
    public interface IMigrateDatabase
    {
        void Migrate();
    }
    
    public class MigrateDatabase : IMigrateDatabase
    {
        private readonly DataContext _dataContext;

        public MigrateDatabase(IDbContextFactory<DataContext> dbContextFactory)
        {
            _dataContext = dbContextFactory.CreateDbContext();
        }
        
        public void Migrate()
        {
            _dataContext.Database.Migrate();
        }
    }
}