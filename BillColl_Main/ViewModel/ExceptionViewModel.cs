using BillColl_Main.Base;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace BillColl_Main.ViewModel
{
    public class ExceptionViewModel
    {
        [Required(ErrorMessage = "Debtor code is required")]
        public string ClientCode { get; set; }
        [Required(ErrorMessage = "Debtor name is required")]
        public string ClientName { get; set; }
        [Required(ErrorMessage ="Please fill in the reason's field.")]
        public string Reasons { get; set; }
    }
}
