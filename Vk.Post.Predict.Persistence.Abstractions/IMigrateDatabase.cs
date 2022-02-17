using System.Threading.Tasks;

namespace Vk.Post.Predict.Persistence.Abstractions;

public interface IMigrateDatabase
{
    Task MigrateAsync();
}
