using System;
using System.Collections.Generic;
using System.Data;
using System.Threading;
using System.Threading.Tasks;
using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Options;
using Npgsql;
using BillingMail.Plugin.Models;
using BillingMail.Plugin.Options;

namespace BillingMail.Plugin.Services;

/// <summary>
/// Dapper-based repository implementation supporting both PostgreSQL and MS SQL Server.
/// Completely parameterized to eliminate SQL injection risks.
/// </summary>
public class BillingRepository : IBillingRepository
{
    private readonly BillingMailOptions _options;

    public BillingRepository(IOptions<BillingMailOptions> options)
    {
        _options = options.Value ?? throw new ArgumentNullException(nameof(options));
    }

    private IDbConnection CreateConnection()
    {
        if (IsPostgreSql)
        {
            return new NpgsqlConnection(_options.ConnectionString);
        }
        return new SqlConnection(_options.ConnectionString);
    }

    private bool IsPostgreSql =>
        string.Equals(_options.DatabaseProvider, "PostgreSQL", StringComparison.OrdinalIgnoreCase) ||
        string.Equals(_options.DatabaseProvider, "Postgres", StringComparison.OrdinalIgnoreCase);

    public async Task<IEnumerable<ScheduledMailItem>> GetPendingScheduledMailsAsync(CancellationToken cancellationToken = default)
    {
        var sql = IsPostgreSql ? @"
            SELECT * FROM (
                SELECT 
                    a.""Id"",
                    a.""ClientCode"",
                    a.""ClientName"",
                    a.""StatementDate"",
                    a.""ScheduledDateTime"",
                    a.""SentDate"",
                    (SELECT b.""chBillEntityCode""
                     FROM dbo.""tbBCATData"" b
                     WHERE b.""ARClientCode"" = a.""ClientCode"" AND b.""StatementDate"" = a.""StatementDate""
                     LIMIT 1) AS EntityCode,
                    a.""Recepients"" AS ContactMails,
                    a.""CC"" AS CCMails
                FROM dbo.""tblScheduledMail"" a
                WHERE a.""ScheduledDateTime"" <= CURRENT_TIMESTAMP 
                  AND a.""SentDate"" IS NULL 
                  AND (a.""Status"" IS NULL OR a.""Status"" = '') 
                  AND a.""ClientCode"" NOT IN (SELECT ""ClientCode"" FROM dbo.""Exceptions"")
            ) v 
            WHERE ContactMails IS NOT NULL AND ContactMails <> ''"
        : @"
            SELECT * FROM (
                SELECT 
                    a.[Id],
                    a.[ClientCode],
                    a.[ClientName],
                    a.[StatementDate],
                    a.[ScheduledDateTime],
                    a.[SentDate],
                    (SELECT TOP 1 chBillEntityCode 
                     FROM tbBCATData 
                     WHERE ARClientCode = a.ClientCode AND StatementDate = a.StatementDate) AS EntityCode,
                    a.Recepients AS ContactMails,
                    a.CC AS CCMails
                FROM [tblScheduledMail] a
                WHERE a.ScheduledDateTime <= GETDATE() 
                  AND a.SentDate IS NULL 
                  AND (a.[Status] IS NULL OR a.[Status] = '') 
                  AND a.ClientCode NOT IN (SELECT ClientCode FROM Exceptions)
            ) v 
            WHERE ContactMails IS NOT NULL AND ContactMails <> ''";

        using var connection = CreateConnection();
        var command = new CommandDefinition(sql, cancellationToken: cancellationToken);
        return await connection.QueryAsync<ScheduledMailItem>(command);
    }

