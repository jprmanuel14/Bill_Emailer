using BillColl_Main.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BillColl_Main.Class
{
    public class ExceptionsList: ExceptionsBase
    {
        public string PreviousSentDate { get; set; }
        public string Reason { get; set; }
        public string Reference_No { get; set; }
        public string Bill_No { get; set; }
        public string Manager { get; set; }
        public string Partner { get; set; }
        public int TotalPages { get; set; }
        public int Exceptions_Total { get; set; }

    }
}
