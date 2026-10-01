using BillColl_Main.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BillColl_Main.Services
{
    public interface IGroup
    {
        IEnumerable<Group> GetGroups();
        Group GetGroup(string GroupName);
        Group GetGroup(int GroupId);
        IEnumerable<Group> GetGroups(string searchGName);
        IEnumerable<Group_Ver> GetGroupCodeList();
    }
}
