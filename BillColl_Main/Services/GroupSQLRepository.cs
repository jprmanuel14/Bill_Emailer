using BillColl_Main.AppDbContext;
using BillColl_Main.Models;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace BillColl_Main.Services
{
    public class GroupSQLRepository : IGroup
    {
        private readonly myDBContext context;
        private readonly DbConnectionFactory connectionFactory;

        public GroupSQLRepository(myDBContext context, IConfiguration configuration, DbConnectionFactory connectionFactory = null)
        {
            this.context = context;
            this.connectionFactory = connectionFactory ?? new DbConnectionFactory(configuration);
        }

        public Group GetGroup(string GroupName)
        {
            return context.Group.FirstOrDefault(e => e.Description == GroupName);
        }

        public Group GetGroup(int GroupId)
        {
            return context.Group.FirstOrDefault(e => e.Id == GroupId);
        }

        public IEnumerable<Group> GetGroups()
        {
            return context.Group;
        }

        public IEnumerable<Group> GetGroups(string searchGName)
        {
            return context.Group.Where(e => e.Description.ToUpper().Contains(searchGName.ToUpper()));
        }

        public IEnumerable<Group_Ver> GetGroupCodeList()
        {
            var sql = connectionFactory.IsPostgreSql
                ? @"SELECT ""Code"" AS ""Group_Code_WO_Desc"", ""Description"" AS ""vcGroupDesc""
                   FROM dbo.""Group""
                   WHERE ""Code"" IS NOT NULL AND BTRIM(""Code"") <> ''
                   ORDER BY ""Description"""
                : $@"SELECT [Code] [Group_Code_WO_Desc], [Description] [vcGroupDesc]
                   FROM {connectionFactory.BcatTable("Group")}
                   WHERE [Code] IS NOT NULL AND LTRIM(RTRIM([Code])) <> ''
                   ORDER BY [Description]";

            var dt = connectionFactory.FillDataTable(sql);

            return (from DataRow dr in dt.Rows
                    let code = dr["Group_Code_WO_Desc"] == DBNull.Value ? "" : dr["Group_Code_WO_Desc"].ToString().Trim()
                    select new Group_Ver()
                    {
                        Group_Id = code,
                        Group_Type = code,
                        Description = dr["vcGroupDesc"] == DBNull.Value ? "" : dr["vcGroupDesc"].ToString(),
                        Group_Code_WO_Desc = code
                    }).ToList();
        }
    }
}
