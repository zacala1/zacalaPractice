using Greeting;
using GreetRouter.Extensions;
using GreetRouter.Infrastructure.Data;
using GreetRouter.Infrastructure.Repositories;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace GreetRouter
{
    public class Startup
    {
        public Startup(IConfiguration configuration, IWebHostEnvironment env)
        {
            Configuration = configuration;
            _env = env;
        }

        public IConfiguration Configuration { get; }

        private readonly IWebHostEnvironment _env;

        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddDbContextWithDefaultOptions<AppDbContext>(_env);
            services.AddScoped<IGreetMemberRepository, GreetMemberRepository>();

            services.AddGrpc();
            services.AddGrpcClientWithDefaultOptions<Greeter.GreeterClient>("greeter01", new System.Uri("http://localhost:5000"));
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
                endpoints.MapGrpcService<GreeterService>();
                endpoints.MapGrpcReflectionService();
            });
            
            PrepareDb.PrepPopulation(app, env.IsProduction());
        }
    }
}
