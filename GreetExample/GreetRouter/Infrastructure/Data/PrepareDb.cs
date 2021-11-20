using GreetRouter.Models;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GreetRouter.Infrastructure.Data
{
    public static class PrepareDb
    {
        public static void PrepPopulation(IApplicationBuilder app, bool isProd)
        {
            using var serviceScope = app.ApplicationServices.CreateScope();
            SeedData(serviceScope.ServiceProvider.GetService<AppDbContext>(), isProd);
        }

        private static void SeedData(AppDbContext context, bool isProd)
        {
            if (isProd)
            {
                Console.WriteLine("--> Attempting to apply migrations...");
                try
                {
                    context.Database.Migrate();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"--> Could not run migrations: {ex.Message}");
                }
            }

            if (!context.GreetMembers.Any())
            {
                Console.WriteLine("--> Seeding Data...");

                context.GreetMembers.AddRange(
                    new GreetMember() { Id = 1, Name = "Dot Net"},
                    new GreetMember() { Id = 2, Name = "SQL Server Express" },
                    new GreetMember() { Id = 3, Name = "Kubernetes" }
                );

                context.SaveChanges();
            }
            else
            {
                Console.WriteLine("--> We already have data");
            }
        }
    }
}
