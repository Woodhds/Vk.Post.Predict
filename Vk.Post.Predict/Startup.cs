using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpLogging;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.ML;
using Vk.Post.Predict.Abstractions;
using Vk.Post.Predict.Models;
using Vk.Post.Predict.Persistence.Pgsql;
using Vk.Post.Predict.Services;
using Vk.Post.Predict.Services.Abstractions;

namespace Vk.Post.Predict
{
    public class Startup
    {
        private IConfiguration Configuration { get; }

        public Startup(IConfiguration configuration)
            => Configuration = configuration;

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers();
            services.AddScoped<IMessageService, MessageService>();
            services.AddPredictionEnginePool<VkMessageML, VkMessagePredict>()
                .FromUri("https://github.com/Woodhds/Vk.Post.Model/raw/master/Model.zip", TimeSpan.FromDays(1));

            var connUrl = Environment.GetEnvironmentVariable("DATABASE_URL");
            // Parse connection URL to connection string for Npgsql

            services.AddPostgres(string.IsNullOrEmpty(connUrl)
                ? Configuration.GetConnectionString("DataContext")
                : GetConnectionString(connUrl));

            services.AddLogging();

            services.AddGrpcReflection();

            services.AddGrpc(x =>
                {
                    x.EnableDetailedErrors = true;
                })
                .AddJsonTranscoding();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                endpoints.MapGrpcService<Services.MessagePredictService>();
            });
        }

        string GetConnectionString(string connUrl)
        {
            connUrl = connUrl?.Replace("postgres://", string.Empty);
            var pgUserPass = connUrl.Split("@")[0];
            var pgHostPortDb = connUrl.Split("@")[1];
            var pgHostPort = pgHostPortDb.Split("/")[0];
            var pgDb = pgHostPortDb.Split("/")[1];
            var pgUser = pgUserPass.Split(":")[0];
            var pgPass = pgUserPass.Split(":")[1];
            var pgHost = pgHostPort.Split(":")[0];
            var pgPort = pgHostPort.Split(":")[1];
            return
                $"Server={pgHost};Port={pgPort};User Id={pgUser};Password={pgPass};Database={pgDb};Pooling=true;SSL Mode=Require;Trust Server Certificate=true";
        }
    }
}
