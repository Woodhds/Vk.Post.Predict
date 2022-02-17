using Microsoft.Extensions.DependencyInjection;
using Vk.Post.Predict.Persistence.Abstractions;

namespace Vk.Post.Predict.Persistence.Pgsql;

public static class ServiceCollectionExtensions
{
    public static void AddPostgres(this IServiceCollection services, string connectionString)
    {
        services.AddSingleton<IConnectionFactory>(new PostgresConnectionFactory(connectionString));
        services.AddScoped<IMigrateDatabase, MigrateDatabase>();
    }
}
