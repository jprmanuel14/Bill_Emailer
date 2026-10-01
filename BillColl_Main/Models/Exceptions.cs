using BillColl_Main.Base;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace BillColl_Main.Models
{
    public class Exceptions: ExceptionsBase
    {       
        public string Reason { get; set; }
    }
}
