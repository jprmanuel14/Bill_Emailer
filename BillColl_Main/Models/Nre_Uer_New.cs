namespace BillColl_Main.Models
{
    public class Nre_Uer_New
    {
        public string Id { get; set; }
        public string Employee_Code { get; set; }
        public string Employee_Name { get; set; }
        public string User_Role { get; set; }
        public string Date_Assigned { get; set; }

        // For the admin logs

        public string Log_Date { get; set; }
        public string Log_Time { get; set; }
        public string Action_Taker { get; set; }
        public string Action_Taken { get; set; }
        public string Log_Time_Format { get; set; }

    }



    public class Engagement_Secretary
    {
        public string Id { get; set; }
        public string Secretary { get; set; }
        public string Secretary_Code { get; set; }
        public string Partner { get; set; }
        public string Partner_Code { get; set; }
        public string Manager { get; set; }
        public string Group { get; set; }
        public string Group_Id { get; set; }
        public bool is_default { get; set; }
        public int adding_counter { get; set; }

        public string intBillID { get; set; }
        public string vcStatementName { get; set; }
        public string Debtor_Name { get; set; }
        public string Debtor_Address { get; set; }
        public string Reference_No { get; set; }
        public string Fee_Note_No { get; set; }
        public string Tot_Amount_Payable { get; set; }

        public object  imgFeeNoteBody { get; set; }
        public object  imgPDFTaxInvoice { get; set; }
        public string imgFeeNoteBody_str { get; set; }
        public string chBillType { get; set; }
        public string chDebtorCode { get; set; }
        public string chJobCode { get; set; }
        public string chBillEntityCode { get; set; }
        public string Entity { get; set; }
        public string vcAccountNumber { get; set; }
        public string AccountCode { get; set; }
        public string los { get; set; }
        public string StatementDate { get; set; }
        public string Email { get; set; }
        public string EX { get; set; }
        public string Invoice { get; set; }
        public string Bill_No { get; set; }
        public string Age { get; set; }
        public string Age_Bracket { get; set; }
        public string Other_Currency_Code { get; set; }
        public decimal Dollar { get; set; }
        public decimal Peso { get; set; }
        public string Trans_Date { get; set; }
        public string Practice_Name { get; set; }
        public string Remarks { get; set; }
        public string Run_Date { get; set; }


        // For the e-s logs

        public string Log_Date { get; set; }
        public string Log_Time { get; set; }
        public string Action_Taker { get; set; }
        public string Action_Taken { get; set; }
        public string Log_Time_Format { get; set; }
        public int Engagement_Secretary_Count { get; set; }

    }
}