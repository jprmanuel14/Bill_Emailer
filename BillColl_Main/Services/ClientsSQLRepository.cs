using BillColl_Main.Class;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

namespace BillColl_Main.Services
{
    public class ClientsSQLRepository : IClients
    {
        private readonly IConfiguration configuration;
        private readonly DbConnectionFactory connectionFactory;

        public ClientsSQLRepository(IConfiguration configuration, DbConnectionFactory connectionFactory = null)
        {
            this.configuration = configuration;
            this.connectionFactory = connectionFactory ?? new DbConnectionFactory(configuration);
        }

        public Clients GetClient(string clientcode)
        {
            var isPostgres = connectionFactory.IsPostgreSql;
            var sql = isPostgres
                ? @"SELECT ""ClientCode"", ""ClientName"" FROM dbo.""vwClients"" WHERE ""ClientCode"" = @clientcode"
                : @"SELECT [ClientCode], [ClientName] FROM [vwClients] WHERE [ClientCode] = @clientcode";

            var dt = connectionFactory.FillDataTable(sql, new { clientcode });

            Clients _Clients = new Clients();
            if (dt.Rows.Count != 0)
            {
                _Clients.ClientId = dt.Rows[0]["ClientCode"].ToString();
                _Clients.ClientName = dt.Rows[0]["ClientName"].ToString();
            }

            return _Clients;
        }

        public IEnumerable<Clients> GetClients(string clientname = "", string clientContact = "", string clientEmail = "", string partner = "", string clientContactSort = "", string clientEmailSort = "", string partnerSort = "", string sortH = "")
        {
            var isPostgres = connectionFactory.IsPostgreSql;
            var sb = new StringBuilder();

            if (isPostgres)
            {
                sb.Append(@"SELECT ""ClientCode"", ""ClientName"" FROM dbo.""vwClients""");
                if (!string.IsNullOrWhiteSpace(clientname))
                {
                    sb.Append(@" WHERE ""ClientName"" ILIKE @clientname");
                }
                sb.Append(@" ORDER BY ""ClientName"" LIMIT 300");
            }
            else
            {
                sb.Append(@"SELECT TOP 300 [ClientCode], [ClientName] FROM [vwClients]");
                if (!string.IsNullOrWhiteSpace(clientname))
                {
                    sb.Append(@" WHERE [ClientName] LIKE @clientname");
                }
                sb.Append(@" ORDER BY [ClientName]");
            }

            var dt = connectionFactory.FillDataTable(sb.ToString(), new { clientname = clientname + "%" });

            return (from DataRow dr in dt.Rows
                    select new Clients()
                    {
                        ClientId = dr["ClientCode"].ToString(),
                        ClientName = dr["ClientName"].ToString(),
                    }).ToList();
        }

        public IEnumerable<Clients> GetClientsAll(string clientname = "", string clientContact = "", string clientEmail = "", string partner = "", string clientContactSort = "", string clientEmailSort = "", string partnerSort = "", string sortH = "")
        {
            var isPostgres = connectionFactory.IsPostgreSql;
            var sb = new StringBuilder();

            if (isPostgres)
            {
                sb.Append(@"SELECT ""ClientCode"", ""ClientName"" FROM dbo.""vwClients""");
                if (!string.IsNullOrWhiteSpace(clientname))
                {
                    sb.Append(@" WHERE ""ClientName"" ILIKE @clientname");
                }
                sb.Append(@" ORDER BY ""ClientName""");
            }
            else
            {
                sb.Append(@"SELECT [ClientCode], [ClientName] FROM [vwClients]");
                if (!string.IsNullOrWhiteSpace(clientname))
                {
                    sb.Append(@" WHERE [ClientName] LIKE @clientname");
                }
                sb.Append(@" ORDER BY [ClientName]");
            }

            var dt = connectionFactory.FillDataTable(sb.ToString(), new { clientname = clientname + "%" });

            return (from DataRow dr in dt.Rows
                    select new Clients()
                    {
                        ClientId = dr["ClientCode"].ToString(),
                        ClientName = dr["ClientName"].ToString(),
                    }).ToList();
        }
    }
}
