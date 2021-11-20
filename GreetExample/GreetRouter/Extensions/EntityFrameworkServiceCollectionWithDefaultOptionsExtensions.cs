using FileContextCore;
using FileContextCore.FileManager;
using FileContextCore.Serializer;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace GreetRouter
{
    public static class EntityFrameworkServiceCollectionWithDefaultOptionsExtensions
    {
        public static IServiceCollection AddDbContextWithDefaultOptions<TContext>(this IServiceCollection services, IWebHostEnvironment env) where TContext : DbContext
        {
            if (env.IsProduction())
            {
                services.AddDbContext<TContext>(opt =>
                {
                    // File Context Core
                    // https://github.com/morrisjdev/FileContextCore
                    opt.UseFileContextDatabase<JSONSerializer, DefaultFileManager>();
                });
            }
            else
            {
                services.AddDbContext<TContext>(opt =>
                {
                    opt.UseInMemoryDatabase("InMem");
                });
            }

            return services;
        }
    }
}
