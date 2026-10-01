using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BillColl_Main.Models
{
    public class Group
    {
        public int Id { get; set; }
        public string Code { get; set; }
        public string Description { get; set; }
        public ICollection<EngagementTeam> EngagementTeams { get; set; }
    }
}
