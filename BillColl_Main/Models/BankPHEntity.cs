using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BillColl_Main.Models
{
    public class BankPHEntity
    {
        public int BankId { get; set; }
        public Bank Bank { get; set; }
        public int PHEntityId { get; set; }
        public PHEntity PHEntity { get; set; }
    }
}
