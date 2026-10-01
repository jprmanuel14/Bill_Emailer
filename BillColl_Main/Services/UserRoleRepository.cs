using BillColl_Main.Models;
using Dapper;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace BillColl_Main.Services
{
    public class UserRoleRepository : IUserRole
    {
        private readonly DbConnectionFactory connectionFactory;

        public UserRoleRepository(IConfiguration configuration, DbConnectionFactory connectionFactory = null)
        {
            this.connectionFactory = connectionFactory ?? new DbConnectionFactory(configuration);
        }

        public IEnumerable<Nre_Uer_New> GetUserRoles()
        {
            var isPostgres = connectionFactory.IsPostgreSql;
            var table = connectionFactory.BcatTable("User_Role");

            var sql = isPostgres
                ? $@"SELECT ""Id"", ""E_Code"", ""EmployeeName"", ""User_Role"", ""Date_Assigned"" FROM {table} ORDER BY ""Id"" DESC"
                : $@"SELECT [Id], [E_Code], [EmployeeName], [User_Role], [Date_Assigned] FROM {table} ORDER BY [Id] DESC";

            return connectionFactory.FillDataTable(sql)
                .AsEnumerable()
                .Select(MapRole)
                .ToList();
        }

        public Nre_Uer_New GetUserRole(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                return null;
            }

            var isPostgres = connectionFactory.IsPostgreSql;
            var table = connectionFactory.BcatTable("User_Role");

            var sql = isPostgres
                ? $@"SELECT ""Id"", ""E_Code"", ""EmployeeName"", ""User_Role"", ""Date_Assigned"" FROM {table} WHERE ""Id"" = @Id"
                : $@"SELECT [Id], [E_Code], [EmployeeName], [User_Role], [Date_Assigned] FROM {table} WHERE [Id] = @Id";

            var dt = connectionFactory.FillDataTable(sql, new { Id = DbValues.CoerceId(id) });

            return dt.Rows.Count == 0 ? null : MapRole(dt.Rows[0]);
        }

        public void AddUserRole(Nre_Uer_New userRole, string actionTaker, string actionTaken)
        {
            var isPostgres = connectionFactory.IsPostgreSql;
            var roleTable = connectionFactory.BcatTable("User_Role");
            var logTable = connectionFactory.BcatTable("Log_Admin");
            var now = isPostgres ? "CURRENT_DATE, CURRENT_TIMESTAMP" : "GETDATE(), GETDATE()";

            var insertRole = isPostgres
                ? $@"INSERT INTO {roleTable} (""E_Code"", ""EmployeeName"", ""User_Role"", ""Date_Assigned"")
                    VALUES (@E_Code, @EmployeeName, @User_Role, CURRENT_TIMESTAMP)"
                : $@"INSERT INTO {roleTable} ([E_Code], [EmployeeName], [User_Role], [Date_Assigned])
                    VALUES (@E_Code, @EmployeeName, @User_Role, GETDATE())";

            var insertLog = isPostgres
                ? $@"INSERT INTO {logTable} (""Log_Date"", ""Log_Time"", ""Action_Taker"", ""Action_Taken"")
                    VALUES ({now}, @Action_Taker, @Action_Taken)"
                : $@"INSERT INTO {logTable} ([Log_Date], [Log_Time], [Action_Taker], [Action_Taken])
                    VALUES ({now}, @Action_Taker, @Action_Taken)";

            using var conn = connectionFactory.CreateConnection();
            conn.Open();
            using var tx = conn.BeginTransaction();

            conn.Execute(insertRole, new
            {
                E_Code = userRole.Employee_Code,
                EmployeeName = userRole.Employee_Name,
                User_Role = userRole.User_Role
            }, tx);

            conn.Execute(insertLog, LogParams(actionTaker, actionTaken), tx);

            tx.Commit();
        }

        public void UpdateUserRole(Nre_Uer_New userRole, string actionTaker, string actionTaken)
        {
            var isPostgres = connectionFactory.IsPostgreSql;
            var roleTable = connectionFactory.BcatTable("User_Role");
            var logTable = connectionFactory.BcatTable("Log_Admin");
            var now = isPostgres ? "CURRENT_DATE, CURRENT_TIMESTAMP" : "GETDATE(), GETDATE()";

            var updateRole = isPostgres
                ? $@"UPDATE {roleTable} SET ""User_Role"" = @User_Role WHERE ""Id"" = @Id"
                : $@"UPDATE {roleTable} SET [User_Role] = @User_Role WHERE [Id] = @Id";

            var insertLog = isPostgres
                ? $@"INSERT INTO {logTable} (""Log_Date"", ""Log_Time"", ""Action_Taker"", ""Action_Taken"")
                    VALUES ({now}, @Action_Taker, @Action_Taken)"
                : $@"INSERT INTO {logTable} ([Log_Date], [Log_Time], [Action_Taker], [Action_Taken])
                    VALUES ({now}, @Action_Taker, @Action_Taken)";

            using var conn = connectionFactory.CreateConnection();
            conn.Open();
            using var tx = conn.BeginTransaction();

            conn.Execute(updateRole, new { User_Role = userRole.User_Role, Id = DbValues.CoerceId(userRole.Id) }, tx);
            conn.Execute(insertLog, LogParams(actionTaker, actionTaken), tx);

            tx.Commit();
        }

        public void DeleteUserRole(string id, string actionTaker, string actionTaken)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                return;
            }

            var isPostgres = connectionFactory.IsPostgreSql;
            var roleTable = connectionFactory.BcatTable("User_Role");
            var logTable = connectionFactory.BcatTable("Log_Admin");
            var now = isPostgres ? "CURRENT_DATE, CURRENT_TIMESTAMP" : "GETDATE(), GETDATE()";

            var insertLog = isPostgres
                ? $@"INSERT INTO {logTable} (""Log_Date"", ""Log_Time"", ""Action_Taker"", ""Action_Taken"")
                    VALUES ({now}, @Action_Taker, @Action_Taken)"
                : $@"INSERT INTO {logTable} ([Log_Date], [Log_Time], [Action_Taker], [Action_Taken])
                    VALUES ({now}, @Action_Taker, @Action_Taken)";

            var deleteRole = isPostgres
                ? $@"DELETE FROM {roleTable} WHERE ""Id"" = @Id"
                : $@"DELETE FROM {roleTable} WHERE [Id] = @Id";

            using var conn = connectionFactory.CreateConnection();
            conn.Open();
            using var tx = conn.BeginTransaction();

            conn.Execute(insertLog, LogParams(actionTaker, actionTaken), tx);
            conn.Execute(deleteRole, new { Id = DbValues.CoerceId(id) }, tx);

            tx.Commit();
        }

        private static object LogParams(string actionTaker, string actionTaken)
        {
            return new
            {
                Action_Taker = string.IsNullOrWhiteSpace(actionTaker) ? null : actionTaker,
                Action_Taken = string.IsNullOrWhiteSpace(actionTaken) ? null : actionTaken
            };
        }

        private static Nre_Uer_New MapRole(DataRow dr)
        {
            return new Nre_Uer_New
            {
                Id = Value(dr, "Id"),
                Employee_Code = Value(dr, "E_Code"),
                Employee_Name = Value(dr, "EmployeeName"),
                User_Role = Value(dr, "User_Role"),
                Date_Assigned = Value(dr, "Date_Assigned")
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
    }
}
