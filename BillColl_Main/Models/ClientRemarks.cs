using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace BillColl_Main.Models
{
    public class ClientRemarks
    {
        public int Id { get; set; }
        [Required]
        public string ClientCode { get; set; }
        [Required]
        public DateTime StatementDate { get; set; }
        public string Remarks { get; set; }
    }
}
