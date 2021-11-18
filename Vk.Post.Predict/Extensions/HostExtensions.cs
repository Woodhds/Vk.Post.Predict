using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Vk.Post.Predict.Extensions;

public static class HostExtensions
{
    public static IHost MigrateDatabase(this IHost host)
    {
        using (var scope = host.Services.CreateScope())
        {
            scope.ServiceProvider.GetRequiredService<IMigrateDatabase>().Migrate();
        }

        return host;
    }
}
