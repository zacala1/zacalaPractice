using GreetRouter.Infrastructure.Data;
using GreetRouter.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GreetRouter.Infrastructure.Repositories
{
    public class GreetMemberRepository : IGreetMemberRepository
    {
        private readonly AppDbContext _context;

        public GreetMemberRepository(AppDbContext context)
        {
            _context = context;
        }
        public void CreateGreetMember(GreetMember value)
        {
            if (value is null) throw new ArgumentNullException(nameof(value));
            _context.GreetMembers.Add(value);
        }

        public IEnumerable<GreetMember> GetAllGreetMembers()
        {
            return _context.GreetMembers.ToList();
        }

        public GreetMember GetGreetMemberById(int id)
        {
            return _context.GreetMembers.FirstOrDefault(p => p.Id.Equals(id));
        }

        public bool SaveChanges()
        {
            return (_context.SaveChanges() >= 0);
        }
    }
}
