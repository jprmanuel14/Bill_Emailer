using BillColl_Main.Base;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace BillColl_Main.Class
{
    public class EngagementTeamList: CommonContactInfoBase
    {
        [Required]
        public string Name { get; set; }
        [NotMapped]
        public string GroupName { get; set; }
        public int IsBillingManager { get; set; }
    }
}
