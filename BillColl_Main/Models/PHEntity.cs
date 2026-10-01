using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BillColl_Main.Models
{
    public class PHEntity
    {
        public int Id { get; set; }
        public string EntityCode { get; set; }
        public string EntityName { get; set; }
        public int ReportHierarchy { get; set; }
        public IList<BankPHEntity> BankPHEntity { get; set; }
    }
}
