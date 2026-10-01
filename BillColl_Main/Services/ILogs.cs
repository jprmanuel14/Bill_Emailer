using BillColl_Main.Models;
using System.Collections.Generic;

namespace BillColl_Main.Services
{
    public interface ILogs
    {
        IEnumerable<Logg_Reports> GetClientActivityLogs();

        void AddClientActivityLog(string userName, string clientName, string invoiceNumber, string action,
            string previousReason, string currentReason, string deletionReason, string billNo);
    }
}
