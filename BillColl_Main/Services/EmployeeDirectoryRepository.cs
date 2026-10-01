using BillColl_Main.Models;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Data;
using System.Net;

namespace BillColl_Main.Services
{
    public class EmployeeDirectoryRepository : IEmployeeDirectory
    {
        private readonly SharedSqlConnectionFactory connectionFactory;

        public EmployeeDirectoryRepository(IConfiguration configuration, SharedSqlConnectionFactory connectionFactory = null)
        {
            this.connectionFactory = connectionFactory ?? new SharedSqlConnectionFactory(configuration);
        }

        public bool IsConfigured => connectionFactory.IsConfigured;

        public string GetNameByEmployeeCode(string employeeCode)
        {
            if (string.IsNullOrWhiteSpace(employeeCode))
            {
                return "";
            }

            if (!IsConfigured)
            {
                return employeeCode;
            }

            var sql = $@"SELECT XEX.[EmployeeCode], (XEX.[FamilyName] + ', ' + XEX.[FirstName]) [EmployeeName]
                         FROM {HrisTable("viewEmployeeProfile")} XEX
                         WHERE XEX.[CloudEmail] IS NOT NULL AND XEX.[EmployeeCode] = @EmployeeCode";

            var dt = Query(sql, new { EmployeeCode = employeeCode });

            if (dt.Rows.Count == 0)
            {
                return employeeCode;
            }

            var name = Cell(dt.Rows[0], "EmployeeName");

            return string.IsNullOrWhiteSpace(name) ? employeeCode : name;
        }

        public string GetEmployeeCodeByEmail(string email)
        {
            if (!IsConfigured || string.IsNullOrWhiteSpace(email))
            {
                return null;
            }

            var sql = $@"SELECT XEX.[EmployeeCode]
                         FROM {HrisTable("viewEmployeeProfile")} XEX
                         WHERE XEX.[CloudEmail] IS NOT NULL AND XEX.[CloudEmail] = @Email";

            var dt = Query(sql, new { Email = email });

            return dt.Rows.Count == 0 ? null : Cell(dt.Rows[0], "EmployeeCode");
        }

        public IEnumerable<Example> GetStaff(string selection)
        {
            var results = new List<Example>();

            if (!IsConfigured)
            {
                return results;
            }

            var sql = $@"SELECT XEX.[EmployeeCode],
                                (XEX.[FamilyName] + ', ' + XEX.[FirstName]) [EmployeeName],
                                LTRIM(RTRIM(XEX.[StaffOUCode])) [GroupName],
                                XEX.[CloudEmail],
                                XEX.[ManagementLevel]
                         FROM {HrisTable("viewEmployeeProfile")} XEX
                         WHERE XEX.[Active] = 1 AND XEX.[CloudEmail] IS NOT NULL AND XEX.[GUID] IS NOT NULL";

            if (IsPartner(selection))
            {
                sql += " AND XEX.[ManagementLevel] LIKE '%Partner%'";
            }
            else if (IsExecutive(selection))
            {
                sql += " AND XEX.[ManagementLevel] LIKE '%Administrative%'";
            }

            var dt = Query(sql, null);

            foreach (DataRow dr in dt.Rows)
            {
                results.Add(new Example
                {
                    Id = Cell(dr, "EmployeeCode"),
                    Name = WebUtility.HtmlEncode(Cell(dr, "EmployeeName")),
                    group = Cell(dr, "GroupName"),
                    Email = Cell(dr, "CloudEmail")
                });
            }

            results.Sort((a, b) => string.Compare(a.Name, b.Name, StringComparison.OrdinalIgnoreCase));

            return results;
        }

        private static bool IsPartner(string selection)
        {
            return string.Equals(selection, StaffSelection.Partner, StringComparison.OrdinalIgnoreCase);
        }

        private static bool IsExecutive(string selection)
        {
            return string.Equals(selection, StaffSelection.Executive, StringComparison.OrdinalIgnoreCase);
        }

        private string HrisTable(string tableName)
        {
            var servers = connectionFactory.LinkedServers;

            return "[" + servers.Hris + "].[HRIS].dbo.[" + tableName + "]";
        }

        private DataTable Query(string sql, object parameters)
        {
            try
            {
                return connectionFactory.FillDataTable(sql, parameters);
            }
            catch
            {
                return new DataTable();
            }
        }

        private static string Cell(DataRow dr, string column)
        {
            if (!dr.Table.Columns.Contains(column) || dr[column] == DBNull.Value)
            {
                return "";
            }

            return dr[column].ToString();
        }
    }
}