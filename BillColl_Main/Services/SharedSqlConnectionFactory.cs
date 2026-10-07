using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System;
using System.Data;

namespace BillColl_Main.Services
{
    /// <summary>
    /// Connection to the shared SQL Server estate (PH Report Databank, FinApps, HRIS).
    /// These databases did not move to PostgreSQL with BCAT, so this connection is always
    /// SQL Server regardless of the primary provider. It stays inert until a connection
    /// string is supplied for the target environment.
    /// </summary>
    public class SharedSqlConnectionFactory
    {
        private readonly IConfiguration _configuration;

        public SharedSqlConnectionFactory(IConfiguration configuration)
        {
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        }

        public LinkedServerOptions LinkedServers => new LinkedServerOptions
        {
            Bcat = _configuration["Connections:LinkedServers:Bcat"] ?? "BCAT",
            Databank = _configuration["Connections:LinkedServers:Databank"] ?? "PH_MLAAPP002S",
            FinApps = _configuration["Connections:LinkedServers:FinApps"] ?? "PH_MLAAPP004S",
            Hris = _configuration["Connections:LinkedServers:Hris"] ?? "PH_MLAAPP002S"
        };

        public string ConnectionString
        {
            get
            {
                return _configuration["Connections:SharedSql:ConnectionString"] ?? string.Empty;
            }
        }

        public bool IsConfigured => !string.IsNullOrWhiteSpace(ConnectionString);

        public IDbConnection CreateConnection()
        {
            return new SqlConnection(ConnectionString);
        }

        public DataTable FillDataTable(string sql, object parameters = null)
        {
            using var conn = new SqlConnection(ConnectionString);
            var dt = new DataTable();

            using var cmd = new SqlCommand(sql, conn);
            AddParameters(cmd, parameters);
            using var da = new SqlDataAdapter(cmd);
            da.Fill(dt);

            return dt;
        }

        private static void AddParameters(IDbCommand cmd, object parameters)
        {
            if (parameters == null)
            {
                return;
            }

            foreach (var prop in parameters.GetType().GetProperties())
            {
                if (!prop.CanRead || prop.GetIndexParameters().Length > 0)
                {
                    continue;
                }

                var param = cmd.CreateParameter();
                param.ParameterName = prop.Name.StartsWith("@") ? prop.Name : "@" + prop.Name;
                param.Value = prop.GetValue(parameters) ?? DBNull.Value;
                cmd.Parameters.Add(param);
            }
        }
    }
}
