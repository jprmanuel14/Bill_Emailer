using BillColl_Main.Models;
using System;
using System.Collections.Generic;
using System.Data;

namespace BillColl_Main.Services
{
    /// <summary>
    /// Read model behind the Settings screens. Everything here comes from the primary BCAT
    /// database, so it works on both providers. Fields that only exist in the shared SQL
    /// Server estate (HRIS names, databank line-of-service) are filled in by
    /// <see cref="IEmployeeDirectory"/> and <see cref="ISharedSql"/>, which degrade quietly
    /// when that estate is not configured.
    /// </summary>
    public interface ISettingsRepository
    {
        IEnumerable<Nre_Uer_New> GetAdminLogs();

        IEnumerable<Engagement_Secretary> GetSecretaryLogs();

        IEnumerable<Engagement_Secretary> GetEngagementSecretaries();

        Engagement_Secretary GetEngagementSecretary(string id);

        IEnumerable<Engagement_Secretary> GetViewReports(string clientCode, string statementDate, string los);

        DataSet GetAdminLogsExport();

        DataSet GetSecretaryLogsExport();

        IReadOnlyCollection<string> GetUserRoleEmployeeCodes();
    }
}