using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BillColl_Main.Class
{
    public class MailRequest
    {
        public string ToEmail { get; set; }
        public string ToEmails { get; set; }
        public string ToCCEmails { get; set; }
        public string ToCCEmail { get; set; }
        public string Subject { get; set; }
        public string Body { get; set; }
        public bool WithImage { get; set; }
    }
}
