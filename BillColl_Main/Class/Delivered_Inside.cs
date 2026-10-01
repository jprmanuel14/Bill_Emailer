using BillColl_Main.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BillColl_Main.Class
{
    public class Delivered_Inside
    {
        public string eClientId { get; set; }
        public string Remarks { get; set; }
        public bool ReadReceipt { get; set; }

        public string ClientId { get; set; }
        public string ClientName { get; set; }
        public string LoS { get; set; }
        public string OU { get; set; }

        public string OU_Code { get; set; }
        public string Billing_Entity { get; set; }
        public string Manager { get; set; }
        public string Partner { get; set; }
        public string Statement_Date { get; set; }
        public string Reference_No { get; set; }
        public string Client_Contact { get; set; }
        public string Client_Contact_Email { get; set; }
        public string Amount_Peso { get; set; }
        public string Amount_Dollar { get; set; }
        public string Sent_Out_Date { get; set; }
        public string Contacts_Id { get; set; }
        public string Primary_Contact { get; set; }
        public int Delivered_Count { get; set; }
        public int Delivered_Total { get; set; }
        public int TotalPages { get; set; }

        public List<Delivered_Inside> delivered_Insides { get; set; }
    }
}
