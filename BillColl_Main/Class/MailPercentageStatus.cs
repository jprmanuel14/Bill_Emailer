using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BillColl_Main.Class
{
    public class MailPercentageStatus
    {
        public DateTime StatementDate { get; set; }
        public float DeliveryRate { get; set; }
        public int Percentage_Total { get; set; }
        public int TotalPages { get; set; }
    }
}
