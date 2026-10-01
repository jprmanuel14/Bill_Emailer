using BillColl_Main.Models;
using System.Collections.Generic;

namespace BillColl_Main.Services
{
    public interface IUserRole
    {
        IEnumerable<Nre_Uer_New> GetUserRoles();
        Nre_Uer_New GetUserRole(string id);
        void AddUserRole(Nre_Uer_New userRole, string actionTaker, string actionTaken);
        void UpdateUserRole(Nre_Uer_New userRole, string actionTaker, string actionTaken);
        void DeleteUserRole(string id, string actionTaker, string actionTaken);
    }
}
