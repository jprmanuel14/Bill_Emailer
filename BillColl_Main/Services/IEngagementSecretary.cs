using BillColl_Main.Models;
using System.Collections.Generic;

namespace BillColl_Main.Services
{
    public interface IEngagementSecretary
    {
        int CountBySecretaryPartnerGroup(string secretary, string partner, string group);
        int CountByPartnerGroup(string partner, string group);
        void AddEngagementSecretary(Engagement_Secretary secretary, string actionTaker, string actionTaken);
        void UpdateEngagementSecretary(Engagement_Secretary secretary, string actionTaker, string actionTaken);
        void DeleteEngagementSecretary(string id, string actionTaker, string actionTaken);
    }
}
