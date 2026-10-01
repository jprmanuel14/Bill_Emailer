using BillColl_Main.Models;
using Dapper;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace BillColl_Main.Services
{
    public class LogsSQLRepository : ILogs
    {
        private readonly IConfiguration configuration;
        private readonly DbConnectionFactory connectionFactory;

        public LogsSQLRepository(IConfiguration configuration, DbConnectionFactory connectionFactory = null)
        {
            this.configuration = configuration;
            this.connectionFactory = connectionFactory ?? new DbConnectionFactory(configuration);
        }

        public IEnumerable<Logg_Reports> GetClientActivityLogs()
        {
            var sql = connectionFactory.IsPostgreSql
                ? @"SELECT ""Id"", ""Date"", ""Time"", ""User_Name"", ""Client_Name"", ""inv_Number"",
                          ""BillNo"", ""Action"", ""Previous_Reason"", ""Current_Reason"", ""Deletion_Reason""
                   FROM dbo.""tbl_Log_Clients""
                   ORDER BY ""Time"" DESC"
                : $@"SELECT [Id], [Date], [Time], [User_Name], [Client_Name], [inv_Number],
                          [BillNo], [Action], [Previous_Reason], [Current_Reason], [Deletion_Reason]
                   FROM {connectionFactory.BcatTable("tbl_Log_Clients")}
                   ORDER BY [Time] DESC";

            var dt = connectionFactory.FillDataTable(sql);

            return dt.AsEnumerable()
                      .Select(MapLog)
                      .ToList();
        }

        public void AddClientActivityLog(string userName, string clientName, string invoiceNumber, string action,
            string previousReason, string currentReason, string deletionReason, string billNo)
        {
            var isPostgres = connectionFactory.IsPostgreSql;

            var sql = isPostgres
                ? @"INSERT INTO dbo.""tbl_Log_Clients""
                    (""Date"", ""Time"", ""User_Name"", ""Client_Name"", ""inv_Number"", ""Action"",
                     ""Previous_Reason"", ""Current_Reason"", ""Deletion_Reason"", ""BillNo"")
                    VALUES (CURRENT_DATE, CURRENT_TIMESTAMP, @User_Name, @Client_Name, @inv_Number, @Action,
                            @Previous_Reason, @Current_Reason, @Deletion_Reason, @BillNo)"
                : $@"INSERT INTO {connectionFactory.BcatTable("tbl_Log_Clients")}
                    ([Date], [Time], [User_Name], [Client_Name], [inv_Number], [Action],
                     [Previous_Reason], [Current_Reason], [Deletion_Reason], [BillNo])
                    VALUES (GETDATE(), GETDATE(), @User_Name, @Client_Name, @inv_Number, @Action,
                            @Previous_Reason, @Current_Reason, @Deletion_Reason, @BillNo)";

            using var conn = connectionFactory.CreateConnection();
            conn.Execute(sql, new
            {
                User_Name = NullIfEmpty(userName),
                Client_Name = NullIfEmpty(clientName),
                inv_Number = NullIfEmpty(invoiceNumber),
                Action = NullIfEmpty(action),
                Previous_Reason = NullIfEmpty(previousReason),
                Current_Reason = NullIfEmpty(currentReason),
                Deletion_Reason = NullIfEmpty(deletionReason),
                BillNo = NullIfEmpty(billNo)
            });
        }

        private static string NullIfEmpty(string value)
        {
            return string.IsNullOrWhiteSpace(value) ? null : value;
        }

        private static Logg_Reports MapLog(DataRow dr)
        {
            return new Logg_Reports()
            {
                Id = Value(dr, "Id"),
                Date_Log = FormatDate(Value(dr, "Date"), "yyyy-MM-dd"),
                Time_Log = FormatDate(Value(dr, "Time"), "yyyy-MM-dd HH:mm:ss"),
                User_Name = Value(dr, "User_Name"),
                Client_Name = Value(dr, "Client_Name"),
                Invoice_Number = Value(dr, "inv_Number"),
                Bill_No = Value(dr, "BillNo"),
                Action = Value(dr, "Action"),
                Previous_Exception_Reason = Value(dr, "Previous_Reason"),
                Current_Exception_Reason = Value(dr, "Current_Reason"),
                Deletion_Reason = Value(dr, "Deletion_Reason")
            };
        }

        private static string Value(DataRow dr, string column)
        {
            if (!dr.Table.Columns.Contains(column) || dr[column] == DBNull.Value)
            {
                return "";
            }

            return dr[column].ToString();
        }

        private static string FormatDate(string raw, string format)
        {
            if (string.IsNullOrWhiteSpace(raw))
            {
                return "";
            }

            if (DateTime.TryParse(raw, out var parsed))
            {
                return parsed.ToString(format);
            }

            return raw;
        }
    }
}
