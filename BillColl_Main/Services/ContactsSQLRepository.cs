using BillColl_Main.AppDbContext;
using BillColl_Main.Models;
using Microsoft.Extensions.Configuration;
using myhelperclass;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BillColl_Main.Services
{
    public class ContactsSQLRepository : IContacts
    {
        private readonly myDBContext context;
        private readonly IConfiguration configuration;
        private readonly DbConnectionFactory connectionFactory;

        public ContactsSQLRepository(myDBContext context, IConfiguration configuration, DbConnectionFactory connectionFactory = null)
        {
            this.context = context;
            this.configuration = configuration;
            this.connectionFactory = connectionFactory ?? new DbConnectionFactory(configuration);
        }
        public Contacts AddContact(Contacts contacts)
        {
            context.Contacts.Add(contacts);
            context.SaveChanges();
            return contacts;
        }

        public EngagementTeam AddEngagementTeam(EngagementTeam engagementTeam)
        {
            context.EngagementTeams.Add(engagementTeam);
            context.SaveChanges();
            return engagementTeam;
        }

        public Contacts DeleteContact(int Id)
        {
            var myContact = context.Contacts.FirstOrDefault(e => e.Id == Id);
            context.Contacts.Remove(myContact);
            context.SaveChanges();
            return myContact;
        }

        public EngagementTeam DeleteEngagementTeam(int Id)
        {
            var myEngagement = context.EngagementTeams.FirstOrDefault(e => e.Id == Id);
            context.EngagementTeams.Remove(myEngagement);
            context.SaveChanges();
            return myEngagement;
        }

        public Contacts GetContact(int Id)
        {
            return context.Contacts.FirstOrDefault(e => e.Id == Id);
        }

        public string GetContactFromScheduleMail(string ClientCode, DateTime StatementDate)
        {
            var isPostgres = connectionFactory.IsPostgreSql;
            var sql = isPostgres
                ? @"SELECT ""Recepients"" FROM dbo.""tblScheduledMail"" WHERE ""ClientCode"" = @ClientId AND ""StatementDate"" = @StatementDate"
                : @"SELECT [Recepients] FROM [tblScheduledMail] WHERE [ClientCode] = @ClientId AND [StatementDate] = @StatementDate";

            var dt = connectionFactory.FillDataTable(sql, new { ClientId = ClientCode, StatementDate });

            if (dt.Rows.Count == 0)
            {
                return "";
            }
            return dt.Rows[0]["Recepients"].ToString();
        }

        public IEnumerable<Contacts> GetContacts(string ClientCode)
        {
            return context.Contacts.Where(e => e.ClientCode == ClientCode).Take(1000);
        }

        public IEnumerable<Contacts> GetContactsByEmail(string emailstr)
        {
            return context.Contacts.Where(e => e.Email.Contains(emailstr)).Take(1000);
        }

        public ContactsLimit GetContactsLimit()
        {
            return context.ContactsLimit.FirstOrDefault(e => e.Id == 1);
        }

        public string GetEngagementFromScheduleMail(string ClientCode, DateTime StatementDate)
        {
            var isPostgres = connectionFactory.IsPostgreSql;
            var sql = isPostgres
                ? @"SELECT ""CC"" FROM dbo.""tblScheduledMail"" WHERE ""ClientCode"" = @ClientId AND ""StatementDate"" = @StatementDate"
                : @"SELECT [CC] FROM [tblScheduledMail] WHERE [ClientCode] = @ClientId AND [StatementDate] = @StatementDate";

            var dt = connectionFactory.FillDataTable(sql, new { ClientId = ClientCode, StatementDate });

            if (dt.Rows.Count == 0)
            {
                return "";
            }
            return dt.Rows[0]["CC"].ToString();
        }

        public EngagementTeam GetEngagementTeam(int Id)
        {
            return context.EngagementTeams.FirstOrDefault(e => e.Id == Id);
        }

        public IEnumerable<EngagementTeam> GetEngagementTeams(string ClientCode)
        {
            return context.EngagementTeams.Where(e => e.ClientCode == ClientCode).Take(1000);
        }

        public IEnumerable<EngagementTeam> GetEngagementTeamsWithGroupName(string ClientCode)
        {

            var engagementTeam = (from e in context.EngagementTeams
                                  join g in context.Group on e.GroupId equals g.Id
                                  where e.ClientCode== ClientCode
                                  select new EngagementTeam
                                  {
                                      Id = e.Id,
                                      Designation = e.Designation,
                                      Name = e.Name,
                                      ClientCode = e.ClientCode,
                                      Email = e.Email,
                                      ContactNumber = e.ContactNumber,
                                      GroupName = g.Description
                                  }

                        ).ToList();

            //var serv = (from s in db.Services
            //            join sl in Location on s.id equals sl.id
            //            where sl.id = s.id
            //            select s).ToList();

            return engagementTeam;
            //throw new NotImplementedException();
        }

        public Contacts UpdateContact(Contacts contacts)
        {
            context.Contacts.Update(contacts);
            context.SaveChanges();
            return contacts;
        }

        public EngagementTeam UpdateEngagementTeam(EngagementTeam engagementTeam)
        {
            context.EngagementTeams.Update(engagementTeam);
            context.SaveChanges();
            return engagementTeam;
        }
    }
}
