namespace BillColl_Main.Models
{
    public class Logg_Reports
    {
        public string Id { get; set; }
        public string Date_Log { get; set; }
        public string Time_Log { get; set; }
        public string User_Name { get; set; }
        public string Client_Name { get; set; }
        public string Invoice_Number { get; set; }
        public string Bill_No { get; set; }
        public string Action { get; set; }
        public string Previous_Exception_Reason { get; set; }
        public string Current_Exception_Reason { get; set; }
        public string Deletion_Reason { get; set; }
    }
}
