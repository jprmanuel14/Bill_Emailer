using BillColl_Main.AppDbContext;
using BillColl_Main.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BillColl_Main.Services
{
    public class RemarksSQLRepository : IRemarks
    {
        private readonly myDBContext context;

        public RemarksSQLRepository(myDBContext context)
        {
            this.context = context;
        }

        public ClientRemarks AddRemarks(ClientRemarks clientRemarks)
        {
            context.ClientRemarks.Add(clientRemarks);
            context.SaveChanges();
            return clientRemarks;
        }

        public ClientRemarks GetRemarks(string ClientCode, DateTime StatementDate)
        {
            return context.ClientRemarks.FirstOrDefault(e => e.ClientCode == ClientCode && e.StatementDate == StatementDate);
        }

        public ClientRemarks UpdateRemarks(ClientRemarks clientRemarks)
        {
            context.ClientRemarks.Update(clientRemarks);
            context.SaveChanges();
            return clientRemarks;
        }
    }
}
