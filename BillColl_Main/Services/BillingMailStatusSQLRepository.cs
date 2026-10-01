using BillColl_Main.AppDbContext;
using BillColl_Main.Class;
using BillColl_Main.Models;
using Dapper;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BillColl_Main.Services
{
    public class BillingMailStatusSQLRepository : IBillingMailStatus
    {
        private readonly IConfiguration configuration;
        private readonly myDBContext context;
        private readonly DbConnectionFactory connectionFactory;

        public BillingMailStatusSQLRepository(IConfiguration configuration, myDBContext context, DbConnectionFactory connectionFactory = null)
        {
            this.configuration = configuration;
            this.context = context;
            this.connectionFactory = connectionFactory ?? new DbConnectionFactory(configuration);
        }

        public Bank GetBank(string AccountCode, string BankName, string AccountNumber)
        {
            return context.Banks.FirstOrDefault(e => e.AccountCode == AccountCode && e.BankName == BankName && e.AccountNumber == AccountNumber);
        }

        public IEnumerable<Bank> GetBanks(string AccountCode)
        {
            return context.Banks.Where(e => e.AccountCode == AccountCode);
        }

        public IEnumerable<Bank> GetBanks()
        {
            return context.Banks;
        }

        public IEnumerable<Delivered> GetDelivered(string clientName = "", string dateSent = "", string dateSort = "")
        {
            var isPostgres = connectionFactory.IsPostgreSql;
            var sb = new StringBuilder();
            var sqlParams = new List<SqlParameter>();

            if (isPostgres)
            {
                sb.Append(@"SELECT ""Id"", ""ClientCode"", ""ClientName"", (""StatementDate"" - INTERVAL '1 day') AS ""StatementDate"", ""Recepients"", ""CC"", split_part(""Recepients"", ',', 1) AS ""RecepientsD""
                            FROM dbo.""tblScheduledMail""
                            WHERE ""SentDate"" IS NOT NULL
                              AND ""ClientCode"" IN (SELECT a.""ARClientCode"" FROM dbo.""tbBCATData"" a WHERE a.""StatementDate"" = dbo.""tblScheduledMail"".""StatementDate"" GROUP BY a.""ARClientCode"" HAVING SUM(a.""Peso"") >= 100)");

                if (!string.IsNullOrWhiteSpace(clientName))
                {
                    sb.Append(@" AND TRIM(""ClientName"") ILIKE @ClientName");
                    sqlParams.Add(new SqlParameter("@ClientName", clientName + "%"));
                }
            }
            else
            {
                sb.Append(@"SELECT TOP 1000 [Id],[ClientCode],[ClientName],dateadd(DAY,-1,[StatementDate]) as [StatementDate],[Recepients],[CC],(SELECT top 1 value FROM string_split(tblScheduledMail.Recepients,',')) as RecepientsD
                            FROM [tblScheduledMail]
                            WHERE SentDate is not null
                              AND clientcode in (select a.ARClientCode from [tbBCATData] as a where a.StatementDate=tblScheduledMail.StatementDate group by a.ARClientCode having sum(a.Peso)>=100)");

                if (!string.IsNullOrWhiteSpace(clientName))
                {
                    sb.Append(@" AND ltrim(ClientName) like @ClientName");
                    sqlParams.Add(new SqlParameter("@ClientName", clientName + "%"));
                }
            }

            if (!string.IsNullOrWhiteSpace(dateSent))
            {
                string[] myarrDate = dateSent.Split("-");
                sb.Append(@" AND ""StatementDate"" BETWEEN @StatementDateF AND @StatementDateT");
                sqlParams.Add(new SqlParameter("@StatementDateF", Convert.ToDateTime(myarrDate[0])));
                sqlParams.Add(new SqlParameter("@StatementDateT", Convert.ToDateTime(myarrDate[1])));
            }

            if (!string.IsNullOrWhiteSpace(dateSort) && dateSort != "undefined")
            {
                switch (dateSort)
                {
                    case "Newest - Oldest":
                        sb.Append(" ORDER BY \"StatementDate\" DESC");
                        break;
                    default:
                        sb.Append(" ORDER BY \"StatementDate\"");
                        break;
                }
            }
            else
            {
                sb.Append(" ORDER BY \"ClientName\"");
            }

            var dt = connectionFactory.FillDataTable(sb.ToString(), sqlParams);

            return (from DataRow dr in dt.Rows
                    select new Delivered()
                    {
                        ClientId = dr["ClientCode"].ToString(),
                        ClientName = dr["ClientName"].ToString(),
                        StatementDate = Convert.ToDateTime(dr["StatementDate"].ToString()),
                        ContactEmail = dr["Recepients"].ToString(),
                        ContactEmailD = dr["RecepientsD"].ToString(),
                    }).ToList();
        }

        public IEnumerable<Delivered> GetDeliveredAll(string clientName = "", string dateSent = "", string dateSort = "")
        {
            var isPostgres = connectionFactory.IsPostgreSql;
            var sb = new StringBuilder();
            var sqlParams = new List<SqlParameter>();

            if (isPostgres)
            {
                sb.Append(@"SELECT ""Id"", ""ClientCode"", ""ClientName"", (""StatementDate"" - INTERVAL '1 day') AS ""StatementDate"", ""Recepients"", ""CC""
                            FROM dbo.""tblScheduledMail""
                            WHERE ""SentDate"" IS NOT NULL
                              AND ""ClientCode"" IN (SELECT a.""ARClientCode"" FROM dbo.""tbBCATData"" a WHERE a.""StatementDate"" = dbo.""tblScheduledMail"".""StatementDate"" GROUP BY a.""ARClientCode"" HAVING SUM(a.""Peso"") >= 100)");

                if (!string.IsNullOrWhiteSpace(clientName))
                {
                    sb.Append(@" AND TRIM(""ClientName"") ILIKE @ClientName");
                    sqlParams.Add(new SqlParameter("@ClientName", clientName + "%"));
                }
            }
            else
            {
                sb.Append(@"SELECT [Id],[ClientCode],[ClientName],dateadd(DAY,-1,[StatementDate]) as [StatementDate],[Recepients],[CC]
                            FROM [tblScheduledMail]
                            WHERE SentDate is not null
                              AND clientcode in (select a.ARClientCode from [tbBCATData] as a where a.StatementDate=tblScheduledMail.StatementDate group by a.ARClientCode having sum(a.Peso)>=100)");

                if (!string.IsNullOrWhiteSpace(clientName))
                {
                    sb.Append(@" AND ltrim(ClientName) like @ClientName");
                    sqlParams.Add(new SqlParameter("@ClientName", clientName + "%"));
                }
            }

            if (!string.IsNullOrWhiteSpace(dateSent))
            {
                string[] myarrDate = dateSent.Split("-");
                sb.Append(@" AND ""StatementDate"" BETWEEN @StatementDateF AND @StatementDateT");
                sqlParams.Add(new SqlParameter("@StatementDateF", Convert.ToDateTime(myarrDate[0])));
                sqlParams.Add(new SqlParameter("@StatementDateT", Convert.ToDateTime(myarrDate[1])));
            }

            if (!string.IsNullOrWhiteSpace(dateSort) && dateSort != "undefined")
            {
                switch (dateSort)
                {
                    case "Newest - Oldest":
                        sb.Append(" ORDER BY \"StatementDate\" DESC");
                        break;
                    default:
                        sb.Append(" ORDER BY \"StatementDate\"");
                        break;
                }
            }
            else
            {
                sb.Append(" ORDER BY \"ClientName\"");
            }

            var dt = connectionFactory.FillDataTable(sb.ToString(), sqlParams);

            return (from DataRow dr in dt.Rows
                    select new Delivered()
                    {
                        ClientId = dr["ClientCode"].ToString(),
                        ClientName = dr["ClientName"].ToString(),
                        StatementDate = Convert.ToDateTime(dr["StatementDate"].ToString()),
                        ContactEmail = dr["Recepients"].ToString()
                    }).ToList();
        }
public IEnumerable<MailPercentageStatus> GetMailPercentageStatuses(int offset_parameter = 0)
        {
            var isPostgres = connectionFactory.IsPostgreSql;
            var sqlParams = new List<SqlParameter>();

            string sql;
            if (isPostgres)
            {
                sql = @"
                SELECT ""ScheduledDateTime"", ""StatementDate"", ROUND(CAST((""TotalSent"" / NULLIF(""Total"", 0)) * 100 AS numeric), 2) AS ""Percentage""
                FROM (
                    SELECT ""ScheduledDateTime"", ""StatementDate"",
                        CAST(COALESCE((SELECT COUNT(*) FROM dbo.""tblScheduledMail"" WHERE ""ScheduledDateTime"" = a.""ScheduledDateTime"" AND (""Status"" IS NULL OR ""Status"" = '') GROUP BY ""ScheduledDateTime""), 0) AS float) AS ""Total"",
                        CAST(COALESCE((SELECT COUNT(*) FROM dbo.""tblScheduledMail"" WHERE ""ScheduledDateTime"" = a.""ScheduledDateTime"" AND (""Status"" IS NULL OR ""Status"" = '') AND ""SentDate"" IS NOT NULL GROUP BY ""ScheduledDateTime""), 0) AS float) AS ""TotalSent""
                    FROM dbo.""vwScheduledSendDates"" a
                ) v";
            }
            else
            {
                sql = @"
                SELECT ScheduledDateTime, StatementDate, round((TotalSent/NULLIF(Total,0)) * 100, 2) AS [Percentage]
                FROM (
                    SELECT ScheduledDateTime, StatementDate,
                        cast(isnull((SELECT count(*) FROM [tblScheduledMail] WHERE ScheduledDateTime=a.ScheduledDateTime AND ([Status] IS NULL OR [Status]='') GROUP BY ScheduledDateTime), 0) AS float) AS Total,
                        cast(isnull((SELECT count(*) FROM [tblScheduledMail] WHERE ScheduledDateTime=a.ScheduledDateTime AND ([Status] IS NULL OR [Status]='') AND SentDate IS NOT NULL GROUP BY ScheduledDateTime), 0) AS float) AS TotalSent
                    FROM vwScheduledSendDates a
                ) v";
            }

            var dt = connectionFactory.FillDataTable(sql, sqlParams);

            var result = (from DataRow dr in dt.Rows
                          select new MailPercentageStatus()
                          {
                              DeliveryRate = float.Parse(dr["Percentage"].ToString()),
                              StatementDate = Convert.ToDateTime(dr["StatementDate"].ToString()),
                          }).ToList();

            if (result.Any())
            {
                result[0].Percentage_Total = Display_Percentage_Totals();
                result[0].TotalPages = Convert.ToInt32(Math.Ceiling(Convert.ToDecimal(result[0].Percentage_Total) / 10));
            }

            return result;
        }

        public IEnumerable<PHEntityList> GetPHEntityLists(string ClientId, string StatementDate)
        {
            var isPostgres = connectionFactory.IsPostgreSql;
            var sql = isPostgres ? @"
                SELECT DISTINCT b.""EntityCode"", a.""vcPracticeName"", c.""AccountName"", b.""ReportHierarchy""
                FROM dbo.""tbBCATData"" a
                INNER JOIN dbo.""PHEntity"" b ON a.""vcPracticeName"" = b.""EntityName""
                INNER JOIN dbo.""Banks"" c ON b.""EntityCode"" = c.""AccountCode""
                WHERE a.""ARClientCode"" = @ClientId AND a.""StatementDate"" = CAST(@StatementDate AS date)
                ORDER BY b.""ReportHierarchy"""
            : @"
                SELECT DISTINCT b.EntityCode, a.vcPracticeName, c.AccountName, b.ReportHierarchy
                FROM [tbBCATData] a
                INNER JOIN PHEntity b ON a.vcPracticeName = b.EntityName
                INNER JOIN [Banks] c ON b.EntityCode = c.AccountCode
                WHERE a.ARClientCode = @ClientId AND a.StatementDate = @StatementDate
                ORDER BY b.ReportHierarchy";

            var dt = connectionFactory.FillDataTable(sql, new { ClientId, StatementDate });

            return (from DataRow dr in dt.Rows
                    select new PHEntityList()
                    {
                        EntityCode = dr["EntityCode"].ToString(),
                        EntityName = dr["vcPracticeName"].ToString(),
                        EntityNameD = dr["AccountName"].ToString()
                    }).ToList();
        }

        public DateTime? GetRecentStatementDate(string ClientId)
        {
            var isPostgres = connectionFactory.IsPostgreSql;
            var sql = isPostgres ? @"
                SELECT ""StatementDate"" FROM dbo.""tblScheduledMail""
                WHERE ""ClientCode"" = @ClientId AND CURRENT_TIMESTAMP < ""ScheduledDateTime""
                ORDER BY ""StatementDate"" DESC LIMIT 1"
            : @"
                SELECT TOP 1 StatementDate FROM [tblScheduledMail]
                WHERE ClientCode = @ClientId AND GETDATE() < ScheduledDateTime
                ORDER BY StatementDate DESC";

            var dt = connectionFactory.FillDataTable(sql, new { ClientId });

            if (dt.Rows.Count == 0)
            {
                return null;
            }
            return Convert.ToDateTime(dt.Rows[0]["StatementDate"].ToString());
        }

        public IEnumerable<StatementOfAccounts> GetStatementOfAccounts(string ClientId)
        {
            var isPostgres = connectionFactory.IsPostgreSql;
            var sb = new StringBuilder();

            if (isPostgres)
            {
                sb.Append(@"SELECT ""AgeBracket"", ""BillNo"", ""TransDate"", ""Age"", ""ARClientCode"", ""ARClientName"", ""StaffName"", ""chBillEntityCode"", ""vcPracticeName"", ""Dollar"", ""Peso"", ""Manager"", ""Remarks"", ""RunDate"", ""Currency"", ""StatementDate"", ""ReferenceNo"" FROM dbo.""tbBCATData""");
                if (!string.IsNullOrWhiteSpace(ClientId))
                {
                    sb.Append(@" WHERE ""ARClientCode"" = @ClientId");
                }
                sb.Append(@" ORDER BY ""TransDate""");
            }
            else
            {
                sb.Append(@"SELECT [AgeBracket], [BillNo], [TransDate], [Age], [ARClientCode], [ARClientName], [StaffName], [chBillEntityCode], [vcPracticeName], [Dollar], [Peso], [Manager], [Remarks], [RunDate], [Currency], [StatementDate], [ReferenceNo] FROM [tbBCATData]");
                if (!string.IsNullOrWhiteSpace(ClientId))
                {
                    sb.Append(@" WHERE ARClientCode = @ClientId");
                }
                sb.Append(@" ORDER BY TransDate");
            }

            var dt = connectionFactory.FillDataTable(sb.ToString(), new { ClientId });

            return (from DataRow dr in dt.Rows
                    select new StatementOfAccounts()
                    {
                        ClientId = dr["ARClientCode"].ToString(),
                        ClientName = dr["ARClientName"].ToString(),
                        Age = Convert.ToInt32(dr["Age"].ToString()),
                        BillDescription = "",
                        InvoiceDate = Convert.ToDateTime(dr["TransDate"].ToString()).ToString("MM/dd/yyyy"),
                        InvoiceNo = dr["BillNo"].ToString(),
                        ReferenceNo = dr["ReferenceNo"].ToString(),
                        AmountInOtherCurr = Math.Round(Convert.ToDecimal(dr["Dollar"].ToString()), 2),
                        AmountInPeso = Math.Round(Convert.ToDecimal(dr["Peso"].ToString()), 2),
                        EntityCode = dr["chBillEntityCode"].ToString(),
                        OtherCurrCode = dr["Currency"].ToString(),
                        StatementDate = Convert.ToDateTime(dr["StatementDate"].ToString()).ToString("dd MMM yyyy"),
                    }).ToList();
        }

        public IEnumerable<StatementOfAccounts> GetStatementOfAccounts(string ClientId, string StatementDate)
        {
            var isPostgres = connectionFactory.IsPostgreSql;
            var sql = isPostgres ? @"
                SELECT ""AgeBracket"", ""BillNo"", ""TransDate"", ""Age"", ""ARClientCode"", ""ARClientName"", ""StaffName"", ""chBillEntityCode"", ""vcPracticeName"", ""Dollar"", ""Peso"", ""Manager"", ""Remarks"", ""RunDate"", ""Currency"", ""StatementDate"", ""ReferenceNo""
                FROM dbo.""tbBCATData""
                WHERE ""ARClientCode"" = @ClientId AND ""StatementDate"" = CAST(@StatementDate AS date)
                ORDER BY ""TransDate"""
            : @"
                SELECT [AgeBracket], [BillNo], [TransDate], [Age], [ARClientCode], [ARClientName], [StaffName], [chBillEntityCode], [vcPracticeName], [Dollar], [Peso], [Manager], [Remarks], [RunDate], [Currency], [StatementDate], ReferenceNo
                FROM [tbBCATData]
                WHERE ARClientCode = @ClientId AND StatementDate = @StatementDate
                ORDER BY TransDate";

            var dt = connectionFactory.FillDataTable(sql, new { ClientId, StatementDate });

            return (from DataRow dr in dt.Rows
                    select new StatementOfAccounts()
                    {
                        ClientId = dr["ARClientCode"].ToString(),
                        ClientName = dr["ARClientName"].ToString(),
                        Age = Convert.ToInt32(dr["Age"].ToString()),
                        BillDescription = "",
                        InvoiceDate = Convert.ToDateTime(dr["TransDate"].ToString()).ToString("MM/dd/yyyy"),
                        InvoiceNo = dr["BillNo"].ToString(),
                        ReferenceNo = dr["ReferenceNo"].ToString(),
                        AmountInOtherCurr = Math.Round(Convert.ToDecimal(dr["Dollar"].ToString()), 2),
                        AmountInPeso = Math.Round(Convert.ToDecimal(dr["Peso"].ToString()), 2),
                        EntityCode = dr["chBillEntityCode"].ToString(),
                        OtherCurrCode = dr["Currency"].ToString(),
                        StatementDate = Convert.ToDateTime(dr["StatementDate"].ToString()).ToString("dd MMM yyyy"),
                    }).ToList();
        }

        public IEnumerable<Undelivered> GetUndelivered(string clientName = "", string dateSent = "", string dateSort = "")
        {
            var isPostgres = connectionFactory.IsPostgreSql;
            var sb = new StringBuilder();
            var sqlParams = new List<SqlParameter>();

            if (isPostgres)
            {
                sb.Append(@"SELECT ""Id"", ""ClientCode"", ""ClientName"", (""StatementDate"" - INTERVAL '1 day') AS ""StatementDate"", ""Recepients"", ""CC"", split_part(""Recepients"", ',', 1) AS ""RecepientsD""
                            FROM dbo.""tblScheduledMail""
                            WHERE ""SentDate"" IS NULL AND (""Status"" IS NULL OR ""Status"" = '')
                              AND ""ClientCode"" IN (SELECT a.""ARClientCode"" FROM dbo.""tbBCATData"" a WHERE a.""StatementDate"" = dbo.""tblScheduledMail"".""StatementDate"" GROUP BY a.""ARClientCode"" HAVING SUM(a.""Peso"") >= 100)");

                if (!string.IsNullOrWhiteSpace(clientName))
                {
                    sb.Append(@" AND TRIM(""ClientName"") ILIKE @ClientName");
                    sqlParams.Add(new SqlParameter("@ClientName", clientName + "%"));
                }
            }
            else
            {
                sb.Append(@"SELECT TOP 1000 [Id],[ClientCode],[ClientName],dateadd(DAY,-1,[StatementDate]) as [StatementDate],[Recepients],[CC],(SELECT top 1 value FROM string_split(tblScheduledMail.Recepients,',')) as RecepientsD
                            FROM [tblScheduledMail]
                            WHERE SentDate is null AND ([Status] is null or [Status]='')
                              AND clientcode in (select a.ARClientCode from [tbBCATData] as a where a.StatementDate=tblScheduledMail.StatementDate group by a.ARClientCode having sum(a.Peso)>=100)");

                if (!string.IsNullOrWhiteSpace(clientName))
                {
                    sb.Append(@" AND ltrim(ClientName) like @ClientName");
                    sqlParams.Add(new SqlParameter("@ClientName", clientName + "%"));
                }
            }

            if (!string.IsNullOrWhiteSpace(dateSent))
            {
                string[] myarrDate = dateSent.Split("-");
                sb.Append(@" AND ""StatementDate"" BETWEEN @StatementDateF AND @StatementDateT");
                sqlParams.Add(new SqlParameter("@StatementDateF", Convert.ToDateTime(myarrDate[0])));
                sqlParams.Add(new SqlParameter("@StatementDateT", Convert.ToDateTime(myarrDate[1])));
            }

            if (!string.IsNullOrWhiteSpace(dateSort) && dateSort != "undefined")
            {
                switch (dateSort)
                {
                    case "Newest - Oldest":
                        sb.Append(" ORDER BY \"StatementDate\" DESC");
                        break;
                    default:
                        sb.Append(" ORDER BY \"StatementDate\"");
                        break;
                }
            }
            else
            {
                sb.Append(" ORDER BY \"ClientName\"");
            }

            var dt = connectionFactory.FillDataTable(sb.ToString(), sqlParams);

            return (from DataRow dr in dt.Rows
                    select new Undelivered()
                    {
                        ClientId = dr["ClientCode"].ToString(),
                        ClientName = dr["ClientName"].ToString(),
                        StatementDate = Convert.ToDateTime(dr["StatementDate"].ToString()),
                        ContactEmail = dr["Recepients"].ToString(),
                        ContactEmailD = dr["RecepientsD"].ToString(),
                    }).ToList();
        }

        public IEnumerable<Undelivered> GetUndeliveredAll(string clientName = "", string dateSent = "", string dateSort = "")
        {
            var isPostgres = connectionFactory.IsPostgreSql;
            var sb = new StringBuilder();
            var sqlParams = new List<SqlParameter>();

            if (isPostgres)
            {
                sb.Append(@"SELECT ""Id"", ""ClientCode"", ""ClientName"", (""StatementDate"" - INTERVAL '1 day') AS ""StatementDate"", ""Recepients"", ""CC""
                            FROM dbo.""tblScheduledMail""
                            WHERE ""SentDate"" IS NULL AND (""Status"" IS NULL OR ""Status"" = '')
                              AND ""ClientCode"" IN (SELECT a.""ARClientCode"" FROM dbo.""tbBCATData"" a WHERE a.""StatementDate"" = dbo.""tblScheduledMail"".""StatementDate"" GROUP BY a.""ARClientCode"" HAVING SUM(a.""Peso"") >= 100)");

                if (!string.IsNullOrWhiteSpace(clientName))
                {
                    sb.Append(@" AND TRIM(""ClientName"") ILIKE @ClientName");
                    sqlParams.Add(new SqlParameter("@ClientName", clientName + "%"));
                }
            }
            else
            {
                sb.Append(@"SELECT [Id],[ClientCode],[ClientName],dateadd(DAY,-1,[StatementDate]) as [StatementDate],[Recepients],[CC]
                            FROM [tblScheduledMail]
                            WHERE SentDate is null AND ([Status] is null or [Status]='')
                              AND clientcode in (select a.ARClientCode from [tbBCATData] as a where a.StatementDate=tblScheduledMail.StatementDate group by a.ARClientCode having sum(a.Peso)>=100)");

                if (!string.IsNullOrWhiteSpace(clientName))
                {
                    sb.Append(@" AND ltrim(ClientName) like @ClientName");
                    sqlParams.Add(new SqlParameter("@ClientName", clientName + "%"));
                }
            }

            if (!string.IsNullOrWhiteSpace(dateSent))
            {
                string[] myarrDate = dateSent.Split("-");
                sb.Append(@" AND ""StatementDate"" BETWEEN @StatementDateF AND @StatementDateT");
                sqlParams.Add(new SqlParameter("@StatementDateF", Convert.ToDateTime(myarrDate[0])));
                sqlParams.Add(new SqlParameter("@StatementDateT", Convert.ToDateTime(myarrDate[1])));
            }

            if (!string.IsNullOrWhiteSpace(dateSort) && dateSort != "undefined")
            {
                switch (dateSort)
                {
                    case "Newest - Oldest":
                        sb.Append(" ORDER BY \"StatementDate\" DESC");
                        break;
                    default:
                        sb.Append(" ORDER BY \"StatementDate\"");
                        break;
                }
            }
            else
            {
                sb.Append(" ORDER BY \"ClientName\"");
            }

            var dt = connectionFactory.FillDataTable(sb.ToString(), sqlParams);

            return (from DataRow dr in dt.Rows
                    select new Undelivered()
                    {
                        ClientId = dr["ClientCode"].ToString(),
                        ClientName = dr["ClientName"].ToString(),
                        StatementDate = Convert.ToDateTime(dr["StatementDate"].ToString()),
                        ContactEmail = dr["Recepients"].ToString()
                    }).ToList();
        }

        public void UpdateContacts(string ClientId, DateTime StatementDate)
        {
            var isPostgres = connectionFactory.IsPostgreSql;
            var sql = isPostgres ? @"
                UPDATE dbo.""tblScheduledMail"" m
                SET ""Recepients"" = (SELECT string_agg(""Email"", ', ') FROM dbo.""Contacts"" WHERE ""ClientCode"" = m.""ClientCode"")
                WHERE ""ClientCode"" = @ClientId AND ""StatementDate"" = CAST(@StatementDate AS date)"
            : @"
                UPDATE [tblScheduledMail]
                SET Recepients = (SELECT SUBSTRING((SELECT ', ' + Email FROM Contacts WHERE ClientCode = [tblScheduledMail].ClientCode FOR XML PATH('')), 2, 9999))
                WHERE ClientCode = @ClientId AND StatementDate = @StatementDate";

            using var conn = connectionFactory.CreateConnection();
            conn.Execute(sql, new { ClientId, StatementDate });
        }

        public void UpdateEngagements(string ClientId, DateTime StatementDate)
        {
            var isPostgres = connectionFactory.IsPostgreSql;
            var sql = isPostgres ? @"
                UPDATE dbo.""tblScheduledMail"" m
                SET ""CC"" = (SELECT string_agg(""Email"", ', ') FROM dbo.""EngagementTeams"" WHERE ""ClientCode"" = m.""ClientCode"")
                WHERE ""ClientCode"" = @ClientId AND ""StatementDate"" = CAST(@StatementDate AS date)"
            : @"
                UPDATE [tblScheduledMail]
                SET CC = (SELECT SUBSTRING((SELECT ', ' + Email FROM EngagementTeams WHERE ClientCode = [tblScheduledMail].ClientCode FOR XML PATH('')), 2, 9999))
                WHERE ClientCode = @ClientId AND StatementDate = @StatementDate";

            using var conn = connectionFactory.CreateConnection();
            conn.Execute(sql, new { ClientId, StatementDate });
        }

        public void UpdateContacts_V2(string ClientId, DateTime StatementDate)
        {
            var isPostgres = connectionFactory.IsPostgreSql;
            var sql = isPostgres ? @"
                UPDATE dbo.""tblScheduledMail""
                SET ""Recepients"" = (SELECT string_agg(""Email"", ', ') FROM dbo.""Contacts"" WHERE ""ClientCode"" = @ClientId)
                WHERE ""ClientCode"" = @ClientId AND ""StatementDate"" = CAST(@StatementDate AS date)"
            : @"
                UPDATE [tblScheduledMail]
                SET Recepients = (SELECT SUBSTRING((SELECT ', ' + Email FROM Contacts WHERE ClientCode = @ClientId FOR XML PATH('')), 2, 9999))
                WHERE ClientCode = @ClientId AND StatementDate = @StatementDate";

            var sqlParams = new { ClientId, StatementDate };

            using var conn = connectionFactory.CreateConnection();
            conn.Execute(sql, sqlParams);
        }

        public void UpdateEngagements_V2(string ClientId, DateTime StatementDate)
        {
            var isPostgres = connectionFactory.IsPostgreSql;
            var sql = isPostgres ? @"
                UPDATE dbo.""tblScheduledMail""
                SET ""CC"" = (SELECT string_agg(""Email"", ', ') FROM dbo.""EngagementTeams"" WHERE ""ClientCode"" = @ClientId)
                WHERE ""ClientCode"" = @ClientId AND ""StatementDate"" = CAST(@StatementDate AS date)"
            : @"
                UPDATE [tblScheduledMail]
                SET CC = (SELECT SUBSTRING((SELECT ', ' + Email FROM EngagementTeams WHERE ClientCode = @ClientId FOR XML PATH('')), 2, 9999))
                WHERE ClientCode = @ClientId AND StatementDate = @StatementDate";

            var sqlParams = new { ClientId, StatementDate };

            using var conn = connectionFactory.CreateConnection();
            conn.Execute(sql, sqlParams);
        }

        public void UpdateSendDate(string ClientCode, string StatementDate)
        {
            var isPostgres = connectionFactory.IsPostgreSql;
            var sql = isPostgres ? @"
                UPDATE dbo.""tblScheduledMail""
                SET ""SentDate"" = CURRENT_TIMESTAMP
                WHERE ""ClientCode"" = @ClientCode AND ""StatementDate"" = CAST(@StatementDate AS date)"
            : @"
                UPDATE [tblScheduledMail]
                SET [SentDate] = GETDATE()
                WHERE [ClientCode] = @ClientCode AND [StatementDate] = @StatementDate";

            using var conn = connectionFactory.CreateConnection();
            conn.Execute(sql, new { ClientCode, StatementDate });
        }

        public void UpdateTblScheduleMailStatus(string ClientId, string myStatus)
        {
            var isPostgres = connectionFactory.IsPostgreSql;
            var sql = isPostgres ? @"
                UPDATE dbo.""tblScheduledMail""
                SET ""Status"" = @myStatus
                WHERE ""ClientCode"" = @ClientId AND ""StatementDate"" = (SELECT ""StatementDate"" FROM dbo.""tblScheduledMail"" WHERE CURRENT_TIMESTAMP < ""ScheduledDateTime"" ORDER BY ""StatementDate"" DESC LIMIT 1)"
            : @"
                UPDATE [tblScheduledMail]
                SET [Status] = @myStatus
                WHERE [ClientCode] = @ClientId AND [StatementDate] = (SELECT TOP 1 StatementDate FROM tblScheduledMail WHERE GETDATE() < ScheduledDateTime ORDER BY StatementDate DESC)";

            using var conn = connectionFactory.CreateConnection();
            conn.Execute(sql, new { ClientId, myStatus });
        }

        public IEnumerable<Delivered_Inside> GetDelivered_New(string clientName = "", string dateSent = "", string dateSort = "", int variable_offset = 0)
        {
            var isPostgres = connectionFactory.IsPostgreSql;
            var sb = new StringBuilder();
            var sqlParams = new List<SqlParameter>();

            if (isPostgres)
            {
                sb.Append(@"SELECT sm.""ClientCode"", sm.""ClientName"", CAST(sm.""StatementDate"" AS date) AS ""StatementDate""");
                sb.Append(@" FROM dbo.""tblScheduledMail"" sm");
                sb.Append(@" JOIN dbo.""tbBCATData"" bct ON bct.""ARClientCode"" = sm.""ClientCode"" AND RIGHT(sm.""BillNo"", LENGTH(sm.""BillNo"") - 3) = bct.""BillNo"" AND bct.""StatementDate"" = CAST(sm.""StatementDate"" AS date)");
                sb.Append(@" WHERE sm.""SentDate"" IS NOT NULL AND sm.""ReferenceNo"" IS NOT NULL");
            }
            else
            {
                sb.Append(@"SELECT sm.ClientCode, sm.ClientName, CAST(sm.StatementDate AS date) StatementDate");
                sb.Append(@" FROM dbo.[tblScheduledMail] sm");
                sb.Append(@" JOIN dbo.[tbBCATData] bct ON bct.ARClientCode = sm.ClientCode AND RIGHT(sm.BillNo, LEN(sm.BillNo) - 3) = bct.BillNo AND bct.StatementDate = CAST(sm.StatementDate AS date)");
                sb.Append(@" WHERE sm.[SentDate] IS NOT NULL AND sm.[ReferenceNo] IS NOT NULL");
            }

            sqlParams.Add(new SqlParameter("@offset_variable", variable_offset));

            if (clientName != "" && clientName != null)
            {
                if (isPostgres)
                {
                    sb.Append(@" AND TRIM(sm.""ClientName"") ILIKE @ClientName");
                }
                else
                {
                    sb.Append(@" AND LTRIM(RTRIM(sm.ClientName)) LIKE @ClientName");
                }
                sqlParams.Add(new SqlParameter("@ClientName", "%" + clientName + "%"));
            }

            if (dateSent != "" && dateSent != null)
            {
                string[] myarrDate = dateSent.Split("-");
                if (isPostgres)
                {
                    sb.Append(@" AND sm.""StatementDate"" BETWEEN @StatementDateF AND @StatementDateT");
                }
                else
                {
                    sb.Append(@" AND sm.StatementDate BETWEEN @StatementDateF AND @StatementDateT");
                }
                sqlParams.Add(new SqlParameter("@StatementDateF", Convert.ToDateTime(myarrDate[0])));
                sqlParams.Add(new SqlParameter("@StatementDateT", Convert.ToDateTime(myarrDate[1])));
            }

            if (dateSort != "" && dateSort != null && dateSort != "undefined")
            {
                switch (dateSort)
                {
                    case "Newest - Oldest":
                        if (isPostgres)
                        {
                            sb.Append(@" GROUP BY sm.""ClientCode"", sm.""ClientName"", sm.""StatementDate"" ORDER BY sm.""ClientName"", sm.""ClientCode"" OFFSET @offset_variable ROWS FETCH NEXT 10 ROWS ONLY");
                        }
                        else
                        {
                            sb.Append(@" GROUP BY sm.ClientCode, sm.ClientName, sm.StatementDate ORDER BY sm.ClientName OFFSET @offset_variable ROWS FETCH NEXT 10 ROWS ONLY");
                        }
                        break;
                    default:
                        if (isPostgres)
                        {
                            sb.Append(@" GROUP BY sm.""ClientCode"", sm.""ClientName"", sm.""StatementDate"" ORDER BY sm.""ClientName"" OFFSET @offset_variable ROWS FETCH NEXT 10 ROWS ONLY");
                        }
                        else
                        {
                            sb.Append(@" GROUP BY sm.ClientCode, sm.ClientName, sm.StatementDate ORDER BY sm.ClientName OFFSET @offset_variable ROWS FETCH NEXT 10 ROWS ONLY");
                        }
                        break;
                }
            }
            else
            {
                if (isPostgres)
                {
                    sb.Append(@" GROUP BY sm.""ClientCode"", sm.""ClientName"", sm.""StatementDate"" ORDER BY sm.""ClientName"" OFFSET @offset_variable ROWS FETCH NEXT 10 ROWS ONLY");
                }
                else
                {
                    sb.Append(@" GROUP BY sm.ClientCode, sm.ClientName, sm.StatementDate ORDER BY sm.ClientName OFFSET @offset_variable ROWS FETCH NEXT 10 ROWS ONLY");
                }
            }

            var dt = connectionFactory.FillDataTable(sb.ToString(), sqlParams);

            var result = (from DataRow dr in dt.Rows
                          select new Delivered_Inside()
                          {
                              ClientId = dr["ClientCode"].ToString(),
                              ClientName = dr["ClientName"].ToString(),
                              Sent_Out_Date = "",
                              Statement_Date = dr["StatementDate"].ToString(),
                              Delivered_Count = Display_Delivered_Counter(clientName, dr["StatementDate"].ToString()),
                              Delivered_Total = Display_Delivered_Counter_Total(clientName, dr["StatementDate"].ToString())
                          }).ToList();

            if (result.Count > 0)
            {
                result[0].TotalPages = Convert.ToInt32(Math.Ceiling(Convert.ToDecimal((result[0].Delivered_Total / 10.0) % 1 == 0 ? (result[0].Delivered_Total / 10.0) + 1 : (result[0].Delivered_Total / 10.0))));
            }

            foreach (var item in result)
            {
                item.delivered_Insides = GetDeliveredInsideDetails(item.ClientId, item.Statement_Date);
            }

            return result;
        }

        public IEnumerable<Undelivered_Inside> GetUndelivered_New(string clientName = "", string dateSent = "", string dateSort = "", int variable_offset = 0)
        {
            var isPostgres = connectionFactory.IsPostgreSql;
            var sb = new StringBuilder();
            var sqlParams = new List<SqlParameter>();

            if (isPostgres)
            {
                sb.Append(@"SELECT sm.""ClientCode"", sm.""ClientName"", CAST(sm.""StatementDate"" AS date) AS ""StatementDate""");
                sb.Append(@" FROM dbo.""tblScheduledMail"" sm");
                sb.Append(@" JOIN dbo.""tbBCATData"" bct ON bct.""ARClientCode"" = sm.""ClientCode"" AND RIGHT(sm.""BillNo"", LENGTH(sm.""BillNo"") - 3) = bct.""BillNo"" AND bct.""StatementDate"" = CAST(sm.""StatementDate"" AS date)");
                sb.Append(@" WHERE sm.""SentDate"" IS NULL");
                sb.Append(@" AND sm.""ClientCode"" NOT IN (SELECT ""ClientCode"" FROM dbo.""Exceptions"" WHERE ""ClientCode"" = sm.""ClientCode"" AND ""Reference_No"" = sm.""ReferenceNo"")");
            }
            else
            {
                sb.Append(@"SELECT sm.ClientCode, sm.ClientName, CAST(sm.StatementDate AS date) AS StatementDate");

                sb.Append(@" FROM dbo.[tblScheduledMail] sm");
                sb.Append(@" JOIN dbo.[tbBCATData] bct ON bct.ARClientCode = sm.ClientCode AND RIGHT(sm.BillNo, LEN(sm.BillNo) - 3) = bct.BillNo AND bct.StatementDate = CAST(sm.StatementDate AS date)");
                sb.Append(@" WHERE sm.[SentDate] IS NULL");
                sb.Append(@" AND sm.ClientCode NOT IN (SELECT [ClientCode] FROM dbo.[Exceptions] WHERE [ClientCode] = sm.[ClientCode] AND [Reference_No] = sm.ReferenceNo)");
            }

            sqlParams.Add(new SqlParameter("@offset_variable", variable_offset));

            if (clientName != "" && clientName != null)
            {
                if (isPostgres)
                {
                    sb.Append(@" AND TRIM(sm.""ClientName"") ILIKE @ClientName");
                }
                else
                {
                    sb.Append(@" AND LTRIM(RTRIM(sm.ClientName)) LIKE @ClientName");
                }
                sqlParams.Add(new SqlParameter("@ClientName", "%" + clientName + "%"));
            }

            if (dateSent != "" && dateSent != null)
            {
                string[] myarrDate = dateSent.Split("-");
                if (isPostgres)
                {
                    sb.Append(@" AND sm.""StatementDate"" BETWEEN @StatementDateF AND @StatementDateT");
                }
                else
                {
                    sb.Append(@" AND sm.StatementDate BETWEEN @StatementDateF AND @StatementDateT");
                }
                sqlParams.Add(new SqlParameter("@StatementDateF", Convert.ToDateTime(myarrDate[0])));
                sqlParams.Add(new SqlParameter("@StatementDateT", Convert.ToDateTime(myarrDate[1])));
            }

            if (dateSort != "" && dateSort != null && dateSort != "undefined")
            {
                switch (dateSort)
                {
                    case "Newest - Oldest":
                        if (isPostgres)
                        {
                            sb.Append(@" GROUP BY sm.""ClientCode"", sm.""ClientName"", sm.""StatementDate"" HAVING SUM(bct.""Peso"") >= 100 ORDER BY sm.""ClientName"" OFFSET @offset_variable ROWS FETCH NEXT 10 ROWS ONLY");
                        }
                        else
                        {
                            sb.Append(@" GROUP BY sm.ClientCode, sm.ClientName, sm.StatementDate HAVING SUM(bct.Peso) >= 100 ORDER BY sm.ClientName OFFSET @offset_variable ROWS FETCH NEXT 10 ROWS ONLY");
                        }
                        break;
                    default:
                        if (isPostgres)
                        {
                            sb.Append(@" GROUP BY sm.""ClientCode"", sm.""ClientName"", sm.""StatementDate"" HAVING SUM(bct.""Peso"") >= 100 ORDER BY sm.""ClientName"" OFFSET @offset_variable ROWS FETCH NEXT 10 ROWS ONLY");
                        }
                        else
                        {
                            sb.Append(@" GROUP BY sm.ClientCode, sm.ClientName, sm.StatementDate HAVING SUM(bct.Peso) >= 100 ORDER BY sm.ClientName OFFSET @offset_variable ROWS FETCH NEXT 10 ROWS ONLY");
                        }
                        break;
                }
            }
            else
            {
                if (isPostgres)
                {
                    sb.Append(@" GROUP BY sm.""ClientCode"", sm.""ClientName"", sm.""StatementDate"" HAVING SUM(bct.""Peso"") >= 100 ORDER BY sm.""ClientName"" OFFSET @offset_variable ROWS FETCH NEXT 10 ROWS ONLY");
                }
                else
                {
                    sb.Append(@" GROUP BY sm.ClientCode, sm.ClientName, sm.StatementDate HAVING SUM(bct.Peso) >= 100 ORDER BY sm.ClientName OFFSET @offset_variable ROWS FETCH NEXT 10 ROWS ONLY");
                }
            }

            var dt = connectionFactory.FillDataTable(sb.ToString(), sqlParams);

            var result = (from DataRow dr in dt.Rows
                          select new Undelivered_Inside()
                          {
                              ClientId = dr["ClientCode"].ToString(),
                              ClientName = dr["ClientName"].ToString(),
                              Sent_Out_Date = "",
                              Statement_Date = dr["StatementDate"].ToString(),
                              Undelivered_Count = Display_Undelivered_Counter_No_Search(dr["StatementDate"].ToString()),
                              Undelivered_Total = Display_Undelivered_Counter_No_Search_Total(dr["StatementDate"].ToString())
                          }).ToList();

            if (result.Count > 0)
            {
                result[0].TotalPages = Convert.ToInt32(Math.Ceiling(Convert.ToDecimal((result[0].Undelivered_Total / 10.0) % 1 == 0 ? (result[0].Undelivered_Total / 10.0) + 1 : (result[0].Undelivered_Total / 10.0))));
            }

            foreach (var item in result)
            {
                item.undelivered_Insides = GetUndeliveredInsideDetails(item.ClientId, item.Statement_Date);
            }

            return result;
        }

        public IEnumerable<PHEntityList> GetPHEntityLists_New(string ClientId, string StatementDate, string entity_code_variable)
        {
            var isPostgres = connectionFactory.IsPostgreSql;
            var sb = new StringBuilder();
            var sqlParams = new List<SqlParameter>();

            if (isPostgres)
            {
                sb.Append(@"SELECT DISTINCT x.""chBillEntityCode"", x.""vcPracticeName""");
                sb.Append(@" FROM dbo.""tbBCATData"" x");
                sb.Append(@" JOIN dbo.""PHEntity"" PE ON x.""chBillEntityCode"" = PE.""EntityCode""");
                sb.Append(@" JOIN dbo.""tblBill"" bill ON x.""ARClientCode"" = bill.""chDebtorCode"" AND x.""BillNo"" = bill.""chBillNo""");
                sb.Append(@" WHERE x.""ARClientCode"" = @ClientId AND x.""StatementDate"" = CAST(@StatementDate AS date)");
            }
            else
            {
                sb.Append(@"SELECT DISTINCT x.[chBillEntityCode], x.[vcPracticeName]");
                sb.Append($@" FROM {connectionFactory.BcatTable("tbBCATData")} x");
                sb.Append($@" JOIN {connectionFactory.BcatTable("PHEntity")} PE ON x.[chBillEntityCode] = PE.[EntityCode]");
                sb.Append($@" JOIN {connectionFactory.FinAppsTable("tblBill")} bill ON x.ARClientCode = bill.chDebtorCode AND x.BillNo = bill.chBillNo");
                sb.Append(@" WHERE x.[ARClientCode] = @ClientId AND x.[StatementDate] = @StatementDate");
            }

            sqlParams.Add(new SqlParameter("@ClientId", ClientId));
            sqlParams.Add(new SqlParameter("@StatementDate", StatementDate));

            var dt = connectionFactory.FillDataTable(sb.ToString(), sqlParams);

            return (from DataRow dr in dt.Rows
                    select new PHEntityList()
                    {
                        EntityCode = dr["chBillEntityCode"].ToString(),
                        EntityName = dr["vcPracticeName"].ToString()
                    }).ToList();
        }

        public IEnumerable<PHEntityList> GetPHEntityLists(string ClientId, string StatementDate, bool is_delivered = false, string ex_LOS_variable = "")
        {
            var isPostgres = connectionFactory.IsPostgreSql;
            var sb = new StringBuilder();
            var sqlParams = new List<SqlParameter>();

            if (isPostgres)
            {
                sb.Append(@"SELECT DISTINCT x.""chBillEntityCode"", x.""vcPracticeName"", c.""Email""");
                sb.Append(@" FROM dbo.""tbBCATData"" x");
                sb.Append(@" JOIN dbo.""PHEntity"" PE ON x.""chBillEntityCode"" = PE.""EntityCode""");
                sb.Append(@" JOIN dbo.""tblBill"" bill ON x.""ARClientCode"" = bill.""chDebtorCode"" AND x.""BillNo"" = bill.""chBillNo""");
                sb.Append(@" JOIN dbo.""Contacts"" C ON (x.""ARClientCode"" = C.""ClientCode"" AND CONCAT(bill.""chBillOfficeCode"", bill.""chBillGroupCode"") = C.""GroupType"")");

                if (is_delivered)
                {
                    sb.Append(@" WHERE x.""ARClientCode"" = @ClientId AND x.""StatementDate"" = CAST(@StatementDate AS date) AND C.""Email"" = @LOS AND x.""SentDate"" IS NOT NULL");
                }
                else
                {
                    sb.Append(@" WHERE x.""ARClientCode"" = @ClientId AND x.""StatementDate"" = CAST(@StatementDate AS date) AND C.""Email"" = @LOS AND x.""SentDate"" IS NULL");
                }
            }
            else
            {
                sb.Append(@"SELECT DISTINCT x.[chBillEntityCode], x.[vcPracticeName], C.[Email]");
                sb.Append($@" FROM {connectionFactory.BcatTable("tbBCATData")} x");
                sb.Append($@" JOIN {connectionFactory.BcatTable("PHEntity")} PE ON x.[chBillEntityCode] = PE.[EntityCode]");
                sb.Append($@" JOIN {connectionFactory.FinAppsTable("tblBill")} bill ON x.ARClientCode = bill.chDebtorCode AND x.BillNo = bill.chBillNo");
                sb.Append($@" JOIN {connectionFactory.BcatTable("Contacts")} C ON (x.[ARClientCode] = C.[ClientCode] AND CONCAT(bill.[chBillOfficeCode], bill.[chBillGroupCode]) = C.[GroupType])");

                if (is_delivered)
                {
                    sb.Append(@" WHERE x.[ARClientCode] = @ClientId AND x.[StatementDate] = @StatementDate AND C.[Email] = @LOS AND X.[SentDate] IS NOT NULL");
                }
                else
                {
                    sb.Append(@" WHERE x.[ARClientCode] = @ClientId AND x.[StatementDate] = @StatementDate AND C.[Email] = @LOS AND X.[SentDate] IS NULL");
                }
            }

            sqlParams.Add(new SqlParameter("@ClientId", ClientId));
            sqlParams.Add(new SqlParameter("@StatementDate", StatementDate));
            sqlParams.Add(new SqlParameter("@LOS", ex_LOS_variable));

            var dt = connectionFactory.FillDataTable(sb.ToString(), sqlParams);

            return (from DataRow dr in dt.Rows
                    select new PHEntityList()
                    {
                        EntityCode = dr["chBillEntityCode"].ToString(),
                        EntityName = dr["vcPracticeName"].ToString(),
                        EntityNameD = dr["Email"].ToString()
                    }).ToList();
        }

        private List<Delivered_Inside> GetDeliveredInsideDetails(string ClientId, string StatementDate)
        {
            var isPostgres = connectionFactory.IsPostgreSql;
            var sb = new StringBuilder();
            var sqlParams = new List<SqlParameter>();

            if (isPostgres)
            {
                sb.Append(@"SELECT sm.""ClientCode"", CONCAT(RTRIM(gc.""chOfficeCode""), RTRIM(gc.""chGroupCode""), ' - ', gc.""vcGroupDesc"") AS ""Operating_Unit"", CONCAT(RTRIM(gc.""chOfficeCode""), RTRIM(gc.""chGroupCode"")) AS ""OU_Code""");
                sb.Append(@" , (SELECT Z.""Email"" FROM dbo.""Contacts"" Z WHERE Z.""ClientCode"" = sm.""ClientCode"" AND Z.""GroupType"" = sm.""GroupCode"" ORDER BY Z.""Id"" DESC LIMIT 1) AS ""Email""");
                sb.Append(@" FROM dbo.""tblScheduledMail"" sm");
                sb.Append(@" JOIN dbo.""tbBCATData"" bct ON bct.""ARClientCode"" = sm.""ClientCode"" AND bct.""BillNo"" = sm.""BillNo"" AND bct.""StatementDate"" = CAST(sm.""StatementDate"" AS date)");
                sb.Append(@" JOIN dbo.""Contacts"" C ON sm.""ClientCode"" = C.""ClientCode"" AND sm.""GroupCode"" = C.""GroupType"" AND C.""Email"" IS NOT NULL");
                sb.Append(@" JOIN dbo.""tblGroupCode"" gc ON CONCAT(RTRIM(gc.""chOfficeCode""), RTRIM(gc.""chGroupCode"")) = sm.""GroupCode""");
                sb.Append(@" WHERE sm.""ClientCode"" = @ClientId AND CAST(sm.""StatementDate"" AS date) = CAST(@StatementDate AS date) AND sm.""SentDate"" IS NOT NULL");
                sb.Append(@" GROUP BY sm.""ClientCode"", sm.""ClientName"", gc.""chOfficeCode"", gc.""chGroupCode"", gc.""vcGroupDesc"", sm.""GroupCode""");
                sb.Append(@" ORDER BY sm.""ClientName""");
            }
            else
            {
                sb.Append(@" SELECT sm.ClientCode, CONCAT(RTRIM(gc.chOfficeCode),RTRIM(gc.chGroupCode), ' - ',gc.vcGroupDesc) Operating_Unit, CONCAT(RTRIM(gc.chOfficeCode), RTRIM(gc.chGroupCode)) [OU_Code],");
                sb.Append(@" (SELECT Top 1 Z.[Email] FROM " + connectionFactory.BcatTable("Contacts") + " Z WHERE Z.[ClientCode] = sm.[ClientCode] AND Z.GroupType = sm.GroupCode ORDER BY Z.[Id] DESC) [Email]");
                sb.Append(@" FROM " + connectionFactory.BcatTable("tblScheduledMail_v2") + " sm");
                sb.Append(@" JOIN " + connectionFactory.BcatTable("tbBCATData") + " bct ON bct.ARClientCode = sm.ClientCode AND bct.BillNo = sm.BillNo AND bct.StatementDate = CAST(sm.StatementDate AS date)");
                sb.Append(@" JOIN " + connectionFactory.BcatTable("Contacts") + " C ON sm.[ClientCode] = C.[ClientCode] AND sm.[GroupCode] = C.[GroupType] AND C.[Email] IS NOT NULL");
                sb.Append(@" JOIN " + connectionFactory.DatabankTable("tblGroupCode") + " gc ON CONCAT(RTRIM(gc.chOfficeCode),RTRIM(gc.chGroupCode)) = sm.GroupCode");
                sb.Append(@" WHERE sm.ClientCode = @ClientId AND CAST(sm.StatementDate AS date) = @StatementDate AND sm.[SentDate] IS NOT NULL");
                sb.Append(@" GROUP BY sm.ClientCode, sm.ClientName,gc.chOfficeCode,gc.chGroupCode,gc.vcGroupDesc,gc.chOfficeCode,gc.chGroupCode, sm.GroupCode");
                sb.Append(@" ORDER BY sm.ClientName");
            }

            sqlParams.Add(new SqlParameter("@ClientId", ClientId));
            sqlParams.Add(new SqlParameter("@StatementDate", StatementDate));

            var dt = connectionFactory.FillDataTable(sb.ToString(), sqlParams);
            var result = (from DataRow dr in dt.Rows
                          select new Delivered_Inside()
                          {
                              Contacts_Id = dr["ClientCode"].ToString(),
                              OU = dr["Operating_Unit"].ToString(),
                              Primary_Contact = dr["Email"].ToString(),
                              OU_Code = dr["OU_Code"].ToString()
                          }).ToList();

            return result;
        }

        private List<Undelivered_Inside> GetUndeliveredInsideDetails(string ClientId, string StatementDate)
        {
            var isPostgres = connectionFactory.IsPostgreSql;
            var sb = new StringBuilder();
            var sqlParams = new List<SqlParameter>();

            if (isPostgres)
            {
                sb.Append(@"SELECT sm.""ClientCode"", CONCAT(RTRIM(gc.""chOfficeCode""), RTRIM(gc.""chGroupCode""), ' - ', gc.""vcGroupDesc"") AS ""Operating_Unit"", CONCAT(RTRIM(gc.""chOfficeCode""), RTRIM(gc.""chGroupCode"")) AS ""OU_Code""");
                sb.Append(@" , (SELECT Z.""Email"" FROM dbo.""Contacts"" Z WHERE Z.""ClientCode"" = sm.""ClientCode"" AND Z.""GroupType"" = sm.""GroupCode"" ORDER BY Z.""Id"" DESC LIMIT 1) AS ""Email""");
                sb.Append(@" FROM dbo.""tblScheduledMail"" sm");
                sb.Append(@" JOIN dbo.""tbBCATData"" bct ON bct.""ARClientCode"" = sm.""ClientCode"" AND bct.""BillNo"" = sm.""BillNo"" AND bct.""StatementDate"" = CAST(sm.""StatementDate"" AS date)");
                sb.Append(@" JOIN dbo.""Contacts"" C ON sm.""ClientCode"" = C.""ClientCode"" AND sm.""GroupCode"" = C.""GroupType"" AND C.""Email"" IS NOT NULL");
                sb.Append(@" JOIN dbo.""tblGroupCode"" gc ON CONCAT(RTRIM(gc.""chOfficeCode""), RTRIM(gc.""chGroupCode"")) = sm.""GroupCode""");
                sb.Append(@" WHERE sm.""ClientCode"" = @ClientId AND CAST(sm.""StatementDate"" AS date) = CAST(@StatementDate AS date) AND sm.""SentDate"" IS NULL");
                sb.Append(@" GROUP BY sm.""ClientCode"", sm.""ClientName"", gc.""chOfficeCode"", gc.""chGroupCode"", gc.""vcGroupDesc"", sm.""GroupCode""");
                sb.Append(@" ORDER BY sm.""ClientName""");
            }
            else
            {
                sb.Append(@" SELECT sm.ClientCode, CONCAT(RTRIM(gc.chOfficeCode),RTRIM(gc.chGroupCode), ' - ',gc.vcGroupDesc) Operating_Unit, CONCAT(RTRIM(gc.chOfficeCode), RTRIM(gc.chGroupCode)) [OU_Code],");
                sb.Append(@" (SELECT Top 1 Z.[Email] FROM " + connectionFactory.BcatTable("Contacts") + " Z WHERE Z.[ClientCode] = sm.[ClientCode] AND Z.GroupType = sm.GroupCode ORDER BY Z.[Id] DESC) [Email]");
                sb.Append(@" FROM " + connectionFactory.BcatTable("tblScheduledMail_v2") + " sm");
                sb.Append(@" JOIN " + connectionFactory.BcatTable("tbBCATData") + " bct ON bct.ARClientCode = sm.ClientCode AND bct.BillNo = sm.BillNo AND bct.StatementDate = CAST(sm.StatementDate AS date)");
                sb.Append(@" JOIN " + connectionFactory.BcatTable("Contacts") + " C ON sm.[ClientCode] = C.[ClientCode] AND sm.[GroupCode] = C.[GroupType] AND C.[Email] IS NOT NULL");
                sb.Append(@" JOIN " + connectionFactory.DatabankTable("tblGroupCode") + " gc ON CONCAT(RTRIM(gc.chOfficeCode),RTRIM(gc.chGroupCode)) = sm.GroupCode");
                sb.Append(@" WHERE sm.ClientCode = @ClientId AND CAST(sm.StatementDate AS date) = @StatementDate AND sm.[SentDate] IS NULL");
                sb.Append(@" GROUP BY sm.ClientCode, sm.ClientName,gc.chOfficeCode,gc.chGroupCode,gc.vcGroupDesc,gc.chOfficeCode,gc.chGroupCode, sm.GroupCode");
                sb.Append(@" ORDER BY sm.ClientName");
            }

            sqlParams.Add(new SqlParameter("@ClientId", ClientId));
            sqlParams.Add(new SqlParameter("@StatementDate", StatementDate));

            var dt = connectionFactory.FillDataTable(sb.ToString(), sqlParams);
            var result = (from DataRow dr in dt.Rows
                          select new Undelivered_Inside()
                          {
                              Contacts_Id = dr["ClientCode"].ToString(),
                              OU = dr["Operating_Unit"].ToString(),
                              Primary_Contact = dr["Email"].ToString(),
                              OU_Code = dr["OU_Code"].ToString()
                          }).ToList();

            return result;
        }

        private int Display_Delivered_Counter(string clientName, string statementDate)
        {
            var isPostgres = connectionFactory.IsPostgreSql;
            var sqlParams = new List<SqlParameter>();

            string sql;
            if (isPostgres)
            {
                sql = @"SELECT COUNT(*) FROM dbo.""tblScheduledMail"" sm
                         JOIN dbo.""tbBCATData"" bct ON bct.""ARClientCode"" = sm.""ClientCode"" AND bct.""BillNo"" = sm.""BillNo"" AND bct.""StatementDate"" = CAST(sm.""StatementDate"" AS date)
                         WHERE sm.""SentDate"" IS NOT NULL AND sm.""ReferenceNo"" IS NOT NULL";
                if (clientName != "" && clientName != null)
                {
                    sql += @" AND TRIM(sm.""ClientName"") ILIKE @ClientName";
                    sqlParams.Add(new SqlParameter("@ClientName", "%" + clientName + "%"));
                }
                sql += @" AND CAST(sm.""StatementDate"" AS date) = CAST(@StatementDate AS date) GROUP BY sm.""ClientName""";
                sqlParams.Add(new SqlParameter("@StatementDate", statementDate));
            }
            else
            {
                sql = @"SELECT COUNT(*) FROM dbo.[tblScheduledMail] sm
                         JOIN dbo.[tbBCATData] bct ON bct.ARClientCode = sm.ClientCode AND bct.BillNo = sm.BillNo AND bct.StatementDate = CAST(sm.StatementDate AS date)
                         WHERE sm.[SentDate] IS NOT NULL AND sm.[ReferenceNo] IS NOT NULL";
                if (clientName != "" && clientName != null)
                {
                    sql += @" AND LTRIM(RTRIM(sm.ClientName)) LIKE @ClientName";
                    sqlParams.Add(new SqlParameter("@ClientName", "%" + clientName + "%"));
                }
                sql += @" AND CAST(sm.StatementDate AS date) = @StatementDate GROUP BY sm.ClientName";
                sqlParams.Add(new SqlParameter("@StatementDate", statementDate));
            }

            var dt = connectionFactory.FillDataTable(sql, sqlParams);
            return dt.Rows.Count > 0 ? Convert.ToInt32(dt.Rows[0][0]) : 0;
        }

        private int Display_Delivered_Counter_Total(string clientName, string statementDate)
        {
            var isPostgres = connectionFactory.IsPostgreSql;
            var sqlParams = new List<SqlParameter>();

            string sql;
            if (isPostgres)
            {
                sql = @"SELECT COUNT(*) FROM dbo.""tblScheduledMail"" sm
                         JOIN dbo.""tbBCATData"" bct ON bct.""ARClientCode"" = sm.""ClientCode"" AND bct.""BillNo"" = sm.""BillNo"" AND bct.""StatementDate"" = CAST(sm.""StatementDate"" AS date)
                         WHERE sm.""SentDate"" IS NOT NULL AND sm.""ReferenceNo"" IS NOT NULL";
                if (clientName != "" && clientName != null)
                {
                    sql += @" AND TRIM(sm.""ClientName"") ILIKE @ClientName";
                    sqlParams.Add(new SqlParameter("@ClientName", "%" + clientName + "%"));
                }
                sqlParams.Add(new SqlParameter("@StatementDate", statementDate));
                sql += @" AND CAST(sm.""StatementDate"" AS date) = CAST(@StatementDate AS date)";
            }
            else
            {
                sql = @"SELECT COUNT(*) FROM dbo.[tblScheduledMail] sm
                         JOIN dbo.[tbBCATData] bct ON bct.ARClientCode = sm.ClientCode AND bct.BillNo = sm.BillNo AND bct.StatementDate = CAST(sm.StatementDate AS date)
                         WHERE sm.[SentDate] IS NOT NULL AND sm.[ReferenceNo] IS NOT NULL";
                if (clientName != "" && clientName != null)
                {
                    sql += @" AND LTRIM(RTRIM(sm.ClientName)) LIKE @ClientName";
                    sqlParams.Add(new SqlParameter("@ClientName", "%" + clientName + "%"));
                }
                sqlParams.Add(new SqlParameter("@StatementDate", statementDate));
                sql += @" AND CAST(sm.StatementDate AS date) = @StatementDate";
            }

            var dt = connectionFactory.FillDataTable(sql, sqlParams);
            return dt.Rows.Count > 0 ? Convert.ToInt32(dt.Rows[0][0]) : 0;
        }

        private int Display_Undelivered_Counter_No_Search(string statementDate)
        {
            var isPostgres = connectionFactory.IsPostgreSql;
            var sqlParams = new List<SqlParameter>();

            string sql;
            if (isPostgres)
            {
                sql = @"SELECT COUNT(*) FROM dbo.""tblScheduledMail"" sm
                         JOIN dbo.""tbBCATData"" bct ON bct.""ARClientCode"" = sm.""ClientCode"" AND bct.""BillNo"" = sm.""BillNo"" AND bct.""StatementDate"" = CAST(sm.""StatementDate"" AS date)
                         WHERE sm.""SentDate"" IS NULL AND sm.""ClientCode"" NOT IN (SELECT ""ClientCode"" FROM dbo.""Exceptions"" WHERE ""ClientCode"" = sm.""ClientCode"" AND ""Reference_No"" = sm.""ReferenceNo"")";
                sqlParams.Add(new SqlParameter("@StatementDate", statementDate));
                sql += @" AND CAST(sm.""StatementDate"" AS date) = CAST(@StatementDate AS date) GROUP BY sm.""StatementDate""";
            }
            else
            {
                sql = @"SELECT COUNT(*) FROM dbo.[tblScheduledMail] sm
                         JOIN dbo.[tbBCATData] bct ON bct.ARClientCode = sm.ClientCode AND bct.BillNo = sm.BillNo AND bct.StatementDate = CAST(sm.StatementDate AS date)
                         WHERE sm.[SentDate] IS NULL AND sm.ClientCode NOT IN (SELECT [ClientCode] FROM dbo.[Exceptions] WHERE [ClientCode] = sm.[ClientCode] AND [Reference_No] = sm.ReferenceNo)";
                sqlParams.Add(new SqlParameter("@StatementDate", statementDate));
                sql += @" AND CAST(sm.StatementDate AS date) = @StatementDate GROUP BY sm.StatementDate";
            }

            var dt = connectionFactory.FillDataTable(sql, sqlParams);
            return dt.Rows.Count > 0 ? Convert.ToInt32(dt.Rows[0][0]) : 0;
        }

        private int Display_Undelivered_Counter_No_Search_Total(string statementDate)
        {
            var isPostgres = connectionFactory.IsPostgreSql;
            var sqlParams = new List<SqlParameter>();

            string sql;
            if (isPostgres)
            {
                sql = @"SELECT COUNT(*) FROM dbo.""tblScheduledMail"" sm
                         JOIN dbo.""tbBCATData"" bct ON bct.""ARClientCode"" = sm.""ClientCode"" AND bct.""BillNo"" = sm.""BillNo"" AND bct.""StatementDate"" = CAST(sm.""StatementDate"" AS date)
                         WHERE sm.""SentDate"" IS NULL AND sm.""ClientCode"" NOT IN (SELECT ""ClientCode"" FROM dbo.""Exceptions"" WHERE ""ClientCode"" = sm.""ClientCode"" AND ""Reference_No"" = sm.""ReferenceNo"")";
                sqlParams.Add(new SqlParameter("@StatementDate", statementDate));
                sql += @" AND CAST(sm.""StatementDate"" AS date) = CAST(@StatementDate AS date)";
            }
            else
            {
                sql = @"SELECT COUNT(*) FROM dbo.[tblScheduledMail] sm
                         JOIN dbo.[tbBCATData] bct ON bct.ARClientCode = sm.ClientCode AND bct.BillNo = sm.BillNo AND bct.StatementDate = CAST(sm.StatementDate AS date)
                         WHERE sm.[SentDate] IS NULL AND sm.ClientCode NOT IN (SELECT [ClientCode] FROM dbo.[Exceptions] WHERE [ClientCode] = sm.[ClientCode] AND [Reference_No] = sm.ReferenceNo)";
                sqlParams.Add(new SqlParameter("@StatementDate", statementDate));
                sql += @" AND CAST(sm.StatementDate AS date) = @StatementDate";
            }

            var dt = connectionFactory.FillDataTable(sql, sqlParams);
            return dt.Rows.Count > 0 ? Convert.ToInt32(dt.Rows[0][0]) : 0;
        }

        private int Display_Percentage_Totals()
        {
            var isPostgres = connectionFactory.IsPostgreSql;
            var sqlParams = new List<SqlParameter>();

            string sql;
            if (isPostgres)
            {
                sql = @"SELECT COUNT(*) FROM dbo.""vwScheduledSendDates""";
            }
            else
            {
                sql = @"SELECT COUNT(*) FROM vwScheduledSendDates";
            }

            var dt = connectionFactory.FillDataTable(sql, sqlParams);
            return dt.Rows.Count > 0 ? Convert.ToInt32(dt.Rows[0][0]) : 0;
        }

        private int Display_Clients_Total(string client_name_variable)
        {
            var isPostgres = connectionFactory.IsPostgreSql;
            var sqlParams = new List<SqlParameter>();

            string sql;
            if (isPostgres)
            {
                sql = @"SELECT COUNT(*) FROM dbo.""vwClients"" X
                        JOIN dbo.""tbBCATData"" Y ON X.""ClientCode"" = Y.""ARClientCode""
                        LEFT JOIN dbo.""Exceptions"" EX ON (Y.""ARClientCode"" = EX.""ClientCode"" AND Y.""ReferenceNo"" = EX.""Reference_No"")
                        JOIN dbo.""tblBill"" BILL ON Y.""BillNo"" = BILL.""chBillNo""
                        JOIN dbo.""tblGroupCode"" GC ON (RTRIM(LTRIM(BILL.""chBillOfficeCode"")) + RTRIM(LTRIM(BILL.""chBillGroupCode""))) = CONCAT(GC.""chOfficeCode"", GC.""chGroupCode"")
                        WHERE EX.""Reference_No"" IS NULL AND Y.""ReferenceNo"" IS NOT NULL";
                if (!string.IsNullOrWhiteSpace(client_name_variable))
                {
                    sql += @" AND TRIM(X.""ClientName"") ILIKE @ClientName";
                    sqlParams.Add(new SqlParameter("@ClientName", "%" + client_name_variable + "%"));
                }
            }
            else
            {
                sql = "SELECT COUNT(X.[ClientCode]) FROM " + connectionFactory.BcatTable("vwClients") + " X\n"
                        + "JOIN " + connectionFactory.BcatTable("tbBCATData") + " Y ON X.[ClientCode] = Y.[ARClientCode]\n"
                        + "LEFT JOIN " + connectionFactory.BcatTable("Exceptions") + " EX ON (Y.[ARClientCode] = EX.[ClientCode] AND Y.[ReferenceNo] = EX.[Reference_No])\n"
                        + "JOIN " + connectionFactory.FinAppsTable("tblBill") + " BILL ON Y.[BillNo] = BILL.[chBillNo]\n"
                        + "JOIN " + connectionFactory.DatabankTable("tblGroupCode") + " GC ON (RTRIM(LTRIM(BILL.[chBillOfficeCode])) + RTRIM(LTRIM(BILL.[chBillGroupCode]))) = CONCAT(GC.[chOfficeCode], GC.[chGroupCode])\n"
                        + "WHERE EX.[Reference_No] IS NULL AND Y.[ReferenceNo] IS NOT NULL";
                if (!string.IsNullOrWhiteSpace(client_name_variable))
                {
                    sql += @" AND RTRIM(LTRIM(X.[ClientName])) LIKE @ClientName";
                    sqlParams.Add(new SqlParameter("@ClientName", "%" + client_name_variable + "%"));
                }
            }

            var dt = connectionFactory.FillDataTable(sql, sqlParams);
            return dt.Rows.Count > 0 ? Convert.ToInt32(dt.Rows[0][0]) : 0;
        }

    }
}
