using System;

namespace BillColl_Main.ViewModel
{
    public class ExceptionUpdateViewModel
    {
        public int Id { get; set; }
        public string ClientCode { get; set; }
        public string ClientName { get; set; }
        public string Reason { get; set; }
        public string Other_Reason { get; set; }
        public string Previous_Reason { get; set; }
        public string User_Name { get; set; }
        public string Action { get; set; }
        public string Invoice_Number { get; set; }
        public string Bill_No { get; set; }
    }
}