    public async Task<IEnumerable<PracticeEntity>> GetPracticeEntitiesAsync(string clientId, DateTime statementDate, CancellationToken cancellationToken = default)
    {
        var sql = IsPostgreSql ? @"
            SELECT DISTINCT 
                b.""EntityCode"",
                a.""vcPracticeName"",
                c.""AccountName"",
                b.""ReportHierarchy""
            FROM dbo.""tbBCATData"" a
            INNER JOIN dbo.""PHEntity"" b ON a.""vcPracticeName"" = b.""EntityName""
            INNER JOIN dbo.""Banks"" c ON b.""EntityCode"" = c.""AccountCode""
            WHERE a.""ARClientCode"" = @ClientId
              AND a.""StatementDate"" = @StatementDate
            ORDER BY b.""ReportHierarchy"""
        : @"
            SELECT DISTINCT 
                b.EntityCode,
                a.vcPracticeName,
                c.AccountName,
                b.ReportHierarchy
            FROM [tbBCATData] a
            INNER JOIN [PHEntity] b ON a.vcPracticeName = b.EntityName
            INNER JOIN [Banks] c ON b.EntityCode = c.AccountCode
            WHERE a.ARClientCode = @ClientId
              AND a.StatementDate = @StatementDate
            ORDER BY b.ReportHierarchy";

        using var connection = CreateConnection();
        var command = new CommandDefinition(sql, new { ClientId = clientId, StatementDate = statementDate }, cancellationToken: cancellationToken);
        return await connection.QueryAsync<PracticeEntity>(command);
    }

    public async Task<IEnumerable<StatementLineItem>> GetStatementLineItemsAsync(string clientId, DateTime statementDate, string practiceName, CancellationToken cancellationToken = default)
    {
        var sql = IsPostgreSql ? @"
            SELECT 
                ""TransDate"",
                ""BillNo"",
                ""ReferenceNo"",
                ""Age"",
                ""Currency"",
                ""Dollar"",
                ""Peso""
            FROM dbo.""tbBCATData""
            WHERE ""StatementDate"" = @StatementDate
              AND ""ARClientCode"" = @ClientId
              AND ""vcPracticeName"" = @PracticeName
            ORDER BY ""TransDate"""
        : @"
            SELECT 
                TransDate,
                BillNo,
                ReferenceNo,
                Age,
                Currency,
                Dollar,
                Peso
            FROM [tbBCATData]
            WHERE StatementDate = @StatementDate
              AND ARClientCode = @ClientId
              AND vcPracticeName = @PracticeName
            ORDER BY TransDate";

        using var connection = CreateConnection();
        var command = new CommandDefinition(sql, new { ClientId = clientId, StatementDate = statementDate, PracticeName = practiceName }, cancellationToken: cancellationToken);
        return await connection.QueryAsync<StatementLineItem>(command);
    }

    public async Task<IEnumerable<BankDetail>> GetBankDetailsAsync(string accountCode, CancellationToken cancellationToken = default)
    {
        var sql = IsPostgreSql ? @"
            SELECT 
                ""AccountCode"",
                ""AccountName"",
                ""AccountNumber"",
                ""BankName"",
                ""Branch"",
                ""SwiftCode""
            FROM dbo.""Banks""
            WHERE ""AccountCode"" = @AccountCode"
        : @"
            SELECT 
                AccountCode,
                AccountName,
                AccountNumber,
                BankName,
                Branch,
                SwiftCode
            FROM [Banks]
            WHERE AccountCode = @AccountCode";

        using var connection = CreateConnection();
        var command = new CommandDefinition(sql, new { AccountCode = accountCode }, cancellationToken: cancellationToken);
        return await connection.QueryAsync<BankDetail>(command);
    }

    public async Task MarkMailAsSentAsync(string clientId, DateTime statementDate, CancellationToken cancellationToken = default)
    {
        var sql = IsPostgreSql ? @"
            UPDATE dbo.""tblScheduledMail"" 
            SET ""SentDate"" = CURRENT_TIMESTAMP 
            WHERE ""ClientCode"" = @ClientId 
              AND ""StatementDate"" = @StatementDate"
        : @"
            UPDATE tblScheduledMail 
            SET SentDate = GETDATE() 
            WHERE ClientCode = @ClientId 
              AND StatementDate = @StatementDate";

        using var connection = CreateConnection();
        var command = new CommandDefinition(sql, new { ClientId = clientId, StatementDate = statementDate }, cancellationToken: cancellationToken);
        await connection.ExecuteAsync(command);
    }
}
