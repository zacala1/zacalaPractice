using GreetRouter.Models;
using Microsoft.EntityFrameworkCore;

namespace GreetRouter.Infrastructure.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {

        }

        public DbSet<GreetMember> GreetMembers { get; set; }
    }
}
