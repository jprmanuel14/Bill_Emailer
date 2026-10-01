using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BillColl_Main.Base
{
    public abstract class StatusSDateBase: StatusBillingBase
    {
        public DateTime StatementDate { get; set; }
        public string Remarks { get; set; }
    }
}
