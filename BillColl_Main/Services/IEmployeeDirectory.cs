using BillColl_Main.Models;
using System.Collections.Generic;

namespace BillColl_Main.Services
{
    /// <summary>
    /// Read access to HRIS (<c>viewEmployeeProfile</c>). HRIS stayed on SQL Server when BCAT
    /// moved to PostgreSQL, so every lookup here goes through the shared SQL Server
    /// connection. When that connection is not configured the directory reports
    /// <see cref="IsConfigured"/> as false and the lookups degrade to returning the value
    /// that was asked for, so the UI stays usable instead of returning HTTP 500.
    /// </summary>
    public interface IEmployeeDirectory
    {
        bool IsConfigured { get; }

        /// <summary>Display name for an employee code. Falls back to the code itself.</summary>
        string GetNameByEmployeeCode(string employeeCode);

        /// <summary>Employee code for a cloud email address, or null when unknown.</summary>
        string GetEmployeeCodeByEmail(string email);

        /// <summary>People who can be picked in the Settings screens.</summary>
        IEnumerable<Example> GetStaff(string selection);
    }

    /// <summary>Selection modes for <see cref="IEmployeeDirectory.GetStaff"/>.</summary>
    public static class StaffSelection
    {
        public const string All = "all";
        public const string Executive = "executive";
        public const string Partner = "partner";
    }
}