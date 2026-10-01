using BillColl_Main.Class;
using BillColl_Main.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BillColl_Main.Services
{
    public interface IBillingMailStatus
    {
        IEnumerable<Delivered> GetDelivered(string clientName = "", string dateSent = "", string dateSort = "");

        IEnumerable<Delivered_Inside> GetDelivered_New(string clientName = "", string dateSent = "", string dateSort = "", int variable_offset = 0);

        IEnumerable<Undelivered> GetUndelivered(string clientName = "", string dateSent = "", string dateSort = "");

        IEnumerable<Undelivered_Inside> GetUndelivered_New(string clientName = "", string dateSent = "", string dateSort = "", int variable_offset = 0);

        IEnumerable<Delivered> GetDeliveredAll(string clientName = "", string dateSent = "", string dateSort = "");

        IEnumerable<Undelivered> GetUndeliveredAll(string clientName = "", string dateSent = "", string dateSort = "");

        IEnumerable<MailPercentageStatus> GetMailPercentageStatuses(int offset_parameter = 0);
        IEnumerable<StatementOfAccounts> GetStatementOfAccounts(string ClientId);
        IEnumerable<StatementOfAccounts> GetStatementOfAccounts(string ClientId, string StatementDate);
        //BankDetails GetBankDetails(string AccountCode);
        IEnumerable<Bank> GetBanks(string AccountCode);
        IEnumerable<Bank> GetBanks();

        IEnumerable<PHEntityList> GetPHEntityLists(string ClientId, string StatementDate);

        IEnumerable<PHEntityList> GetPHEntityLists_New(string ClientId, string StatementDate, string entity_code_variable);

        IEnumerable<PHEntityList> GetPHEntityLists(string ClientId, string StatementDate, bool is_delivered = false, string ex_LOS_variable = "");

        Bank GetBank(string AccountCode, string BankName, string AccountNumber);

        DateTime? GetRecentStatementDate(string ClientId);

        void UpdateTblScheduleMailStatus(string ClientId, string myStatus);

        void UpdateContacts(string ClientId, DateTime StatementDate);

        void UpdateEngagements(string ClientId, DateTime StatementDate);

        void UpdateContacts_V2(string ClientId, DateTime StatementDate);

        void UpdateEngagements_V2(string ClientId, DateTime StatementDate);

        void UpdateSendDate(string ClientCode, string StatementDate);

    }
}
