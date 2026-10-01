using BillColl_Main.Models;
using Microsoft.Extensions.Configuration;
using System;
using System.Data;

namespace BillColl_Main.Services
{
    public class SharedSqlRepository : ISharedSql
    {
        private readonly SharedSqlConnectionFactory connectionFactory;

        public SharedSqlRepository(IConfiguration configuration, SharedSqlConnectionFactory connectionFactory = null)
        {
            this.connectionFactory = connectionFactory ?? new SharedSqlConnectionFactory(configuration);
        }

        public bool IsConfigured => connectionFactory.IsConfigured;

        public Group_Ver GetLOSByGroupCode(string groupCode)
        {
            if (!IsConfigured || string.IsNullOrWhiteSpace(groupCode))
            {
                return null;
            }

            var servers = connectionFactory.LinkedServers;

            var sql = $@"SELECT IIF(EntityCode = '090','BSP', LoSName) [vcGroupTypeDesc], LOSCode [chGroupTypeCode]
                         FROM [{servers.Databank}].[PH Report Databank].[dbo].tblEntityMapping
                         WHERE GroupCode = @groupcode";

            var dt = connectionFactory.FillDataTable(sql, new { groupcode = groupCode });

            if (dt.Rows.Count == 0)
            {
                return null;
            }

            return new Group_Ver
            {
                Group_Code_WO_Desc = groupCode.Trim(),
                los = Value(dt.Rows[0], "vcGroupTypeDesc")
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
