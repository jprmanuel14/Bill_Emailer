using BillColl_Main.Models;
using Dapper;
using Microsoft.Extensions.Configuration;
using System;
using System.Data;

namespace BillColl_Main.Services
{
    public class EngagementSecretaryRepository : IEngagementSecretary
    {
        private readonly DbConnectionFactory connectionFactory;

        public EngagementSecretaryRepository(IConfiguration configuration, DbConnectionFactory connectionFactory = null)
        {
            this.connectionFactory = connectionFactory ?? new DbConnectionFactory(configuration);
        }

        public int CountBySecretaryPartnerGroup(string secretary, string partner, string group)
        {
            var isPostgres = connectionFactory.IsPostgreSql;
            var table = connectionFactory.BcatTable("Engagement_Secretary");

            var sql = isPostgres
                ? $@"SELECT COUNT(""Is_Default"") AS ""counterz"" FROM {table}
                    WHERE ""Secretary"" = @Secretary AND ""Partner"" = @Partner AND ""Group"" = @Group"
                : $@"SELECT COUNT([Is_Default]) AS [counterz] FROM {table}
                    WHERE [Secretary] = @Secretary AND [Partner] = @Partner AND [Group] = @Group";

            var dt = connectionFactory.FillDataTable(sql, new
            {
                Secretary = secretary,
                Partner = partner,
                Group = group
            });

            return dt.Rows.Count == 0 ? 0 : Convert.ToInt32(dt.Rows[0]["counterz"]);
        }

        public int CountByPartnerGroup(string partner, string group)
        {
            var isPostgres = connectionFactory.IsPostgreSql;
            var table = connectionFactory.BcatTable("Engagement_Secretary");

            var sql = isPostgres
                ? $@"SELECT COUNT(""Is_Default"") AS ""counterz"" FROM {table}
                    WHERE ""Partner"" = @Partner AND ""Group"" = @Group"
                : $@"SELECT COUNT([Is_Default]) AS [counterz] FROM {table}
                    WHERE [Partner] = @Partner AND [Group] = @Group";

            var dt = connectionFactory.FillDataTable(sql, new { Partner = partner, Group = group });

            return dt.Rows.Count == 0 ? 0 : Convert.ToInt32(dt.Rows[0]["counterz"]);
        }

        public void AddEngagementSecretary(Engagement_Secretary secretary, string actionTaker, string actionTaken)
        {
            var isPostgres = connectionFactory.IsPostgreSql;
            var table = connectionFactory.BcatTable("Engagement_Secretary");
            var logTable = connectionFactory.BcatTable("Secretary_Logs");
            var now = isPostgres ? "CURRENT_DATE, CURRENT_TIMESTAMP" : "GETDATE(), GETDATE()";

            var insertSecretary = isPostgres
                ? $@"INSERT INTO {table} (""Secretary"", ""Partner"", ""Group"", ""Is_Default"")
                    VALUES (@Secretary, @Partner, @Group, TRUE)"
                : $@"INSERT INTO {table} ([Secretary], [Partner], [Group], [Is_Default])
                    VALUES (@Secretary, @Partner, @Group, 1)";

            var insertLog = isPostgres
                ? $@"INSERT INTO {logTable} (""Date"", ""Time"", ""Action_Taker"", ""Action_Taken"")
                    VALUES ({now}, @Action_Taker, @Action_Taken)"
                : $@"INSERT INTO {logTable} ([Date], [Time], [Action_Taker], [Action_Taken])
                    VALUES ({now}, @Action_Taker, @Action_Taken)";

            using var conn = connectionFactory.CreateConnection();
            conn.Open();
            using var tx = conn.BeginTransaction();

            conn.Execute(insertSecretary, new
            {
                Secretary = secretary.Secretary,
                Partner = secretary.Partner,
                Group = secretary.Group
            }, tx);

            conn.Execute(insertLog, LogParams(actionTaker, actionTaken), tx);

            tx.Commit();
        }

        public void UpdateEngagementSecretary(Engagement_Secretary secretary, string actionTaker, string actionTaken)
        {
            var isPostgres = connectionFactory.IsPostgreSql;
            var table = connectionFactory.BcatTable("Engagement_Secretary");
            var logTable = connectionFactory.BcatTable("Secretary_Logs");
            var now = isPostgres ? "CURRENT_DATE, CURRENT_TIMESTAMP" : "GETDATE(), GETDATE()";

            var updateSecretary = isPostgres
                ? $@"UPDATE {table} SET ""Secretary"" = @Secretary, ""Group"" = @Group WHERE ""Id"" = @Id"
                : $@"UPDATE {table} SET [Secretary] = @Secretary, [Group] = @Group WHERE [Id] = @Id";

            var insertLog = isPostgres
                ? $@"INSERT INTO {logTable} (""Date"", ""Time"", ""Action_Taker"", ""Action_Taken"")
                    VALUES ({now}, @Action_Taker, @Action_Taken)"
                : $@"INSERT INTO {logTable} ([Date], [Time], [Action_Taker], [Action_Taken])
                    VALUES ({now}, @Action_Taker, @Action_Taken)";

            using var conn = connectionFactory.CreateConnection();
            conn.Open();
            using var tx = conn.BeginTransaction();

            conn.Execute(updateSecretary, new
            {
                Secretary = secretary.Secretary,
                Group = secretary.Group,
                Id = DbValues.CoerceId(secretary.Id)
            }, tx);

            conn.Execute(insertLog, LogParams(actionTaker, actionTaken), tx);

            tx.Commit();
        }

        public void DeleteEngagementSecretary(string id, string actionTaker, string actionTaken)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                return;
            }

            var isPostgres = connectionFactory.IsPostgreSql;
            var table = connectionFactory.BcatTable("Engagement_Secretary");
            var logTable = connectionFactory.BcatTable("Secretary_Logs");
            var now = isPostgres ? "CURRENT_DATE, CURRENT_TIMESTAMP" : "GETDATE(), GETDATE()";

            var insertLog = isPostgres
                ? $@"INSERT INTO {logTable} (""Date"", ""Time"", ""Action_Taker"", ""Action_Taken"")
                    VALUES ({now}, @Action_Taker, @Action_Taken)"
                : $@"INSERT INTO {logTable} ([Date], [Time], [Action_Taker], [Action_Taken])
                    VALUES ({now}, @Action_Taker, @Action_Taken)";

            var deleteSecretary = isPostgres
                ? $@"DELETE FROM {table} WHERE ""Id"" = @Id"
                : $@"DELETE FROM {table} WHERE [Id] = @Id";

            using var conn = connectionFactory.CreateConnection();
            conn.Open();
            using var tx = conn.BeginTransaction();

            conn.Execute(insertLog, LogParams(actionTaker, actionTaken), tx);
            conn.Execute(deleteSecretary, new { Id = DbValues.CoerceId(id) }, tx);

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
    }
}
