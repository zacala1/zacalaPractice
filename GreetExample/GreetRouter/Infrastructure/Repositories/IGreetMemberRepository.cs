using GreetRouter.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GreetRouter.Infrastructure.Repositories
{
    public interface IGreetMemberRepository
    {
        void CreateGreetMember(GreetMember value);
        IEnumerable<GreetMember> GetAllGreetMembers();
        GreetMember GetGreetMemberById(int id);

        bool SaveChanges();
    }
}
