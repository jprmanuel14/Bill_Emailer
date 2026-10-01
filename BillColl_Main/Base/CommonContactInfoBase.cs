using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace BillColl_Main.Base
{
    public abstract class CommonContactInfoBase
    {
        public int Id { get; set; }
        [NotMapped]
        public string eId { get; set; }
        public string ClientCode { get; set; }
        [Required]
        public string Email { get; set; }
        public string ContactNumber { get; set; }
        public string Designation { get; set; }
    }
}
