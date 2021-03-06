using Entities;
using GreenPipes;
using MassTransit;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Receiver
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; set; }
        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMassTransit(x =>
            {
                x.AddConsumer<MessageConsumer>();
                x.AddBus(provider => Bus.Factory.CreateUsingRabbitMq(cfg =>
                {
                    cfg.UseHealthCheck(provider);
                    cfg.Host(Configuration.GetSection("RabbitMQ:Host:Url").Get<string>(), h =>
                    {
                        h.Username(Configuration.GetSection("RabbitMQ:Host:UserName").Get<string>());
                        h.Password(Configuration.GetSection("RabbitMQ:Host:Password").Get<string>());
                    });

                    cfg.ReceiveEndpoint(Configuration.GetSection("RabbitMQ:Queue").Get<string>(), e =>
                    {
                        e.PrefetchCount = 16;
                        e.UseMessageRetry(r => r.Interval(2, 100));
                        e.ConfigureConsumer<MessageConsumer>(provider);
                    });
                }));
            });

            services.AddMassTransitHostedService();
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
                endpoints.MapGet("/", async context =>
                {
                    await context.Response.WriteAsync("Hello World!");
                });
            });
        }
    }
}
