using BillColl_Main.Models;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;

namespace BillColl_Main.Services
{
    public class SettingsRepository : ISettingsRepository
    {
        private static readonly string[] StatementDateFormats =
        {
            "yyyy-MM-dd",
            "MM/dd/yyyy",
            "dd/MM/yyyy",
            "MMM d, yyyy",
            "MMMM d, yyyy",
            "dd MMM yyyy",
            "yyyyMMdd"
        };

        private readonly DbConnectionFactory connectionFactory;
        private readonly ISharedSql sharedSql;
        private readonly IEmployeeDirectory employeeDirectory;
        private readonly IGroup groups;

        public SettingsRepository(
            IConfiguration configuration,
            ISharedSql sharedSql,
            IEmployeeDirectory employeeDirectory,
            IGroup groups,
            DbConnectionFactory connectionFactory = null)
        {
            this.connectionFactory = connectionFactory ?? new DbConnectionFactory(configuration);
            this.sharedSql = sharedSql;
            this.employeeDirectory = employeeDirectory;
            this.groups = groups;
        }

        public IEnumerable<Nre_Uer_New> GetAdminLogs()
        {
            var results = new List<Nre_Uer_New>();

            var sql = connectionFactory.IsPostgreSql
                ? $@"SELECT ""Id"", ""Log_Date"", ""Log_Time"", ""Action_Taker"", ""Action_Taken""
                    FROM {connectionFactory.BcatTable("Log_Admin")} ORDER BY ""Log_Time"" DESC"
                : $@"SELECT [Id], [Log_Date], [Log_Time], [Action_Taker], [Action_Taken]
                    FROM {connectionFactory.BcatTable("Log_Admin")} ORDER BY [Log_Time] DESC";

            foreach (DataRow dr in connectionFactory.FillDataTable(sql).Rows)
            {
                results.Add(new Nre_Uer_New
                {
                    Id = Cell(dr, "Id"),
                    Log_Date = FormatDate(Cell(dr, "Log_Date"), "MMM d, yyyy"),
                    Log_Time = Cell(dr, "Log_Time"),
                    Log_Time_Format = FormatTime(Cell(dr, "Log_Time"), "HH:mm"),
                    Action_Taker = employeeDirectory.GetNameByEmployeeCode(Cell(dr, "Action_Taker")),
                    Action_Taken = Cell(dr, "Action_Taken")
                });
            }

            return results;
        }

        public IEnumerable<Engagement_Secretary> GetSecretaryLogs()
        {
            var results = new List<Engagement_Secretary>();

            var sql = connectionFactory.IsPostgreSql
                ? $@"SELECT ""Id"", ""Date"", ""Time"", ""Action_Taker"", ""Action_Taken""
                    FROM {connectionFactory.BcatTable("Secretary_Logs")} ORDER BY ""Id"" DESC"
                : $@"SELECT [Id], [Date], [Time], [Action_Taker], [Action_Taken]
                    FROM {connectionFactory.BcatTable("Secretary_Logs")} ORDER BY [Id] DESC";

            foreach (DataRow dr in connectionFactory.FillDataTable(sql).Rows)
            {
                results.Add(new Engagement_Secretary
                {
                    Id = Cell(dr, "Id"),
                    Log_Date = FormatDate(Cell(dr, "Date"), "dd MMM yyyy"),
                    Log_Time_Format = FormatTime(Cell(dr, "Time"), "HH:mm:ss"),
                    Log_Time = Cell(dr, "Time"),
                    Action_Taker = employeeDirectory.GetNameByEmployeeCode(Cell(dr, "Action_Taker")),
                    Action_Taken = Cell(dr, "Action_Taken")
                });
            }

            return results;
        }

        public IEnumerable<Engagement_Secretary> GetEngagementSecretaries()
        {
            var results = new List<Engagement_Secretary>();

            var sql = connectionFactory.IsPostgreSql
                ? $@"SELECT ""Id"", ""Secretary"", ""Partner"", ""Group"", ""Is_Default""
                    FROM {connectionFactory.BcatTable("Engagement_Secretary")} ORDER BY ""Id"" DESC"
                : $@"SELECT [Id], [Secretary], [Partner], [Group], [Is_Default]
                    FROM {connectionFactory.BcatTable("Engagement_Secretary")} ORDER BY [Id] DESC";

            foreach (DataRow dr in connectionFactory.FillDataTable(sql).Rows)
            {
                results.Add(MapEngagementSecretary(dr));
            }

            return results;
        }

        public Engagement_Secretary GetEngagementSecretary(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                return null;
            }

            var sql = connectionFactory.IsPostgreSql
                ? $@"SELECT ""Id"", ""Secretary"", ""Partner"", ""Group"", ""Is_Default""
                    FROM {connectionFactory.BcatTable("Engagement_Secretary")} WHERE ""Id"" = @Id"
                : $@"SELECT [Id], [Secretary], [Partner], [Group], [Is_Default]
                    FROM {connectionFactory.BcatTable("Engagement_Secretary")} WHERE [Id] = @Id";

