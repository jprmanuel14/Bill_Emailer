using BillColl_Main.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BillColl_Main.Class
{
    public class StatementOfAccounts: StatusBillingBase
    {
        public string TransactionNo { get; set; }
        public string InvoiceDate { get; set; }
        public string StatementDate { get; set; }
        public string InvoiceNo { get; set; }
        public string ReferenceNo { get; set; }
        public int Age { get; set; }
        public string BillDescription { get; set; }
        public string OtherCurrCode { get; set; }
        public decimal AmountInOtherCurr { get; set; }
        public decimal AmountInPeso { get; set; }
        public string EntityCode { get; set; }
    }
}
