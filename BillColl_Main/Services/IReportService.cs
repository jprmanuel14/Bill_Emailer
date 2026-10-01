using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace BillColl_Main.Services
{
    public interface IReportService
    {
        byte[] GenerateSOA(string Clientcode, string StatementDate, string url);
        byte[] GenerateMail(string Clientcode, string AccountCode, string StatementDate, bool withImg, string url);
        DataTable GetUndelivered(string clientName = "", string dateSent = "");
        DataTable GetDelivered(string clientName = "", string dateSent = "");
        DataTable GetClients (string clientName = "");
        DataTable GetClientExceptions(string clientName = "", string dateSentPrev = "");
        DataTable GetClientActivityLogs();

    }
}
