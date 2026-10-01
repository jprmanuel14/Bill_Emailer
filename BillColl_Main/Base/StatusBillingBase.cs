using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BillColl_Main.Base
{
    public abstract class StatusBillingBase
    {
        public string eClientId { get; set; }
        public string ClientId { get; set; }
        public string ClientName { get; set; }
        public string ContactEmail { get; set; }
        public string ContactName { get; set; }
        public string ContactEmailD { get; set; }

    }
}
