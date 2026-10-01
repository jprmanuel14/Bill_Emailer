using System;
using System.Data;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Npgsql;

namespace BillColl_Main.Services
{
    public class DbConnectionFactory
    {
        private readonly IConfiguration _configuration;

        public DbConnectionFactory(IConfiguration configuration)
        {
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        }

        public bool IsPostgreSql
        {
            get
            {
                var provider = _configuration["Connections:Primary:Provider"];

                if (string.IsNullOrWhiteSpace(provider))
                {
                    provider = _configuration["BillingMail:DatabaseProvider"];
                }

                return string.Equals(provider, "PostgreSQL", StringComparison.OrdinalIgnoreCase) ||
                       string.Equals(provider, "Postgres", StringComparison.OrdinalIgnoreCase);
            }
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
                var connStr = _configuration["Connections:Primary:ConnectionString"];

                if (string.IsNullOrWhiteSpace(connStr))
                {
                    connStr = _configuration.GetConnectionString("myAppDBConnection");
                }

                if (string.IsNullOrWhiteSpace(connStr))
                {
                    connStr = _configuration["BillingMail:ConnectionString"];
                }

                return connStr ?? string.Empty;
            }
        }

        /// <summary>
        /// BCAT is reachable two different ways. On SQL Server it is a linked server named
        /// per environment; on PostgreSQL it is the primary database and must be referenced
        /// locally, because a SQL Server linked server cannot reach a PostgreSQL instance.
        /// </summary>
        public string BcatTable(string tableName)
        {
            return IsPostgreSql
                ? "dbo." + QuotePostgres(tableName)
                : "[" + LinkedServers.Bcat + "].[dbo].[" + tableName + "]";
        }

        public string QuotePostgres(string identifier)
        {
            return "\"" + identifier.Replace("\"", "\"\"") + "\"";
        }

        /// <summary>
        /// The shared estate stays on SQL Server, so these are always emitted as
        /// three-part names. Only the server name is environment-specific; the database
        /// names are fixed.
        /// </summary>
        public string DatabankTable(string tableName)
        {
            return "[" + LinkedServers.Databank + "].[PH Report Databank].dbo.[" + tableName + "]";
        }

        public string FinAppsTable(string tableName)
        {
            return "[" + LinkedServers.FinApps + "].[FinAppsDM].dbo.[" + tableName + "]";
        }

        public string HrisTable(string tableName)
        {
            return "[" + LinkedServers.Hris + "].[HRIS].dbo.[" + tableName + "]";
        }

        public IDbConnection CreateConnection()
        {
            if (IsPostgreSql)
            {
                return new NpgsqlConnection(ConnectionString);
            }
            return new SqlConnection(ConnectionString);
        }

        public DataTable FillDataTable(string sql, object parameters = null)
        {
            using var conn = CreateConnection();
            var dt = new DataTable();

            if (conn is NpgsqlConnection npgConn)
            {
                using var cmd = new NpgsqlCommand(sql, npgConn);
                AddParameters(cmd, parameters);
                using var da = new NpgsqlDataAdapter(cmd);
                da.Fill(dt);
            }
            else if (conn is SqlConnection sqlConn)
            {
                using var cmd = new SqlCommand(sql, sqlConn);
                AddParameters(cmd, parameters);
                using var da = new SqlDataAdapter(cmd);
                da.Fill(dt);
            }

            return dt;
        }

        private void AddParameters(IDbCommand cmd, object parameters)
        {
            if (parameters == null) return;

            if (parameters is IDbDataParameter singleParam)
            {
                cmd.Parameters.Add(CloneParameter(cmd, singleParam));
                return;
            }

            if (parameters is System.Collections.IEnumerable parameterList)
            {
                foreach (var item in parameterList)
                {
                    if (item is IDbDataParameter dbParam)
                    {
                        cmd.Parameters.Add(CloneParameter(cmd, dbParam));
                    }
                }
                return;
            }

            foreach (var prop in parameters.GetType().GetProperties())
            {
                if (!prop.CanRead || prop.GetIndexParameters().Length > 0) continue;

                var param = cmd.CreateParameter();
                param.ParameterName = prop.Name.StartsWith("@") ? prop.Name : "@" + prop.Name;
                param.Value = prop.GetValue(parameters) ?? DBNull.Value;
                cmd.Parameters.Add(param);
            }
        }

        private static IDbDataParameter CloneParameter(IDbCommand cmd, IDbDataParameter source)
        {
            var param = cmd.CreateParameter();
            param.ParameterName = source.ParameterName;
            param.Value = source.Value ?? DBNull.Value;

            if (source is System.Data.SqlClient.SqlParameter legacyParam &&
                param is SqlParameter mdsParam &&
                legacyParam.SqlDbType != System.Data.SqlDbType.Variant)
            {
                mdsParam.SqlDbType = (System.Data.SqlDbType)legacyParam.SqlDbType;
                if (legacyParam.Size > 0)
                {
                    mdsParam.Size = legacyParam.Size;
                }
            }

            return param;
        }
    }
}
