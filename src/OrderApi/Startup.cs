using MassTransit;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using OrderApi.Data;
using OrderApi.Data.Repositories.MsSql;
using OrderApi.Extensions;
using OrderApi.Services;

namespace OrderApi
{
    public class Startup
    {
        private IConfiguration Configuration { get; }

        public Startup(IWebHostEnvironment env)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", false, true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", true)
                .AddEnvironmentVariables();

            Configuration = builder.Build();
        }


        public void ConfigureServices(IServiceCollection services)
        {
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "OrderAPI", Version = "v1" });
            });

            services.AddControllers();

            services.AddDbContext<WorkshopDbContext>(options =>
            {
                options.UseSqlServer(Configuration.GetConnectionString("WorkshopDb"),
                    sqlOptions =>
                    {
                        sqlOptions.EnableRetryOnFailure(maxRetryCount: 3, maxRetryDelay: System.TimeSpan.FromSeconds(30), errorNumbersToAdd: null);
                    });
            });

            services.ConfigureBus(Configuration);

            services.AddScoped<IOrderService, OrderService>();
            services.AddScoped<IOrderRepository, OrderRepository>();
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseRouting();
            app.UseEndpoints(endpoints => endpoints.MapDefaultControllerRoute());

            app.UseSwagger();
            app.UseSwaggerUI(c => { c.SwaggerEndpoint("/swagger/v1/swagger.json", "OrderAPI V1"); });

            var serviceScopeFactory = app.ApplicationServices.GetRequiredService<IServiceScopeFactory>();
            using (var serviceScope = serviceScopeFactory.CreateScope())
            {
                var dbContext = serviceScope.ServiceProvider.GetService<WorkshopDbContext>();
                dbContext.Database.EnsureCreated();
            }
        }
    }
}
