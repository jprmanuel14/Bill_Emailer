using BillColl_Main.Models;
using System.Collections.Generic;

namespace BillColl_Main.Services
{
    /// <summary>
    /// Read access to the shared SQL Server estate (PH Report Databank, FinApps, HRIS).
    /// These stay on SQL Server even when the primary BCAT database is PostgreSQL.
    /// </summary>
    public interface ISharedSql
    {
        bool IsConfigured { get; }

        Group_Ver GetLOSByGroupCode(string groupCode);
    }
}
