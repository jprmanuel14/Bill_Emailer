using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BillColl_Main.Models
{
    public class Bank
    {
        public int Id { get; set; }
        public string AccountCode { get; set; }
        public string AccountNumber { get; set; }
        public string AccountName { get; set; }
        public string BankName { get; set; }
        public string Branch { get; set; }
        public string SwiftCode { get; set; }
        public IList<BankPHEntity> BankPHEntity { get; set; }
    }
}
