using BillColl_Main.Class;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BillColl_Main.ViewModel
{
    public class MailReportViewModel
    {
        public List<StatementOfAccounts> StatementOfAccounts { get; set; }
        public BankDetails BankDetails { get; set; }
        public Clients Clients { get; set; }
    }
}