            var dt = connectionFactory.FillDataTable(sql, new { Id = DbValues.CoerceId(id) });

            return dt.Rows.Count == 0 ? null : MapEngagementSecretary(dt.Rows[0]);
        }

        public IEnumerable<Engagement_Secretary> GetViewReports(string clientCode, string statementDate, string los)
        {
            var results = new List<Engagement_Secretary>();

            if (string.IsNullOrWhiteSpace(clientCode))
            {
                return results;
            }

            var dateFilter = TryParseStatementDate(statementDate, out var parsedDate)
                ? parsedDate
                : (DateTime?)null;

            var parameters = dateFilter.HasValue
                ? (object)new { ClientCode = clientCode.Trim(), StatementDate = dateFilter.Value }
                : new { ClientCode = clientCode.Trim() };

            var sql = connectionFactory.IsPostgreSql
                ? BuildViewReportsSql(true, dateFilter.HasValue)
                : BuildViewReportsSql(false, dateFilter.HasValue);

            foreach (DataRow dr in connectionFactory.FillDataTable(sql, parameters).Rows)
            {
                var groupCode = (Cell(dr, "chBillOfficeCode") + Cell(dr, "chBillGroupCode")).Trim();
                var resolvedLos = ResolveLos(groupCode);

                if (!string.IsNullOrWhiteSpace(los) &&
                    !string.Equals(resolvedLos, los.Trim(), StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                results.Add(new Engagement_Secretary
                {
                    chDebtorCode = Cell(dr, "ARClientCode"),
                    Debtor_Name = Cell(dr, "vcDebtorName"),
                    Debtor_Address = "",
                    Reference_No = Cell(dr, "ReferenceNo"),
                    Fee_Note_No = Cell(dr, "BillNo"),
                    Bill_No = Cell(dr, "BillNo"),
                    chBillEntityCode = Cell(dr, "chBillEntityCode"),
                    chBillType = Cell(dr, "AgeBracket"),
                    Entity = Cell(dr, "vcPracticeName"),
                    Practice_Name = Cell(dr, "vcPracticeName"),
                    Email = Cell(dr, "Email"),
                    StatementDate = Cell(dr, "StatementDate"),
                    Trans_Date = Cell(dr, "TransDate"),
                    Run_Date = Cell(dr, "RunDate"),
                    Remarks = Cell(dr, "Remarks"),
                    Other_Currency_Code = Cell(dr, "Currency"),
                    Age_Bracket = Cell(dr, "AgeBracket"),
                    Age = Cell(dr, "Age"),
                    AccountCode = Cell(dr, "ARClientCode"),
                    vcAccountNumber = Cell(dr, "ARClientCode"),
                    los = resolvedLos,
                    Tot_Amount_Payable = Cell(dr, "Peso"),
                    Dollar = ParseDecimal(Cell(dr, "Dollar")),
                    Peso = ParseDecimal(Cell(dr, "Peso"))
                });
            }

            return results;
        }

        public DataSet GetAdminLogsExport()
        {
            return LogExport("Log_Admin", "Log_Date", "Log_Time", "Log_Time");
        }

        public DataSet GetSecretaryLogsExport()
        {
            return LogExport("Secretary_Logs", "Date", "Time", "Time");
        }

        public IReadOnlyCollection<string> GetUserRoleEmployeeCodes()
        {
            var codes = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            var sql = connectionFactory.IsPostgreSql
                ? $@"SELECT ""E_Code"" FROM {connectionFactory.BcatTable("User_Role")}"
                : $@"SELECT [E_Code] FROM {connectionFactory.BcatTable("User_Role")}";

            foreach (DataRow dr in connectionFactory.FillDataTable(sql).Rows)
            {
                var code = Cell(dr, "E_Code");

                if (!string.IsNullOrWhiteSpace(code))
                {
                    codes.Add(code.Trim());
                }
            }

            return codes;
        }

        private DataSet LogExport(string tableName, string dateColumn, string timeColumn, string orderByColumn)
        {
            var dt = connectionFactory.FillDataTable(connectionFactory.IsPostgreSql
                ? $@"SELECT TO_CHAR(""{dateColumn}"", 'FMMon FMDD, YYYY') ""Date"",
                           TO_CHAR(""{timeColumn}"", 'HH24:MI') ""Time"",
                           ""Action_Taker"" ""Action Taker"",
                           ""Action_Taken"" ""Action Taken""
                    FROM {connectionFactory.BcatTable(tableName)} ORDER BY ""{orderByColumn}"" DESC"
                : $@"SELECT FORMAT([{dateColumn}], 'MMM d, yyyy') [Date],
                           FORMAT([{timeColumn}], 'HH:mm') [Time],
                           [Action_Taker] [Action Taker],
                           [Action_Taken] [Action Taken]
                    FROM {connectionFactory.BcatTable(tableName)} ORDER BY [{orderByColumn}] DESC");

            var ds = new DataSet();
            ds.Tables.Add(dt);

            return ds;
        }

        private string BuildViewReportsSql(bool isPostgreSql, bool filterByDate)
        {
            var bcatData = connectionFactory.BcatTable("tbBCATData");
            var bill = connectionFactory.BcatTable("tblBill");
            var contacts = connectionFactory.BcatTable("Contacts");

            var dateFilter = filterByDate
                ? (isPostgreSql ? "AND BCT.\"StatementDate\" = @StatementDate" : "AND BCT.[StatementDate] = @StatementDate")
                : "";

            if (isPostgreSql)
            {
                return $@"SELECT BCT.""ARClientCode"", BCT.""StatementDate"", BCT.""ReferenceNo"", BCT.""BillNo"",
                                 BCT.""ARClientName"" ""vcDebtorName"", BCT.""ARClientName"", BCT.""AgeBracket"",
                                 BCT.""TransDate"", BCT.""RunDate"", BCT.""vcPracticeName"", BCT.""chBillEntityCode"",
                                 BCT.""Remarks"", BCT.""Currency"", BCT.""Age"", BCT.""Dollar"", BCT.""Peso"",
                                 BL.""chBillOfficeCode"", BL.""chBillGroupCode"", CL.""Email""
                          FROM {bcatData} BCT
                          LEFT JOIN {bill} BL ON BCT.""BillNo"" = BL.""chBillNo""
                          LEFT JOIN {contacts} CL ON BCT.""ARClientCode"" = CL.""ClientCode""
                          WHERE BCT.""ARClientCode"" = @ClientCode {dateFilter}
                          ORDER BY BCT.""StatementDate"" DESC";
            }

            return $@"SELECT BCT.[ARClientCode], BCT.[StatementDate], BCT.[ReferenceNo], BCT.[BillNo],
                             BCT.[ARClientName] [vcDebtorName], BCT.[ARClientName], BCT.[AgeBracket],
                             BCT.[TransDate], BCT.[RunDate], BCT.[vcPracticeName], BCT.[chBillEntityCode],
                             BCT.[Remarks], BCT.[Currency], BCT.[Age], BCT.[Dollar], BCT.[Peso],
                             BL.[chBillOfficeCode], BL.[chBillGroupCode], CL.[Email]
                      FROM {bcatData} BCT
                      LEFT JOIN {bill} BL ON BCT.[BillNo] = BL.[chBillNo]
                      LEFT JOIN {contacts} CL ON BCT.[ARClientCode] = CL.[ClientCode]
                      WHERE BCT.[ARClientCode] = @ClientCode {dateFilter}
                      ORDER BY BCT.[StatementDate] DESC";
        }

        private string ResolveLos(string groupCode)
        {
            if (string.IsNullOrWhiteSpace(groupCode))
            {
                return "";
            }

            var mapping = sharedSql.GetLOSByGroupCode(groupCode);

            if (mapping != null && !string.IsNullOrWhiteSpace(mapping.los))
            {
                return mapping.los;
            }

            return FindGroupDescription(groupCode);
        }

        private string FindGroupDescription(string groupCode)
        {
            var group = groups?.GetGroupCodeList()?
                .FirstOrDefault(g => string.Equals(g.Group_Code_WO_Desc?.Trim(), groupCode, StringComparison.OrdinalIgnoreCase));

            return group?.Description ?? "";
        }

        private Engagement_Secretary MapEngagementSecretary(DataRow dr)
        {
            var groupCode = Cell(dr, "Group").Trim();

            return new Engagement_Secretary
            {
                Id = Cell(dr, "Id"),
                Secretary_Code = Cell(dr, "Secretary"),
                Secretary = employeeDirectory.GetNameByEmployeeCode(Cell(dr, "Secretary")),
                Partner_Code = Cell(dr, "Partner"),
                Partner = employeeDirectory.GetNameByEmployeeCode(Cell(dr, "Partner")),
                Group_Id = groupCode,
                Group = FindGroupDescription(groupCode),
                is_default = ParseBoolean(Cell(dr, "Is_Default"))
            };
        }

        private static bool ParseBoolean(string value)
        {
            return bool.TryParse(value, out var result) && result;
        }

        private static decimal ParseDecimal(string value)
        {
            return decimal.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out var result) ? result : 0m;
        }

        private static bool TryParseStatementDate(string value, out DateTime date)
        {
            date = default;

            if (string.IsNullOrWhiteSpace(value))
            {
                return false;
            }

            return DateTime.TryParseExact(value.Trim(), StatementDateFormats, CultureInfo.InvariantCulture, DateTimeStyles.None, out date)
                || DateTime.TryParse(value.Trim(), CultureInfo.InvariantCulture, DateTimeStyles.None, out date);
        }

        private static string FormatDate(string value, string format)
        {
            return DateTime.TryParse(value, CultureInfo.InvariantCulture, DateTimeStyles.None, out var parsed)
                ? parsed.ToString(format, CultureInfo.InvariantCulture)
                : value;
        }

        private static string FormatTime(string value, string format)
        {
            return DateTime.TryParse(value, CultureInfo.InvariantCulture, DateTimeStyles.None, out var parsed)
                ? parsed.ToString(format, CultureInfo.InvariantCulture)
                : value;
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