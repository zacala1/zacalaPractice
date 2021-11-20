using GrpcModel.ProtoFirst;
using GrpcRouter.ProtoFirst;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;

namespace GrpcRouter.ProtoFirst
{
    public class Startup
    {
        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddGrpc();
            services.AddGrpcClient<WeatherForecaster.WeatherForecasterClient>("ForecastClient01", o =>
            {
                o.Address = new Uri("http://localhost:5000");
            }).EnableCallContextPropagation();
            services.AddGrpcClient<WeatherForecaster.WeatherForecasterClient>("ForecastClient02", o =>
            {
                o.Address = new Uri("http://localhost:5000");
            }).EnableCallContextPropagation();
            services.AddGrpcReflection();
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
                // Map Grpc Service
                endpoints.MapGrpcService<WeatherService>();

                endpoints.MapGet("/", async context =>
                {
                    await context.Response.WriteAsync("Communication with gRPC endpoints must be made through a gRPC client. To learn how to create a client, visit: https://go.microsoft.com/fwlink/?linkid=2086909");
                });
                // Map Grpc Server Reflection when debugging
                if (env.IsDevelopment()) endpoints.MapGrpcReflectionService();
            });
        }
    }
}
