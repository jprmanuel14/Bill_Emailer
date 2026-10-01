using BillColl_Main.AppDbContext;
using BillColl_Main.Class;
using BillColl_Main.Models;

using Dapper;
using Microsoft.Extensions.Configuration;
using myhelperclass;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BillColl_Main.Services
{
    public class ExceptionSQLRepository : IExceptions
    {
        private readonly myDBContext context;
        private readonly IConfiguration configuration;
        private readonly DbConnectionFactory connectionFactory;

        public ExceptionSQLRepository(myDBContext context, IConfiguration configuration, DbConnectionFactory connectionFactory = null)
        {
            this.context = context;
            this.configuration = configuration;
            this.connectionFactory = connectionFactory ?? new DbConnectionFactory(configuration);
        }

        public Exceptions AddExceptions(Exceptions exceptions)
        {
            context.Exceptions.Add(exceptions);
            context.SaveChanges();
            return exceptions;
        }

        public Exceptions GetException(string ClientCode)
        {
            return context.Exceptions.FirstOrDefault(e => e.ClientCode == ClientCode);
        }

        public ExceptionDetail GetExceptionById(int id)
        {
            var sql = connectionFactory.IsPostgreSql
                ? @"SELECT ""Id"", ""ClientCode"", ""ClientName"", ""Reason"", ""Other_Reason"", ""Reference_No"", ""BillNo""
                    FROM dbo.""Exceptions""
                    WHERE ""Id"" = @id"
                : $@"SELECT [Id], [ClientCode], [ClientName], [Reason], [Other_Reason], [Reference_No], [BillNo]
                    FROM {connectionFactory.BcatTable("Exceptions")}
                    WHERE [Id] = @id";

            var dt = connectionFactory.FillDataTable(sql, new { id });

            if (dt.Rows.Count == 0)
            {
                return null;
            }

            var dr = dt.Rows[0];

            var reason = Value(dr, "Reason");
            var otherReason = Value(dr, "Other_Reason");

            return new ExceptionDetail
            {
                Id = Value(dr, "Id"),
                ClientCode = Value(dr, "ClientCode"),
                ClientName = Value(dr, "ClientName"),
                Reason = reason,
                Other_Reason = otherReason,
                Previous_Reason = reason == "Others" ? otherReason : reason,
                Reference_No = Value(dr, "Reference_No"),
                Bill_No = Value(dr, "BillNo")
            };
        }

        public bool UpdateExceptionReason(int id, string reason, string otherReason)
        {
            var sql = connectionFactory.IsPostgreSql
                ? @"UPDATE dbo.""Exceptions""
                    SET ""Reason"" = @Reason, ""Other_Reason"" = @Other_Reason
                    WHERE ""Id"" = @id"
                : $@"UPDATE {connectionFactory.BcatTable("Exceptions")}
                    SET [Reason] = @Reason, [Other_Reason] = @Other_Reason
                    WHERE [Id] = @id";

            using var conn = connectionFactory.CreateConnection();
            return conn.Execute(sql, new { id, Reason = reason, Other_Reason = otherReason }) > 0;
        }

        private static string Value(DataRow dr, string column)
        {
            if (!dr.Table.Columns.Contains(column) || dr[column] == DBNull.Value)
            {
                return "";
            }

            return dr[column].ToString();
        }

        public IEnumerable<ExceptionsList> GetExceptions(string clientName = "", string dateSentPrev = "", string dateSortPrev = "")
        {
            var isPostgres = connectionFactory.IsPostgreSql;
            var sb = new StringBuilder();

            if (isPostgres)
            {
                sb.Append(@"SELECT * FROM (
                    SELECT a.""Id"", b.""ClientName"", a.""ClientCode"", a.""Reason"",
                           (SELECT TO_CHAR(""SentDate"", 'YYYY-MM-DD') FROM dbo.""tblScheduledMail"" WHERE ""ClientCode"" = a.""ClientCode"" ORDER BY ""SentDate"" DESC LIMIT 1) AS ""PreviousSentDate""
                    FROM dbo.""Exceptions"" a
                    INNER JOIN dbo.""vwClients"" b ON a.""ClientCode"" = b.""ClientCode""");

                if (!string.IsNullOrWhiteSpace(clientName))
                {
                    sb.Append(@" WHERE b.""ClientName"" ILIKE @clientName");
                }

                sb.Append(@" ORDER BY b.""ClientName"" ASC LIMIT 1000) v WHERE ""ClientCode"" IS NOT NULL");
            }
            else
            {
                sb.Append(@"SELECT * FROM (
                    SELECT TOP 1000 a.Id, b.ClientName, a.ClientCode, a.[Reason],
                           (SELECT TOP 1 CONVERT(nvarchar(10), SentDate, 121) FROM [tblScheduledMail] WHERE ClientCode = a.ClientCode ORDER BY SentDate DESC) AS PreviousSentDate
                    FROM [Exceptions] a
                    INNER JOIN [vwClients] b ON a.ClientCode = b.ClientCode");

                if (!string.IsNullOrWhiteSpace(clientName))
                {
                    sb.Append(@" WHERE b.ClientName LIKE @clientName");
                }

                sb.Append(@" ORDER BY b.ClientName ASC) v WHERE ClientCode IS NOT NULL");
            }

            var dt = connectionFactory.FillDataTable(sb.ToString(), new { clientName = clientName + "%" });

            return (from DataRow dr in dt.Rows
                    select new ExceptionsList()
                    {
                        Id = Convert.ToInt32(dr["Id"].ToString()),
                        ClientCode = dr["ClientCode"].ToString(),
                        ClientName = dr["ClientName"].ToString(),
                        PreviousSentDate = (dr["PreviousSentDate"] == DBNull.Value) ? "" : Convert.ToDateTime(dr["PreviousSentDate"]).ToString("dd MMM yyyy"),
                        Reason = dr["Reason"].ToString(),
                    }).ToList();
        }

        public IEnumerable<ExceptionsList> GetExceptionsAll(string clientName = "", string dateSentPrev = "", string dateSortPrev = "")
        {
            var isPostgres = connectionFactory.IsPostgreSql;
            var sb = new StringBuilder();

            if (isPostgres)
            {
                sb.Append(@"SELECT * FROM (
                    SELECT a.""Id"", b.""ClientName"", a.""ClientCode"", a.""Reason"",
                           (SELECT TO_CHAR(""SentDate"", 'YYYY-MM-DD') FROM dbo.""tblScheduledMail"" WHERE ""ClientCode"" = a.""ClientCode"" ORDER BY ""SentDate"" DESC LIMIT 1) AS ""PreviousSentDate""
                    FROM dbo.""Exceptions"" a
                    INNER JOIN dbo.""vwClients"" b ON a.""ClientCode"" = b.""ClientCode"") v WHERE ""ClientCode"" IS NOT NULL");

                if (!string.IsNullOrWhiteSpace(clientName))
                {
                    sb.Append(@" AND ""ClientName"" ILIKE @clientName");
                }
            }
            else
            {
                sb.Append(@"SELECT * FROM (
                    SELECT a.Id, b.ClientName, a.ClientCode, a.[Reason],
                           (SELECT TOP 1 CONVERT(nvarchar(10), SentDate, 121) FROM [tblScheduledMail] WHERE ClientCode = a.ClientCode ORDER BY SentDate DESC) AS PreviousSentDate
                    FROM [Exceptions] a
                    INNER JOIN [vwClients] b ON a.ClientCode = b.ClientCode) v WHERE ClientCode IS NOT NULL");

                if (!string.IsNullOrWhiteSpace(clientName))
                {
                    sb.Append(@" AND ClientName LIKE @clientName");
                }
            }

            var dt = connectionFactory.FillDataTable(sb.ToString(), new { clientName = clientName + "%" });

            return (from DataRow dr in dt.Rows
                    select new ExceptionsList()
                    {
                        Id = Convert.ToInt32(dr["Id"].ToString()),
                        ClientCode = dr["ClientCode"].ToString(),
                        ClientName = dr["ClientName"].ToString(),
                        PreviousSentDate = (dr["PreviousSentDate"] == DBNull.Value) ? "" : Convert.ToDateTime(dr["PreviousSentDate"]).ToString("dd MMM yyyy"),
                        Reason = dr["Reason"].ToString(),
                    }).ToList();
        }

        public Exceptions RemoveExceptions(int Id)
        {
            var myexception= context.Exceptions.FirstOrDefault(e => e.Id == Id);
            context.Exceptions.Remove(myexception);
            context.SaveChanges();
            return myexception;
        }

        public IEnumerable<Reason> GetReasons()
        {
            return GetReasonList("tbl_Reasons");
        }

        public IEnumerable<Reason> GetDeletionReasons()
        {
            return GetReasonList("Deletion_Reason");
        }

        private IEnumerable<Reason> GetReasonList(string table)
        {
            var sql = connectionFactory.IsPostgreSql
                ? $@"SELECT ""Id"", ""Description"" FROM dbo.""{table}"" ORDER BY ""Description"""
                : $@"SELECT [Id], [Description] FROM {connectionFactory.BcatTable(table)} ORDER BY [Description]";

            var dt = connectionFactory.FillDataTable(sql);

            return (from DataRow dr in dt.Rows
                    select new Reason()
                    {
                        Id = Convert.ToInt32(dr["Id"]),
                        Description = dr["Description"] == DBNull.Value ? "" : dr["Description"].ToString()
                    }).ToList();
        }
    }
}
