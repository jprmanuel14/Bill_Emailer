using BillColl_Main.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BillColl_Main.Services
{
    public interface IContacts
    {
        IEnumerable<Contacts> GetContacts(string ClientCode);
        IEnumerable<Contacts> GetContactsByEmail(string emailstr);
        IEnumerable<EngagementTeam> GetEngagementTeams(string ClientCode);

        IEnumerable<EngagementTeam> GetEngagementTeamsWithGroupName(string ClientCode);

        Contacts GetContact(int Id);
        string GetContactFromScheduleMail(string ClientCode, DateTime StatementDate);

        string GetEngagementFromScheduleMail(string ClientCode, DateTime StatementDate);


        Contacts AddContact(Contacts contacts);
        Contacts UpdateContact(Contacts contacts);
        Contacts DeleteContact(int Id);

        EngagementTeam GetEngagementTeam(int Id);
        EngagementTeam AddEngagementTeam(EngagementTeam engagementTeam);
        EngagementTeam UpdateEngagementTeam(EngagementTeam engagementTeam);
        EngagementTeam DeleteEngagementTeam(int Id);

        ContactsLimit GetContactsLimit();
    }
}
