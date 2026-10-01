using BillColl_Main.AppDbContext;
using BillColl_Main.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BillColl_Main.Services
{
    public class UserSQLRepository : IUsers
    {
        private readonly myDBContext context;

        public UserSQLRepository(myDBContext context)
        {
            this.context = context;
        }
        public User GetUser(string Email)
        {
            return context.Users.FirstOrDefault(e => e.Email == Email);
        }
    }
}
