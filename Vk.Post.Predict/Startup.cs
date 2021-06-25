using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.ML;
using Microsoft.OpenApi.Models;
using Vk.Post.Predict.Entities;
using IHostEnvironment = Microsoft.ML.Runtime.IHostEnvironment;

namespace Vk.Post.Predict
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers();
            services.AddTransient<IMigrateDatabase, MigrateDatabase>();
            services.AddPredictionEnginePool<VkMessageML, VkMessagePredict>()
                .FromUri("https://github.com/Woodhds/Vk.Post.Model/raw/master/Model.zip", TimeSpan.FromDays(1));

            services.AddDbContextFactory<DataContext>(
                x => x.UseNpgsql(Environment.GetEnvironmentVariable("DATABASE_URL") ??
                                 Configuration.GetConnectionString("DataContext")));
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseEndpoints(endpoints => { endpoints.MapControllers(); });

            //app.ApplicationServices.GetRequiredService<IMigrateDatabase>().Migrate();
        }
    }
}