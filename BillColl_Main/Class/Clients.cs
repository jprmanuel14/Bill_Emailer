using BillColl_Main.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BillColl_Main.Class
{
    public class Clients: StatusBillingBase
    {
        public string PartnerInvolved { get; set; }
        public string GroupName { get; set; }
        public string Group_Code { get; set; }
        public int TotalPages { get; set; }
        public int Clients_Total { get; set; }
    }
}
