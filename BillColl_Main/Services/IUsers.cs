using BillColl_Main.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BillColl_Main.Services
{
    public interface IUsers
    {
        User GetUser(string Email);
    }
}
