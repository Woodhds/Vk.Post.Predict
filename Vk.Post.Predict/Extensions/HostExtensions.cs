using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Vk.Post.Predict.Persistence.Abstractions;

namespace Vk.Post.Predict.Extensions;

public static class HostExtensions
{
    public static async Task MigrateDatabase(this IHost host)
    {
        using var scope = host.Services.CreateScope();
        await scope.ServiceProvider.GetRequiredService<IMigrateDatabase>().MigrateAsync();
    }
}
