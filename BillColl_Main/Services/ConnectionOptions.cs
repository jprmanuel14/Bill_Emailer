namespace BillColl_Main.Services
{
    public class ConnectionsOptions
    {
        public PrimaryConnectionOptions Primary { get; set; }
        public SharedSqlConnectionOptions SharedSql { get; set; }
        public LinkedServerOptions LinkedServers { get; set; }
    }

    public class PrimaryConnectionOptions
    {
        public string Provider { get; set; }
        public string ConnectionString { get; set; }
    }

    public class SharedSqlConnectionOptions
    {
        public string ConnectionString { get; set; }
    }

    /// <summary>
    /// Linked server names differ per environment. On STAGE the Cloud Core boxes are
    /// renamed, so these are configuration values rather than hardcoded SQL identifiers.
    /// </summary>
    public class LinkedServerOptions
    {
        public string Bcat { get; set; }
        public string Databank { get; set; }
        public string FinApps { get; set; }
        public string Hris { get; set; }
    }
}
