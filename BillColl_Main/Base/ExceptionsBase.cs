using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace BillColl_Main.Base
{
    public abstract class ExceptionsBase
    {
        public int Id { get; set; }
        [NotMapped]
        public string eId { get; set; }
        [Required]
        public string ClientCode { get; set; }
        public string ClientName { get; set; }
    }
}
