using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace BillColl_Main.ViewModel
{
    public class LoginViewModel
    {
        [Required(ErrorMessage ="This is required!")]
        public string Username { get; set; }
    }
}
